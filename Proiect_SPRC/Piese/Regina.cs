using System;
using System.Collections.Generic;
using System.Text;

namespace Proiect_SPRC.Piese
{
    public class Regina : PiesaSah
    {
        public override bool EsteMutareValida(int startX, int startY, int stopX, int stopY, int[,] tabla)
        {
            bool esteDiagonal = Math.Abs(startX - stopX) == Math.Abs(startY - stopY);
            bool esteLinieSauColoana = (startX == stopX || startY == stopY);

            if (!esteDiagonal && !esteLinieSauColoana) return false;
            return EsteDrumLiber(startX, startY, stopX, stopY, tabla);
        }
    }
}
