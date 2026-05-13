using System;
using System.Collections.Generic;
using System.Text;

namespace Proiect_SPRC.Piese
{
    public class Rege : PiesaSah
    {
        public override bool EsteMutareValida(int startX, int startY, int stopX, int stopY, int[,] tabla)
        {
            return Math.Abs(startX - stopX) <= 1 && Math.Abs(startY - stopY) <= 1;
        }
    }
}
