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
        public long Index_Length { get; set; }
        public long Data_Length { get; set; }

        public void To_Bytes_In_Buffer(byte[] buffer, int startIndex)
        {
            Array.Copy(BitConverter.GetBytes(Root_Address), 0, buffer, startIndex, 8);
            Array.Copy(BitConverter.GetBytes(Index_Length), 0, buffer, startIndex + 8, 8);
            Array.Copy(BitConverter.GetBytes(Data_Length), 0, buffer, startIndex + 16, 8);
            Array.Copy(BitConverter.GetBytes(Order), 0, buffer, startIndex + 24, 4);
        }

        public static Metadata From_Bytes(byte[] buffer)
        {
            return new Metadata 
            { 
                 Root_Address = BitConverter.ToInt64(buffer, 0), 
                 Index_Length = BitConverter.ToInt64(buffer, 8), 
                 Data_Length = BitConverter.ToInt64(buffer, 16), 
                 Order= BitConverter.ToInt32(buffer, 24)
            };
        }


    }
}
