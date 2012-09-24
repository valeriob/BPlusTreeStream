using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BPlusTree.Core.Serializers
{
    public class Null_Serializer : ISerializer<byte[]>
    {
        public byte[] Get_Bytes(byte[] instance)
        {
            return instance;
        }

        public byte[] Get_Instance(byte[] bytes, int startIndex)
        {
            return bytes;
        }
    }
}
