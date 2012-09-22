using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BPlusTree.Core
{
    public class Metadata
    {
        public int Order { get; set; }

        public long Root_Address { get; set; }
        public long IndexStream_Length { get; set; }
        public long DataStream_Length { get; set; }

        public bool IsClustered { get; set; }
        public int Data_Length { get; set; }

        public void To_Bytes_In_Buffer(byte[] buffer, int startIndex)
        {
            Array.Copy(BitConverter.GetBytes(Root_Address), 0, buffer, startIndex, 8);
            Array.Copy(BitConverter.GetBytes(IndexStream_Length), 0, buffer, startIndex + 8, 8);
            Array.Copy(BitConverter.GetBytes(DataStream_Length), 0, buffer, startIndex + 16, 8);
            Array.Copy(BitConverter.GetBytes(Order), 0, buffer, startIndex + 24, 4);
            buffer[startIndex + 28] = IsClustered ? (byte)1 : (byte)0;
            Array.Copy(BitConverter.GetBytes(Data_Length), 0, buffer, startIndex + 29, 4);
        }

        public static Metadata From_Bytes(byte[] buffer)
        {
            return new Metadata 
            { 
                 Root_Address = BitConverter.ToInt64(buffer, 0), 
                 IndexStream_Length = BitConverter.ToInt64(buffer, 8), 
                 DataStream_Length = BitConverter.ToInt64(buffer, 16), 
                 Order= BitConverter.ToInt32(buffer, 24),

                 IsClustered = buffer[28] == 1, 
                 Data_Length = BitConverter.ToInt32(buffer, 29)
            };
        }


    }
}
