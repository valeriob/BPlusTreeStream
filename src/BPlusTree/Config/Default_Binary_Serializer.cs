using BPlusTree.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace BPlusTree.Config
{
    public interface ISerializer<T>
    { 
        byte[] Get_Bytes(T instance);
        T Get_Instance(byte[] bytes);
    }

    public class Default_Binary_Serializer<T> : ISerializer<T>
    {
        BinaryFormatter _formatter = new BinaryFormatter();

        public byte[] Get_Bytes(T instance)
        {
            using (var buffer = new MemoryStream())
            {
                _formatter.Serialize(buffer, instance);
                buffer.Seek(0, SeekOrigin.Begin);
                return buffer.GetBuffer();
            }
        }

        public T Get_Instance(byte[] bytes)
        {
            using (var buffer = new MemoryStream(bytes))
            {
                return (T)_formatter.Deserialize(buffer);
            }
        }
    }
}
