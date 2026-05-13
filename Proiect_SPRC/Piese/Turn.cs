using System;
using System.Collections.Generic;
using System.Text;

namespace Proiect_SPRC.Piese
{
    public class Turn : PiesaSah
    {
        public override bool EsteMutareValida(int startX, int startY, int stopX, int stopY, int[,] tabla)
        {
            if (startX != stopX && startY != stopY) return false;
            return EsteDrumLiber(startX, startY, stopX, stopY, tabla);
        }
    }
}
