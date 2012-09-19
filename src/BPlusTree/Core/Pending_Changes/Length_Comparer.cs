using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BPlusTree.Core.Pending_Changes
{
    public class Length_Comparer : IComparer<Block_Group>
    {
        public long Length { get; set; }
        public Length_Comparer(long length)
        {
            Length = length;
        }

        public int Compare(Block_Group x, Block_Group y)
        {
            long dx = Length - x.Length;
            long dy = Length - y.Length;

            if (dx == 0 && dy == 0)
                return 0;

            if (dx == 0)
                return -1;
            if (dy == 0)
                return 1;

            if (dx < 0 && dy < 0)
                return Math.Sign(dy - dx);

            if (dx > 0 && dy > 0)
                return Math.Sign(dx - dy);

            if (dx > 0 && dy < 0)
                return 1;

            if (dx < 0 && dy > 0)
                return -1;

            return 0;
        }
    }
}
