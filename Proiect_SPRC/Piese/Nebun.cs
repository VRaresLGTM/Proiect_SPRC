using System;
using System.Collections.Generic;
using System.Text;

namespace Proiect_SPRC.Piese
{
    public class Nebun : PiesaSah
    {
        public override bool EsteMutareValida(int startX, int startY, int stopX, int stopY, int[,] tabla)
        {
            if (Math.Abs(startX - stopX) != Math.Abs(startY - stopY)) return false;
            return EsteDrumLiber(startX, startY, stopX, stopY, tabla);
        }
    }
}
