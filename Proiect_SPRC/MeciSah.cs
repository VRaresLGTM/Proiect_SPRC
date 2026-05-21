using System.Net.Sockets;

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
        
    }
}
