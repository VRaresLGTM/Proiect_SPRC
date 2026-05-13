using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace Proiect_SPRC
{
    //Meciul de sah are un cod, pe cei 2 jucatori, (optional) cat timp a trecut din joc, (optional) spectatori, piesele cu locatiile lor  
    internal class MeciSah
    {
        public string lobbyCode { get; set; }
        public TcpClient JucatorAlb { get; set; }
        public TcpClient? JucatorNegru { get; set; }
        public List<TcpClient> Spectatori { get; set; } = new List<TcpClient>();
        public string StareTabla { get; set; }
        public bool EsteActiv { get; set; }
        public MeciSah(string cod, TcpClient creator)
        {
            JucatorAlb = creator;
            lobbyCode = cod;
            EsteActiv = true;
            StareTabla = "4,2,3,5,6,3,2,4,1,1,1,1,1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,-1,-1,-1,-1,-1,-1,-1,-1,-4,-2,-3,-5,-6,-3,-2,-4";
        }
        public bool EstePatraticaAtacata(int x, int y, string culoareRege, string stareString)
        {
            int[,] matrice = Server.ConvertesteInMatrice(stareString);
            string[] valori = stareString.Split(',');

            // Determinăm semnul pieselor adverse
            // Dacă regele e Alb (pozitiv), căutăm piese Negre (negative)
            bool cautamNegative = (culoareRege == "Alb");

            for (int i = 0; i < 64; i++)
            {
                int valoarePiesa = int.Parse(valori[i]);

                if (valoarePiesa != 0)
                {
                    bool estePiesaAdversa = cautamNegative ? (valoarePiesa < 0) : (valoarePiesa > 0);

                    if (estePiesaAdversa)
                    {
                        // Verificăm dacă piesa adversă de la (coloana i%8, rândul i/8) poate ataca (x, y)
                        if (Server.ExecutaValidarePiesa(i % 8, i / 8, x, y, matrice))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public bool ArFiInSahDupaMutare(int sX, int sY, int dX, int dY, string culoareJucator)
        {
            string[] valori = StareTabla.Split(',');

            // Simulăm mutarea în vectorul de string-uri
            valori[dX + dY * 8] = valori[sX + sY * 8];
            valori[sX + sY * 8] = "0";

            string stareSimulata = string.Join(",", valori);
            int[,] matriceSimulata = Server.ConvertesteInMatrice(stareSimulata);

            // Găsim poziția regelui propriu în matricea simulată
            int tintaRege = (culoareJucator == "Alb") ? 6 : -6;
            int regeX = -1, regeY = -1;

            for (int rând = 0; rând < 8; rând++)
            {
                for (int col = 0; col < 8; col++)
                {
                    if (matriceSimulata[rând, col] == tintaRege)
                    {
                        regeX = col;
                        regeY = rând;
                        break;
                    }
                }
            }

            return EstePatraticaAtacata(regeX, regeY, culoareJucator, stareSimulata);
        }

        public string VerificaStareFinala(string culoareJucator)
        {
            int[,] matriceCurenta = Server.ConvertesteInMatrice(StareTabla);
            string[] valori = StareTabla.Split(',');

            // Verificăm dacă jucătorul mai are ORICE mutare legală
            for (int i = 0; i < 64; i++)
            {
                int piesa = int.Parse(valori[i]);
                bool estePiesaMea = (culoareJucator == "Alb") ? (piesa > 0) : (piesa < 0);

                if (estePiesaMea)
                {
                    int sX = i % 8;
                    int sY = i / 8;

                    for (int ty = 0; ty < 8; ty++)
                    {
                        for (int tx = 0; tx < 8; tx++)
                        {
                            if (Server.ExecutaValidarePiesa(sX, sY, tx, ty, matriceCurenta))
                            {
                                if (!ArFiInSahDupaMutare(sX, sY, tx, ty, culoareJucator))
                                {
                                    return "IN_DESFASURARE";
                                }
                            }
                        }
                    }
                }
            }

            // Dacă nu mai are mutări, verificăm dacă este în șah
            int codRege = (culoareJucator == "Alb") ? 6 : -6;
            int rX = -1, rY = -1;

            for (int r = 0; r < 8; r++)
                for (int c = 0; c < 8; c++)
                    if (matriceCurenta[r, c] == codRege) { rX = c; rY = r; break; }

            if (EstePatraticaAtacata(rX, rY, culoareJucator, StareTabla))
            {
                return "SAH_MAT";
            }

            return "REMIZA_PATIN";
        }
    }
}
