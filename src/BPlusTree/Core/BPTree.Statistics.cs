using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BPlusTree.Core
{
    public partial class BPlusTree<T>
    {
        public long cache_hits;
        public long cache_misses;


        public int commitsCount;
        public int writes;

        public Usage Count_Empty_Slots()
        {
            int invalid = 0;
            int valid = 0;
            int blockSize = _node_Factory.Size_In_Bytes(3);
            long position = Index_Stream.Position;

            Index_Stream.Seek(8, SeekOrigin.Begin);
            var buffer = new byte[blockSize];
            while (Index_Stream.Read(buffer, 0, buffer.Length) > 0)
            {
                var node = _node_Factory.From_Bytes(buffer, 3);
                if (node.IsValid)
                    valid++;
                else
                    invalid++;
            }

            int used = valid * blockSize;
            int wasted = invalid * blockSize;

            Index_Stream.Seek(position, SeekOrigin.Begin);
            return new Usage { Invalid = invalid, Valid = valid };
        }

        public void Mark_As_Invalid(Stream index, long block_Address)
        {
            Index_Stream.Seek(block_Address, SeekOrigin.Begin);
            Index_Stream.Write(BitConverter.GetBytes(-1), 0, 4);
        }


        int key_added;
        public void Key_Added() { key_added++; }
        int nodes_added;
        public void Node_Added() { nodes_added++; }
        int leaves_added;
        public void Leave_Added() { leaves_added++; }
    }

    public class Usage
    {
        public int Invalid { get; set; }
        public int Valid { get; set; }

        public override string ToString()
        {
            return string.Format("Valid : {0}, Invalid : {1}", Valid, Invalid);
        }
    }
}
