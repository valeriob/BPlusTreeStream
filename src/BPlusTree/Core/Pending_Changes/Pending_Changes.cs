using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace BPlusTree.Core.Pending_Changes
{
    public class Pending_Changes<T> : IPending_Changes<T> where T: IComparable<T>, IEquatable<T>
    {
        Node_Factory<T> Node_Factory;
        int Block_Size;
        Node<T> Uncommitted_Root;

        private long _index_Pointer;
        private long _data_Pointer;
        List<Node<T>> Nodes;
        List<Block_Group> Empty_Slots;
        List<long> Freed_Empty_Slots;
        List<Node<T>> Pending_Nodes;
        List<Data<T>> Pending_Data;

        Dictionary<long, Block> _base_Address_Index = new Dictionary<long, Block>();
        Dictionary<long, Block> _end_Address_Index = new Dictionary<long, Block>();

        public long Get_Index_Pointer() { return _index_Pointer; }
        public long Get_Current_Data_Pointer() { return _data_Pointer; }
        public IEnumerable<Node<T>> Last_Cached_Nodes() { return Nodes; }
        public List<Block_Group> Get_Empty_Slots() { return Empty_Slots; }
        public Node<T> Get_Uncommitted_Root() { return Uncommitted_Root; }
        public IEnumerable<long> Get_Freed_Empty_Slots() { return Freed_Empty_Slots; }

        public IEnumerable<Data<T>> Get_Pending_Data() { return Pending_Data; }

        public Pending_Changes(int blockSize, long index_Pointer, long data_Pointer,  Node_Factory<T> node_Factory )
        {
            Block_Size = blockSize;
            Node_Factory = node_Factory;

            Freed_Empty_Slots = new List<long>();
            Pending_Nodes = new List<Node<T>>();
            Pending_Data = new List<Data<T>>();
            Nodes = new List<Node<T>>();
            Empty_Slots = new List<Block_Group>();

            _base_Address_Index = new Dictionary<long, Block>();
            _end_Address_Index = new Dictionary<long, Block>();

            _index_Pointer = index_Pointer;
            _data_Pointer = data_Pointer;
        }




        protected void Block_Usage_Finished(Block_Usage usage_Block)
        {
            var block = usage_Block.Block;

            var base_Address = block.Base_Address();
           

            _base_Address_Index.Remove(base_Address);
            block.Reserve_Size(usage_Block.Used_Length);

            Fix_Block_Position_In_Groups(block, base_Address, block.Base_Address(), block.Length + usage_Block.Used_Length, block.Length);

            if (block.IsEmpty())
                _end_Address_Index.Remove(block.End_Address());
            else
                _base_Address_Index[block.Base_Address()] = block;
        }


        public void Add_Block_Address_To_Available_Space()
        {
            // TODO compact addresses
            foreach (var address in Freed_Empty_Slots)
            {
                if (_end_Address_Index.ContainsKey(address))
                {
                    Block before = _end_Address_Index[address];

                    int beforeLength = before.Length;
                    before.Append_Block(Block_Size);
                    int newLength = before.Length;

                    if (_base_Address_Index.ContainsKey(address + Block_Size))
                    {
                        Block after = _base_Address_Index[address + Block_Size];

                        Fix_Block_Position_In_Groups(after, after.Base_Address(), 0, after.Length, 0);

                        newLength += after.Length;
                        before.Append_Block(after.Length);

                        _base_Address_Index.Remove(address + Block_Size);
                    }

                    _end_Address_Index.Remove(address);
                    _end_Address_Index[before.End_Address()] = before;

                    Fix_Block_Position_In_Groups(before, before.Base_Address(), before.Base_Address(), beforeLength, newLength);
                    continue;
                }

                if (_base_Address_Index.ContainsKey(address + Block_Size))
                {
                    Block after = _base_Address_Index[address + Block_Size];

                    _base_Address_Index.Remove(after.Base_Address());
                    after.Prepend_Block(Block_Size);
                    _base_Address_Index[after.Base_Address()] = after;

                    Fix_Block_Position_In_Groups(after, after.Base_Address() + Block_Size, after.Base_Address(), after.Length - Block_Size, after.Length);
                    continue;
                }

                Insert_Block(address, Block_Size);
            }
        }

        protected Block_Group? Find_Block_Group(int length)
        {
            for (int i = 0; i < Empty_Slots.Count; i++)
                if (Empty_Slots[i].Length == length)
                    return Empty_Slots[i];
            return null;
        }

        protected void Fix_Block_Position_In_Groups(Block block, long old_Address, long new_Address, int old_Length, int new_Length)
        {
            int index = -1;
            for (int i = 0; i < Empty_Slots.Count; i++)
                if (Empty_Slots[i].Length == old_Length)
                    index = i;

            var group = Empty_Slots[index];
            
            group.Blocks.Remove(old_Address);

            if(group.Blocks.Count == 0)
                Empty_Slots.RemoveAt(index);
            if (new_Length == 0)
                return;

            index = -1;
            for (int i = 0; i < Empty_Slots.Count; i++)
                if (Empty_Slots[i].Length == new_Length)
                    index = i;

            if (index == -1)
            {
                group = new Block_Group { Length = new_Length, Blocks = new Dictionary<long, Block>() };
                Empty_Slots.Add(group);
            }
            else
                group = Empty_Slots[index];
            group.Blocks[new_Address] = block;
        }

        protected List<Block_Usage> Look_For_Available_Blocks(int length)
        {
            //Empty_Slots.Sort(new Length_Comparer(length));

            var result = new List<Block_Usage>();

            var enumerator = Empty_Slots.GetEnumerator();

            while (enumerator.MoveNext() && length > 0 )
            {
                var group = enumerator.Current;
              //  if (group.Length <=  length / 10) // 5 * Block_Size)
               //     continue;
                var blocks = group.Blocks.Values.GetEnumerator();
                while (blocks.MoveNext() && length > 0)
                {
                    var block = blocks.Current;
                    result.Add(new Block_Usage(block));
                    length -= group.Length;
                }
            }

            return result;
        }

        protected void Insert_Block(long address, int lenght)
        {
            var group = Find_Block_Group(lenght);

            if (group == null)
            {
                group = new Block_Group() { Length = lenght, Blocks = new Dictionary<long,Block>() };
                Empty_Slots.Add(group.Value);
            }

            var block = new Block(address, lenght);
            group.Value.Blocks[address] = block;

            _base_Address_Index[address] = block;
            _end_Address_Index[block.End_Address()] = block;
        }

        protected void Update_Addresses_From(Node<T> root, Queue<long> addresses)
        {
            if(root.Is_Volatile)
                root.Address = addresses.Dequeue();
            if (root.IsLeaf)
                return;

            for (int i = 0; i < root.Key_Num + 1; i++)
            {
                if (root.Children[i] == null || !root.Children[i].Is_Volatile)
                    continue;

                Update_Addresses_From(root.Children[i], addresses);
                root.Pointers[i] = root.Children[i].Address;
            }
        }

        protected void Update_Addresses_From_Base(Node<T>[] nodes, Node<T> root, ref long base_Address)
        {
            root.Address = base_Address;
            base_Address += Block_Size;
            if (root.IsLeaf)
                return;

            for (int i = 0; i < nodes.Length; i++)
            {
                if (nodes[i].Parent != root)
                    continue;

                var new_Child_Address = base_Address;
                var old_Child_Address = nodes[i].Address;
                root.Update_Child_Address(old_Child_Address, new_Child_Address);

                Update_Addresses_From_Base(nodes, nodes[i], ref base_Address);
            }
        }


        protected void Commit_Data(Stream stream)
        {
            long initial_Address = Pending_Data[0].Address;

            int keySize = Node_Factory.Serializer.Serialized_Size_For_Single_Key_In_Bytes();
            int bufferSize = 0;
            for (int i = 0; i < Pending_Data.Count; i++)
                bufferSize += Pending_Data[i].Total_Persisted_Size(keySize);

            byte[] buffer = new byte[bufferSize];
            int offset = 0;
            for (int i = 0; i < Pending_Data.Count; i++)
            {
                Pending_Data[i].Write_To_Buffer(Node_Factory.Serializer, buffer, offset);
                offset += Pending_Data[i].Total_Persisted_Size(keySize);
            }

            Pending_Data.Clear();

            stream.Seek(initial_Address, SeekOrigin.Begin);
            stream.Write(buffer, 0, buffer.Length);
        }


        public void Free_Address(long address)
        {
            if (address != 0)
                Freed_Empty_Slots.Add(address);
        }

        public void Append_Node(Node<T> node)
        {
            Pending_Nodes.Add(node);
        }
        public void Append_Data(Data<T> data)
        {
            Pending_Data.Add(data);
            _data_Pointer = data.Address + data.Total_Persisted_Size(Node_Factory.Serializer.Serialized_Size_For_Single_Key_In_Bytes());
        }

        public void Append_New_Root(Node<T> root)
        {
            Uncommitted_Root = root;
        }

        public int Commit_Count;
        public int Nodes_Count;
        public int Blocks_Count;
        public Dictionary<int, int> Blocks_Count_By_Lenght = new Dictionary<int, int>();

        public Node<T> Commit(Stream indexStream, Stream dataStream)
        {
            Commit_Data(dataStream);

            var pending_Nodes_ = new List<Node<T>>();

            Find_All_Pending_Nodes_From(pending_Nodes_, Uncommitted_Root);

            int neededBytes = Block_Size * pending_Nodes_.Count;

            var blocks = Look_For_Available_Blocks(neededBytes);

            Blocks_Count += blocks.Count;
            Nodes_Count += pending_Nodes_.Count;
            Commit_Count++;

            foreach (var block in blocks)
            {
                if (Blocks_Count_By_Lenght.ContainsKey(block.Length))
                    Blocks_Count_By_Lenght[block.Length]++;
                else
                    Blocks_Count_By_Lenght[block.Length] = 1;
            }

            var block_At_End_Of_File = new Block_Usage(new Block(_index_Pointer, int.MaxValue));

            blocks.Add(block_At_End_Of_File);
            blocks = blocks.OrderBy(b => b.Base_Address()).ToList();

            var addressesQueue = new Queue<long>();
            foreach (var block in blocks)
                for (int i = 0; i < block.Length && pending_Nodes_.Count > addressesQueue.Count; i += Block_Size)
                    addressesQueue.Enqueue(block.Base_Address() + i);

            Update_Addresses_From( Uncommitted_Root, addressesQueue);

            var nodes = new Queue<Node<T>>(pending_Nodes_.OrderBy(d => d.Address));

            foreach (var block in blocks)
            {
                if (nodes.Count == 0)
                    break;

                var toUpdate = new List<Node<T>>();
                for (int j = 0; j < block.Length && nodes.Count > 0; j += Block_Size)
                {
                    toUpdate.Add(nodes.Dequeue());
                    block.Use(Block_Size);
                }

                int buffer_Size = toUpdate.Count * Block_Size;
                var buffer = new byte[buffer_Size];
                for (int i = 0; i < toUpdate.Count; i++)
                    Node_Factory.To_Bytes_In_Buffer(toUpdate[i], buffer, i * Block_Size);

                indexStream.Seek(block.Base_Address(), SeekOrigin.Begin);
                indexStream.Write(buffer, 0, buffer.Length);
            }

            foreach (var block in blocks)
            {
                if (block == block_At_End_Of_File)
                    _index_Pointer = block.Base_Address() + block.Used_Length;
                else
                    Block_Usage_Finished(block);
            }

            foreach (var node in pending_Nodes_)
                node.Is_Volatile = false;


            Nodes.Clear();
            Nodes.AddRange(Pending_Nodes);
            Pending_Nodes.Clear();
            indexStream.Flush();

            Add_Block_Address_To_Available_Space();
            Freed_Empty_Slots.Clear();

            var newRoot = Node_Factory.Create_New_One_Like_This(Uncommitted_Root);
            for (int i = 0; i < newRoot.Key_Num + 1; i++)
                newRoot.Children[i] = null;
            return newRoot;
        }


        protected void Find_All_Pending_Nodes_From(List<Node<T>> nodes, Node<T> root, bool only_Volatile_Nodes = true)
        {
            if(root.Is_Volatile && only_Volatile_Nodes || ! only_Volatile_Nodes)
                nodes.Add(root);
            for (int i = 0; i < root.Key_Num + 1; i++)
            {
                var child = root.Children[i];
                if (child == null || (!child.Is_Volatile && only_Volatile_Nodes))
                    continue;

                Find_All_Pending_Nodes_From(nodes, child);
            }
        }

        public void Clean_Root()
        {
            Uncommitted_Root = null;
            // dispose nodes
        }

        public bool Has_Pending_Changes()
        {
            return Uncommitted_Root != null;
        }
    }

 

   
}
