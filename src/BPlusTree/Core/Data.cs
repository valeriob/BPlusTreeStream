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
        public long Previous_Version_Address { get; set; }
        public byte[] Payload { get; set; }

        public long Address { get; set; }
        

        public void Write_To_Buffer(ISerializer<T> serializer, byte[] buffer, int startIndex, int alignment)
        {
            int size = Total_Persisted_Size(serializer.Serialized_Size_For_Single_Key_In_Bytes(), alignment);

            Array.Copy(BitConverter.GetBytes(size), 0, buffer, startIndex, 4);
            Array.Copy(serializer.GetBytes(Key), 0, buffer, startIndex + 4, serializer.Serialized_Size_For_Single_Key_In_Bytes());
            Array.Copy(BitConverter.GetBytes(Timestamp.Ticks), 0, buffer, startIndex + 8, 8);
            Array.Copy(BitConverter.GetBytes(Version), 0, buffer, startIndex + 16, 4);
            Array.Copy(BitConverter.GetBytes(Previous_Version_Address), 0, buffer, startIndex + 20, 8);
            Array.Copy(BitConverter.GetBytes(Payload.Length), 0, buffer, startIndex + 28, 4);
            Array.Copy(Payload, 0, buffer, startIndex + 32, Payload.Length);
        }

        public int Total_Persisted_Size(int keySize, int alignment)
        {
            var size = 4 + 8 + 4 + 8 + 4 + Payload.Length + keySize;
            if (alignment > 0)
            {
                var rest = size % alignment;
                if (rest != 0)
                    size = size + alignment - rest;
            }
            return size;
        }



        static int read_ahead = 2;
        public static Data<T> From_Bytes(Stream stream, ISerializer<T> serializer, int alignment)
        {
            int bufferSize = alignment * (1 + read_ahead);
            if (alignment == 0)
                bufferSize = 128;

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
                Previous_Version_Address = BitConverter.ToInt64(buffer, startIndex +16),
            };

            int payload_Length = BitConverter.ToInt32(buffer, startIndex + 24);

            data.Payload = new byte[payload_Length];
            Array.Copy(buffer, startIndex + 28, data.Payload, 0, payload_Length);

            return data;
        }


    }


    public class Clustered_Data<T> where T : IComparable<T>, IEquatable<T>
    {
        public DateTime Timestamp { get; protected set; }
        public int Version { get; protected set; }
        public byte[] Payload { get; protected set; }

        public Clustered_Data(int version, DateTime timestamp, byte[] payload)
        {
            Version = version;
            Timestamp = timestamp;
            Payload = payload;
        }


        public void Write_To_Buffer(byte[] buffer, int startIndex)
        {
            Array.Copy(BitConverter.GetBytes(Timestamp.Ticks), 0, buffer, startIndex, 8);
            Array.Copy(BitConverter.GetBytes(Version), 0, buffer, startIndex + 8, 4);
            Array.Copy(Payload, 0, buffer, startIndex + 12, Payload.Length);
        }


        public static Clustered_Data<T> From_Bytes(byte[] buffer, int startIndex, int payload_Length)
        {
            var timestamp = DateTime.FromBinary(BitConverter.ToInt64(buffer, startIndex ));
            var version = BitConverter.ToInt32(buffer, startIndex + 8);
            var payload = new byte[payload_Length - 12];

            Array.Copy(buffer, startIndex + 12, payload, 0, payload.Length);
            
           return new Clustered_Data<T>(version, timestamp, payload);
        }


        public override string ToString()
        {
            var str = Encoding.UTF8.GetString(Payload);
            return str.Substring(0, 20);
        }

    }

}
