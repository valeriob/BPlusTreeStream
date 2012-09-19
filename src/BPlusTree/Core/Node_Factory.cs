using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BPlusTree.Core
{
    public class Node_Factory<T> where T : IComparable<T>, IEquatable<T> // TODO DISPOSE TASK
    {
        ISerializer<T> Serializer;
        Task builder;
        int Size;
        ManualResetEvent _lock = new ManualResetEvent(true);

        public Node_Factory(ISerializer<T> serializer, int size)
        {
            Serializer = serializer;
            Size = size;
            builder = Task.Factory.StartNew(Build, TaskCreationOptions.LongRunning);
        }

        System.Collections.Concurrent.ConcurrentQueue<Node<T>> nodes = new System.Collections.Concurrent.ConcurrentQueue<Node<T>>();

        public Node<T> Create_New(bool isLeaf)
        {
            Node<T> node;
            if (nodes.TryDequeue(out node))
            {
                if (nodes.Count < 50)
                    _lock.Set();

                node.IsLeaf = isLeaf;
                node.Is_Volatile = true;
                return node;
            }

            return new Node<T>(this, Size, isLeaf);
        }

        protected void Build()
        {
            while (true)
            {
                _lock.WaitOne();

                for (int i = 0; i < 50; i++)
                    nodes.Enqueue(new Node<T>(this, Size, false));

                _lock.Reset();
            }
        }

        public void Return(Node<T> node)
        {
            if (nodes.Count < 1024)
                nodes.Enqueue(node);
        }

        unsafe public Node<T> From_Bytes(byte[] buffer, int size)
        {
            var byteCount = Size_In_Bytes(size);
            var keySize = Serializer.Serialized_Size_For_Single_Key_In_Bytes();

            int key_Num;
            fixed (byte* p_buff = &buffer[0])
            {
                byte* shifted = p_buff ;
                Unsafe_Utilities.Memcpy((byte*)&key_Num, shifted, 4);
            }

            var node = Create_New(buffer[4] == 1);
           
            node.Key_Num = key_Num;

            node.Keys = Serializer.Get_Instances(buffer, 5, size); 

            int offset = 5 + keySize * size;

            fixed (long* p_Pointers = &node.Pointers[0])
            fixed (byte* p_buff = &buffer[0])
            {
                byte* shifted = p_buff + offset;
                Unsafe_Utilities.Memcpy((byte*)p_Pointers, shifted, 8 * (key_Num + 1));
            }
            return node;
        }

        public Node<T> From_Bytes_Safe(byte[] buffer, int size)
        {
            var byteCount = Size_In_Bytes(size);
            var keySize = Serializer.Serialized_Size_For_Single_Key_In_Bytes();

            var node = Create_New(BitConverter.ToBoolean(buffer, 4));
            var key_Num = BitConverter.ToInt32(buffer, 0);

            node.Key_Num = key_Num;

            for (int i = 0; i < key_Num; i++)
                node.Keys[i] = Serializer.Get_Instance(buffer, 5 + keySize * i);

            int offset = 5 + keySize * size;
            for (int i = 0; i < key_Num + 1; i++)
                node.Pointers[i] = BitConverter.ToInt64(buffer, offset + 8 * i);

            return node;
        }

        unsafe public void To_Bytes_In_Buffer(Node<T> node, byte[] buffer, int startIndex)
        {
            int key_Num = node.Key_Num;
            int keySize = Serializer.Serialized_Size_For_Single_Key_In_Bytes();

            fixed (byte* p_buff = &buffer[0])
            {
                byte* shifted = p_buff + startIndex;
                Unsafe_Utilities.Memcpy(shifted, (byte*)&key_Num, 4);
            }

            buffer[startIndex + 4] = node.IsLeaf ? (byte)1 : (byte)0;

            int offset = startIndex + 5;
            Serializer.To_Buffer(node.Keys, key_Num, buffer, offset);

            offset = startIndex + 5 + keySize * node.Keys.Length;
            fixed (long* p_Pointers = &node.Pointers[0])
            fixed (byte* p_buff = &buffer[0])
            {
                byte* shifted = p_buff + offset;
                Unsafe_Utilities.Memcpy(shifted, (byte*)p_Pointers, 8 * (key_Num + 1));
            }
        }

        public void To_Bytes_In_Buffer_Safe(Node<T> node, byte[] buffer, int startIndex)
        {
            int key_Num = node.Key_Num;
            int keySize = Serializer.Serialized_Size_For_Single_Key_In_Bytes();

            Array.Copy(BitConverter.GetBytes(key_Num), 0, buffer, startIndex, 4);

            buffer[startIndex + 4] = node.IsLeaf ? (byte)1 : (byte)0;

            int offset = startIndex + 5;
            for (int i = 0; i < key_Num; i++)
                Array.Copy(Serializer.GetBytes(node.Keys[i]), 0, buffer, offset + keySize * i, keySize);

            offset = startIndex + 5 + keySize * node.Keys.Length;
            for (int i = 0; i < key_Num + 1; i++)
                Array.Copy(BitConverter.GetBytes(node.Pointers[i]), 0, buffer, offset + i * 8, 8);
        }


        public Node<T> Create_New_One_Like_This(Node<T> source)
        {
            var node = Create_New(source.IsLeaf);
            node.Key_Num = source.Key_Num;
            Array.Copy(source.Keys, node.Keys, source.Keys.Length);
            Array.Copy(source.Pointers, node.Pointers, source.Pointers.Length);
            Array.Copy(source.Versions, node.Versions, source.Versions.Length);
            Array.Copy(source.Children, node.Children, source.Children.Length);
            node.Parent = source.Parent;
            node.Address = source.Address;
            return node;
        }
        public Node<T> Create_New_One_Detached_Like_This(Node<T> source)
        {
            var node = Create_New(source.IsLeaf);
            node.Key_Num = source.Key_Num;
            Array.Copy(source.Keys, node.Keys, source.Keys.Length);
            Array.Copy(source.Pointers, node.Pointers, source.Pointers.Length);
            Array.Copy(source.Versions, node.Versions, source.Versions.Length);
            node.Address = source.Address;
            return node;
        }


        int alignment = 512;
        public int Size_In_Bytes(int order)
        {
            int net_size =  4 + 1 + Serializer.Serialized_Size_For_Single_Key_In_Bytes() * order + 8 * (order + 1);
            var rest = net_size % alignment;
            if (rest != 0)
                return net_size + alignment - rest;
            return net_size;
        }
    }

}
