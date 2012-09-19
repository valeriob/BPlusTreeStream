
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BPlusTree.Core.Pending_Changes
{
    public struct Block_Group
    {
        public int Length { get; set; }
        public Dictionary<long, Block> Blocks { get; set; }

        public override string ToString()
        {
            return string.Format("Length {0}, # {1}", Length, Blocks.Count);
        }
    }
}
