using System;
using System.Collections.Generic;
using System.Text;

namespace Proiect_SPRC.Piese
{
    public class Pion : PiesaSah
    {
        public override bool EsteMutareValida(int startX, int startY, int stopX, int stopY, int[,] tabla)
        {
            // Determinăm direcția: Albul (pozitiv) urcă în matrice (i crește), Negrul (negativ) coboară
            int directie = (this.Culoare == "Alb") ? 1 : -1;
            int distantaX = stopX - startX;
            int distantaY = Math.Abs(stopY - startY);

            // 1. Mers înainte un pătrățel
            if (distantaY == 0 && distantaX == directie && tabla[stopX, stopY] == 0)
                return true;

            // 2. Mers înainte două pătrățele (doar de la poziția de start)
            int randStart = (this.Culoare == "Alb") ? 1 : 6;
            if (startX == randStart && distantaY == 0 && distantaX == 2 * directie)
            {
                // Verificăm să nu fie piese în drum
                return tabla[startX + directie, startY] == 0 && tabla[stopX, stopY] == 0;
            }

            // 3. Captură pe diagonală
            if (distantaY == 1 && distantaX == directie && tabla[stopX, stopY] != 0)
                return true;

            return false;
        }
    }
}
