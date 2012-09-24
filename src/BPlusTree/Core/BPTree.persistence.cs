using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace BPlusTree.Core
{
    public partial class BPlusTree<T> : IData_Reader<T>
    {
        public DateTime Current_Time;

        private long _index_Pointer;
        private long _data_Pointer;

        private long Data_Pointer()
        {
            return _data_Pointer;
        }


        protected void Write_Node(Node<T> node)
        {
            _pending_Changes.Append_Node(node);
        }

        protected void Renew_Node_And_Dispose_Space(Node<T> node)
        {
            _pending_Changes.Free_Address(node.Address);
            node.Is_Volatile = true;
            node.Address = 0;
        }

        protected Node<T> Read_Root(long address)
        {
            var buffer = new byte[_block_Size];

            Index_Stream.Seek(address, SeekOrigin.Begin);
            Index_Stream.Read(buffer, 0, buffer.Length);

            var node = _node_Factory.From_Bytes(buffer, Order);
            node.Address = address;
            return node;
        }

        public Node<T> Read_Node_From_Parent_Pointer(Node<T> parent, int key_Index)
        {
             long address = parent.Pointers[key_Index];
             
             var node = _cache.Get(address);
             //var node = Read_Node(address);
             if (node == null)
             {
                 node = Read_Node(address);
                 _cache.Put(address, node);
             }
             else
             {
                 node = _node_Factory.Create_New_One_Detached_Like_This(node);
             }


             node.Is_Volatile = false;
             node.Parent = parent;
             node.Address = address;
             parent.Children[key_Index] = node;  // TODO it gets the index in memory with time.
             return node;
        }

        protected Node<T> Read_Node(long address)
        {
            var buffer = new byte[_block_Size];

            Index_Stream.Seek(address, SeekOrigin.Begin);
            Index_Stream.Read(buffer, 0, buffer.Length);

            return _node_Factory.From_Bytes(buffer, Order);
        }



        protected void Write_Data(byte[] value, T key, int version, long address)
        {
            var data = new Data<T>
            { 
                Key = key, 
                Version = version, 
                Payload= value, 
                Timestamp = Current_Time,
                Address = address
            };

            _pending_Changes.Append_Data(data);
        }

        protected byte[] Read_Data(long address)
        {
            var reader = this as IData_Reader<T>;
            Data<T> data = reader.Read_Data(address);
            return data.Payload;
        }




        Data<T> IData_Reader<T>.Read_Data(long address)
        {
            Data<T> data = null;
            if (_pending_Changes.Has_Pending_Changes())
            {
                data = _pending_Changes.Get_Pending_Data().SingleOrDefault(d => d.Address == address);
                if (data != null)
                    return data;
            }

            Data_Stream.Seek(address, SeekOrigin.Begin);
            return _data_Serializer.From_Bytes(Data_Stream);
        }
    }


}
