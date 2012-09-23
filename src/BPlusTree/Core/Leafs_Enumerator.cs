using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BPlusTree.Core
{
    public class Leafs_Right_Enumerator<T> : IEnumerator<Data<T>> where T : IComparable<T>, IEquatable<T>
    {
        IData_Reader<T> _reader;
        T _current_Key;
        Node<T> _current_Node;

        public Leafs_Right_Enumerator(Node<T> leaf, T key, IData_Reader<T> reader)
        {
            if (!leaf.IsLeaf)
                throw new Exception("not a leaf");

            _reader = reader;
            _current_Key = key;
            _current_Node = leaf;
        }


        public Data<T> Current { get; protected set; }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        object System.Collections.IEnumerator.Current
        {
            get { return Current; }
        }

        public bool MoveNext()
        {
            var node = _current_Node;
            var key = _current_Key;
            long dataAddress = 0;

            if (Current == null)
            {
                dataAddress = node.Get_Data_Address(key);
                Current = _reader.Read_Data(dataAddress);
                return true;
            }

            int indexOf = Array.BinarySearch(node.Keys, 0, node.Key_Num, key) + 1;
            if (indexOf < node.Key_Num)
            {
                key = node.Keys[indexOf];
            }
            else
            {
                while (_current_Key.CompareTo(key) >= 0)
                {
                    if (node.Parent == null)
                        return false;

                    node = node.Parent;

                    indexOf = Array.BinarySearch(node.Keys, 0, node.Key_Num, key);
                    if (indexOf < 0)
                        indexOf = ~indexOf;

                    if (indexOf == node.Key_Num)
                        indexOf--;
                    key = node.Keys[indexOf];
                }
            }

            node = _reader.Find_Leaf_Node(key, node);
            _current_Node = node;
            _current_Key = key;

            dataAddress = node.Get_Data_Address(key);
            Current = _reader.Read_Data(dataAddress);
            return true;
        }

   
        public void Reset()
        {
            throw new NotImplementedException();
        }
    }
}
