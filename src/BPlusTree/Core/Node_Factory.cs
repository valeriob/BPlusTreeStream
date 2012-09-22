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
        public ISerializer<T> Serializer { get; protected set; }
        Task builder;
        int Order;
        bool IsClustered;
        bool IsAligned;
        int Clustered_Data_Size;
        public int Alignment { get; protected set; }
        ManualResetEvent _lock = new ManualResetEvent(true);

        public Node_Factory(ISerializer<T> serializer, int order, int alignment, int clusteredDataSize)
        {
            Serializer = serializer;
            Order = order;
            Alignment = alignment;
            IsClustered = clusteredDataSize > 0;
            IsAligned = alignment > 0;
            Clustered_Data_Size = clusteredDataSize;
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

            return new Node<T>(this, Order, isLeaf, IsClustered, Clustered_Data_Size);
        }

        protected void Build()
        {
            while (true)
            {
                _lock.WaitOne();

                for (int i = 0; i < 50; i++)
                    nodes.Enqueue(new Node<T>(this, Order, false, IsClustered, Clustered_Data_Size));

                _lock.Reset();
            }
        }

        public void Return(Node<T> node)
        {
            if (nodes.Count < 1024)
                nodes.Enqueue(node);
        }

        unsafe public Node<T> From_Bytes(byte[] buffer, int order)
        {
            var byteCount = Size_In_Bytes(order);
            var keySize = Serializer.Serialized_Size_For_Single_Key_In_Bytes();

            int key_Num;
            fixed (byte* p_buff = &buffer[0])
            {
                byte* shifted = p_buff ;
                Unsafe_Utilities.Memcpy((byte*)&key_Num, shifted, 4);
            }

            bool isLeaf = buffer[4] == 1;
            var node = Create_New(isLeaf);
           
            node.Key_Num = key_Num;

            node.Keys = Serializer.Get_Instances(buffer, 5, order); 

            int offset = 5 + keySize * order;

            fixed (long* p_Pointers = &node.Pointers[0])
            fixed (byte* p_buff = &buffer[0])
            {
                byte* shifted = p_buff + offset;
                Unsafe_Utilities.Memcpy((byte*)p_Pointers, shifted, 8 * (key_Num + 1));
            }

            if (IsClustered && node.IsLeaf)
            {
                offset += 8 * (node.Pointers.Length);

                fixed (byte* p_Data = &node.Data[0,0])
                fixed (byte* p_buff = &buffer[0])
                {
                    byte* shifted = p_buff + offset;
                    Unsafe_Utilities.Memcpy(p_Data, shifted, Clustered_Data_Size * (key_Num + 1));
                }
                //for (int i = 1; i < key_Num + 1; i++)
                //     node.Data[i] = Clustered_Data<T>.From_Bytes(buffer, offset + i * Clustered_Data_Size, Clustered_Data_Size);
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

            offset += keySize * node.Keys.Length;
            fixed (long* p_Pointers = &node.Pointers[0])
            fixed (byte* p_buff = &buffer[0])
            {
                byte* shifted = p_buff + offset;
                Unsafe_Utilities.Memcpy(shifted, (byte*)p_Pointers, 8 * (key_Num + 1));
            }

            if (IsClustered && node.IsLeaf)
            {
                offset += 8 * (node.Pointers.Length);

                fixed (byte* p_Data = &node.Data[0, 0])
                fixed (byte* p_buff = &buffer[0])
                {
                    byte* shifted = p_buff + offset;
                    Unsafe_Utilities.Memcpy(shifted, p_Data, Clustered_Data_Size * (key_Num + 1));
                }

                //for (int i = 1; i < key_Num + 1; i++)
                //    node.Data[i].Write_To_Buffer(buffer, offset + i * Clustered_Data_Size);
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
            Array.Copy(source.Data, node.Data, source.Data.Length);
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
            Array.Copy(source.Data, node.Data, source.Data.Length);
            node.Address = source.Address;
            return node;
        }


       
        public int Size_In_Bytes(int order)
        {
            int keySize = Serializer.Serialized_Size_For_Single_Key_In_Bytes();

            int net_size = 4 + 1 + keySize * order + 8 * (order + 1);

            if (IsClustered)
                net_size += Clustered_Data_Size * (order + 1);

            if (!IsAligned)
                return net_size;

            var rest = net_size % Alignment;
            if (rest != 0)
                return net_size + Alignment - rest;
            return net_size;
        }
    }

}
