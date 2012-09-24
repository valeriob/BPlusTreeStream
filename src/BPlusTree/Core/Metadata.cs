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


        public int Clustered_Data_Length { get; set; }
        public int Alignment { get; set; }

        // STATS
        public int Number_Of_Keys { get; set; }
        public int Number_Of_Leafes { get; set; }
        public int Number_Of_Nodes { get; set; }

        public void To_Bytes_In_Buffer(byte[] buffer, int startIndex)
        {
            Array.Copy(BitConverter.GetBytes(Order), 0, buffer, startIndex, 4);

            Array.Copy(BitConverter.GetBytes(Root_Address), 0, buffer, startIndex + 4, 8);
            Array.Copy(BitConverter.GetBytes(IndexStream_Length), 0, buffer, startIndex + 12, 8);
            Array.Copy(BitConverter.GetBytes(DataStream_Length), 0, buffer, startIndex + 20, 8);

            Array.Copy(BitConverter.GetBytes(Clustered_Data_Length), 0, buffer, startIndex + 28, 4);
            Array.Copy(BitConverter.GetBytes(Alignment), 0, buffer, startIndex + 32, 4);
        }

        public static Metadata From_Bytes(byte[] buffer)
        {
            return new Metadata 
            {
                Order = BitConverter.ToInt32(buffer, 0),
                Root_Address = BitConverter.ToInt64(buffer, 4), 
                IndexStream_Length = BitConverter.ToInt64(buffer, 12), 
                DataStream_Length = BitConverter.ToInt64(buffer, 20), 

                Clustered_Data_Length = BitConverter.ToInt32(buffer, 28),
                Alignment = BitConverter.ToInt32(buffer, 32)
            };
        }


    }
}
