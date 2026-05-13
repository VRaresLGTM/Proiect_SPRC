using System;
using System.Collections.Generic;
using System.Text;

namespace Proiect_SPRC.Piese
{
    public class Cal : PiesaSah
    {
        public override bool EsteMutareValida(int startX, int startY, int stopX, int stopY, int[,] tabla)
        {
            int dx = Math.Abs(startX - stopX);
            int dy = Math.Abs(startY - stopY);

            return (dx == 2 && dy == 1) || (dx == 1 && dy == 2);
        }
    }
}
