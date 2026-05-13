using System;
using System.Collections.Generic;
using System.Text;

namespace Proiect_SPRC.Piese
{
    public abstract class PiesaSah
    {
        public string Culoare { get; set; }

        public abstract bool EsteMutareValida(int startX, int startY, int stopX, int stopY, int[,] tabla);
        protected bool EsteDrumLiber(int startX, int startY, int stopX, int stopY, int[,] tabla)
        {
            int pasX = Math.Sign(stopX - startX);
            int pasY = Math.Sign(stopY - startY);

            int curentX = startX + pasX;
            int curentY = startY + pasY;

            while (curentX != stopX || curentY != stopY)
            {
                if (tabla[curentX, curentY] != 0) return false; // Am găsit un obstacol

                if (curentX != stopX) curentX += pasX;
                if (curentY != stopY) curentY += pasY;
            }
            return true;
        }
    }
}
