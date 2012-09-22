using BPlusTree.Core.Pending_Changes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Timers;

namespace BPlusTree.Core
{
    public partial class BPlusTree<T> : IBPlusTree<T> where T: IComparable<T>, IEquatable<T>
    {
        public Node<T> Root { get; protected set; }
        IPending_Changes<T> Pending_Changes;

        protected Stream Index_Stream { get; set; }
        protected Stream Data_Stream { get; set; }
        protected Stream Metadata_Stream { get; set; }
        
        protected int Size { get; set; }
        protected int Alignment { get; set; }
        protected int Cluster_Data_Length;
        public bool IsClustered() { return Cluster_Data_Length > 0; }

        public Dictionary<long, Node<T>> Cached_Nodes { get; set; }

        public int Block_Size { get; protected set; }
        ISerializer<T> Serializer;
        Node_Factory<T> Node_Factory;
        Cache_LRU<long, Node<T>> Cache;
        IStream_Factory StreamFactory;
        Metadata Metadata;
        

        public BPlusTree(IStream_Factory stramFactory, int order, ISerializer<T> serializer)
        {
            Index_Stream = StreamFactory.Create_ReadWrite_Index_Stream();
            Data_Stream = StreamFactory.Create_ReadWrite_Data_Stream();

        }
        public BPlusTree(Stream metadataStream, Stream indexStream, Stream dataStream, int order, int alignment, int cluster_data_length, ISerializer<T> serializer)
        {
            Size = order;
            Alignment = alignment;
            Cluster_Data_Length = cluster_data_length;

            Node_Factory = new Node_Factory<T>(serializer, Size, alignment, cluster_data_length);
            Block_Size = Node_Factory.Size_In_Bytes(Size);
            Pending_Changes = new Pending_Changes<T>(Block_Size, _index_Pointer, _data_Pointer, Node_Factory);

            Serializer = serializer;

            Index_Stream = indexStream;
            Data_Stream = dataStream;
            Metadata_Stream = metadataStream;

            Cached_Nodes = new Dictionary<long, Node<T>>();
            Cache = new Cache_LRU<long, Node<T>>();
            Metadata = new Metadata { Order = order };

            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                while (true)
                    lock (this)
                    {
                        Current_Time = DateTime.Now;
                        System.Threading.Monitor.Wait(this, 100);
                    }
            });
            // TODO, from metadata
            //_index_Pointer = indexStream.Length; // Math.Max(8, indexStream.Length);
            //_data_Pointer = Data_Stream.Length;

            Init();
        }



        public void Commit()
        {
            if (!Pending_Changes.Has_Pending_Changes())
                return;
            foreach (var address in Pending_Changes.Get_Freed_Empty_Slots())
                Cache.Invalidate(address);

            var newRoot = Pending_Changes.Commit(Index_Stream, Data_Stream);

            writes++;
            Metadata_Stream.Seek(0, SeekOrigin.Begin);
            var buffer = new byte[512];
            Metadata.DataStream_Length = Pending_Changes.Get_Current_Data_Pointer();
            Metadata.IndexStream_Length = Pending_Changes.Get_Index_Pointer();
            Metadata.Root_Address = newRoot.Address;
            Metadata.Clustered_Data_Length = Cluster_Data_Length;
            Metadata.Alignment = Alignment;

            Metadata.To_Bytes_In_Buffer(buffer, 0);
            Metadata_Stream.Write(buffer, 0, buffer.Length);
            Metadata_Stream.Flush();

            Root = newRoot;

            Pending_Changes.Clean_Root();

            // TODO persist empty pages on metadata
            //Cached_Nodes.Clear();
            //foreach (var node in Pending_Changes.Last_Cached_Nodes())
            //    Cached_Nodes[node.Address] = node;

            _index_Pointer = Pending_Changes.Get_Index_Pointer();

            Index_Stream.Flush();
        }

        public void RollBack()
        {
            throw new NotImplementedException("RollBack");
        }

        private void Init()
        {
            try
            {

                var buffer = new byte[512];
                Metadata_Stream.Seek(0, SeekOrigin.Begin);
                Metadata_Stream.Read(buffer, 0, buffer.Length);

                var metadata = Metadata.From_Bytes(buffer);

                Root = Read_Root(metadata.Root_Address);
                if (Root.IsValid)
                {
                    Size = metadata.Order;
                    _data_Pointer = metadata.DataStream_Length;
                    _index_Pointer = metadata.IndexStream_Length;
                    Cluster_Data_Length = metadata.Clustered_Data_Length;
                    Alignment = metadata.Alignment;

                    Node_Factory = new Node_Factory<T>(Serializer, Size,Alignment, Cluster_Data_Length);
                    Block_Size = Node_Factory.Size_In_Bytes(Size);
                    Pending_Changes = new Pending_Changes<T>(Block_Size, _index_Pointer, _data_Pointer, Node_Factory);
                    Metadata = metadata;
                    return;
                }
            }
            catch (Exception) { }

            
            var root = Node_Factory.Create_New(true);
            Write_Node(root);
            Pending_Changes.Append_New_Root(root);
        }


        public byte[] Get(T key)
        {
            var leaf = Find_Leaf_Node(key);

            if (!IsClustered())
            {
                long address = leaf.Get_Data_Address(key);
                return Read_Data(address);
            }
            else
                return leaf.Get_Clustered_Data(key);            
        }

        public void Put(T key, byte[] value)
        {
            var leaf = Find_Leaf_Node(key);

            int index = Array.BinarySearch(leaf.Keys,0, leaf.Key_Num, key);
            if (index > 0)
                return;
            int i = index;
            if (i < 0)
                i = ~index;
      


            Node<T> newRoot = null;
            //for (int i = 0; i < leaf.Key_Num; i++)
            //    if (key.Equals(leaf.Keys[i]))
            //    {
            //        // TODO
            //        return;
            //    }

            if (!IsClustered())
            {
                var data_Address = Pending_Changes.Get_Current_Data_Pointer();
                Write_Data(value, key, 1, data_Address);
                newRoot = Insert_in_node(leaf, key, data_Address);
            }
            else
            {
                newRoot = Insert_in_node(leaf, key, 0);

                leaf = Find_Leaf_Node(key, newRoot);
                //var data = new Clustered_Data<T>(1, DateTime.Now, value);
                var data = new byte[Cluster_Data_Length + 12];
                Array.Copy(value, 0, data, 12, value.Length);
                leaf.Insert_Clustered_Data(data, key);
            }

            Pending_Changes.Append_New_Root(newRoot);
        }

        public bool Delete(T key)
        {
            var leaf = Find_Leaf_Node(key);
            for (int i = 0; i < leaf.Key_Num; i++)
                if (key.Equals(leaf.Keys[i]))
                {
                    Delete_Key_In_Node(leaf, key);
                    long previous_Address = leaf.Address;
                    Write_Node(leaf);
                    //UncommittedRoot = Clone_Anchestors_Of(leaf);
                    return true;
                }
            return false;
        }

        public void Flush()
        {
            Index_Stream.Flush();
        }



        protected void Delete_Key_In_Node(Node<T> node, T key)
        {
            int x = 0;
            while (!key.Equals(node.Keys[x])) 
                x++;
            for (int i = x; i < node.Key_Num - 1; i++)
                node.Keys[i] = node.Keys[i + 1];
            for (int i = x + 1; i < node.Key_Num; i++)
                node.Pointers[i] = node.Pointers[i + 1];
            node.Key_Num--;
        }

        protected Node<T> Insert_in_node(Node<T> node, T key, long address, Node<T> child = null)
        {
            Renew_Node_And_Dispose_Space(node);
            node.Insert_Key(key, address, child);

            Node<T> newRoot = null;
            if (node.Needs_To_Be_Splitted())
            {
                var split = node.Split(Node_Factory);

                Write_Node(split.Node_Right);

                if (node.Parent == null) // if i'm splitting the root, i need a new up level
                {
                    var root = Node_Factory.Create_New( false);
                    root.Keys[0] = split.Mid_Key;
                    root.Pointers[0] = split.Node_Left.Address;
                    root.Pointers[1] = split.Node_Right.Address;

                    root.Children[0] = split.Node_Left;
                    root.Children[1] = split.Node_Right;

                    root.Key_Num = 1;
                    split.Node_Left.Parent = root;
                    Write_Node(root);

                    split.Node_Left.Parent = split.Node_Right.Parent = root;
                    newRoot = root;
                }
                else
                {
                    newRoot = Insert_in_node(split.Node_Left.Parent, split.Mid_Key, 0, split.Node_Right);
                }
            }
            else
            {
                Write_Node(node);

                while (node.Parent != null)
                {
                    node = node.Parent;
                    Renew_Node_And_Dispose_Space(node);
                }

                newRoot = node;
            }

            return newRoot;
        }

        public Node<T> Clone_Tree_From_Root(Node<T> node)
        {
            var newNode = Node_Factory.Create_New_One_Like_This(node);
            newNode.Address = 0;

            for(int i=0; i < newNode.Key_Num +1; i++)
            {
                if (newNode.Children[i] == null)
                    continue;
                newNode.Children[i] = Clone_Tree_From_Root(newNode.Children[i]);
            }
            return newNode;
        }

        protected Node<T> Find_Leaf_Node(T key)
        {
            var node = Root;
            if(Pending_Changes.Has_Pending_Changes())
                node = Pending_Changes.Get_Uncommitted_Root();

            return Find_Leaf_Node(key, node);
        }

        

        protected Node<T> Find_Leaf_Node(T key, Node<T> root)
        {
            int depth = 0;
            while (!root.IsLeaf)
            {
                var index = Array.BinarySearch(root.Keys, 0, root.Key_Num, key);

                int i = index;
                if (i < 0)
                    i = ~index;
                else
                    if (!root.IsLeaf)
                        i++;

                if (root.Children[i] != null)
                    root = root.Children[i];
                else
                    root = Read_Node_From_Pointer(root, i);

                if (!root.IsValid)
                    throw new Exception("An Invalid node was read");
                depth++;

                Debug.Assert(depth < 100);
            }

            return root;
        }



    }

    
}
