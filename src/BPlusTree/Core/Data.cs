using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BPlusTree.Core
{
    public class Data<T> where T : IComparable<T>, IEquatable<T>
    {
        public T Key { get; set; }
        public DateTime Timestamp { get; set; }
        public int Version { get; set; }
        public byte[] Payload { get; set; }

        public long Address { get; set; }
        //public Node<T> Parent { get; set; }


        static int alignment = 512;
        public byte[] To_Bytes(ISerializer<T> serializer)
        {
            var size = Total_Persisted_Size(serializer.Serialized_Size_For_Single_Key_In_Bytes());

            var buffer = new byte[size];

            Write_To_Buffer(serializer, buffer, 0);

            return buffer;
        }

        public void Write_To_Buffer(ISerializer<T> serializer, byte[] buffer, int startIndex)
        {
            int size = Total_Persisted_Size(serializer.Serialized_Size_For_Single_Key_In_Bytes());

            Array.Copy(BitConverter.GetBytes(size), 0, buffer, startIndex, 4);
            Array.Copy(serializer.GetBytes(Key), 0, buffer, startIndex + 4, serializer.Serialized_Size_For_Single_Key_In_Bytes());
            Array.Copy(BitConverter.GetBytes(Timestamp.Ticks), 0, buffer, startIndex + 8, 8);
            Array.Copy(BitConverter.GetBytes(Version), 0, buffer, startIndex + 16, 4);
            Array.Copy(BitConverter.GetBytes(Payload.Length), 0, buffer, startIndex + 20, 4);
            Array.Copy(Payload, 0, buffer, startIndex + 24, Payload.Length);
        }

        public int Total_Persisted_Size(int keySize)
        {
            var size = 4 + 8 + 4 + 4 + Payload.Length + keySize;
            var rest = size % alignment;
            if (rest != 0)
                size = size + alignment - rest;

            return size;
        }



        static int read_ahead;
        public static Data<T> From_Bytes(Stream stream, ISerializer<T> serializer)
        {
            int bufferSize = alignment * (1 + read_ahead);
            var buffer = new byte[bufferSize];
            stream.Read(buffer, 0, buffer.Length);

            int total_Length = BitConverter.ToInt32(buffer, 0);

            if (bufferSize < total_Length)
            {
                Array.Resize(ref buffer, total_Length);

                buffer = new byte[total_Length];
                stream.Read(buffer, bufferSize, total_Length - bufferSize);
            }

            return From_Bytes(buffer, 4, serializer);
        }

        public static Data<T> From_Bytes(byte[] buffer, int startIndex, ISerializer<T> serializer)
        {
            var data = new Data<T>
            {
                Key = serializer.Get_Instance(buffer, startIndex),
                Timestamp = DateTime.FromBinary(BitConverter.ToInt64(buffer, startIndex + 4)),
                Version = BitConverter.ToInt32(buffer, startIndex + 12),
             //   Payload = new byte[payload_Length]
            };

            int payload_Length = BitConverter.ToInt32(buffer, startIndex + 16);

            data.Payload = new byte[payload_Length];
            Array.Copy(buffer, startIndex + 20, data.Payload, 0, payload_Length);

            return data;
        }


    }
}
