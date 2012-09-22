using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace BPlusTree.Core
{
    public partial class BPlusTree<T>
    {
        private long _index_Pointer;
        private long _data_Pointer;

        private long Data_Pointer()
        {
            return _data_Pointer;
        }


        protected void Write_Node(Node<T> node)
        {
            Pending_Changes.Append_Node(node);
        }

        protected void Renew_Node_And_Dispose_Space(Node<T> node)
        {
            Pending_Changes.Free_Address(node.Address);
            node.Is_Volatile = true;
            node.Address = 0;
        }

        protected Node<T> Read_Root(long address)
        {
            var buffer = new byte[Block_Size];

            Index_Stream.Seek(address, SeekOrigin.Begin);
            Index_Stream.Read(buffer, 0, buffer.Length);

            var node = Node_Factory.From_Bytes(buffer, Size);
            node.Address = address;
            return node;
        }

        protected Node<T> Read_Node_From_Pointer(Node<T> parent, int key_Index)
        {
             long address = parent.Pointers[key_Index];
             
             var node = Cache.Get(address);
             if (node == null)
             {
                 node = Read_Node(address);
                 Cache.Put(address, node);
             }
             else
             {
                 node = Node_Factory.Create_New_One_Detached_Like_This(node);
             }
             //var node = Read_Node(address);

             node.Is_Volatile = false;
             node.Parent = parent;
             node.Address = address;
             parent.Children[key_Index] = node;
             return node;
        }

        protected Node<T> Read_Node(long address)
        {
            var buffer = new byte[Block_Size];

            Index_Stream.Seek(address, SeekOrigin.Begin);
            Index_Stream.Read(buffer, 0, buffer.Length);

            return Node_Factory.From_Bytes(buffer, Size);
        }



        protected void Write_Data(Node<T> leaf, byte[] value, T key, int version, long address)
        {
            var data = new Data<T>
            { 
                Key = key, 
                Version = version, 
                Payload= value, 
                Timestamp = DateTime.Now, 
                //Parent = leaf, 
                Address = address
            };

            Pending_Changes.Append_Data(data);

            //var address = Data_Pointer();
            //Data_Stream.Seek(address, SeekOrigin.Begin);

            //var bytes = data.To_Bytes(Serializer);
            //Data_Stream.Write(bytes, 0, bytes.Length);

            //_data_Pointer += bytes.Length;
        }

        protected byte[] Read_Data(long address)
        {
            Data<T> data = null;
            if(Pending_Changes.Has_Pending_Changes())
            {
                data = Pending_Changes.Get_Pending_Data().SingleOrDefault(d=> d.Address == address);
                if(data!= null)
                    return data.Payload;
            }
            Data_Stream.Seek(address, SeekOrigin.Begin);

            data = Data<T>.From_Bytes(Data_Stream, Serializer);
            return data.Payload;
        }


       
    }


}
