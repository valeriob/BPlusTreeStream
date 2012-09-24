using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BPlusTree.Core
{
    public class Values_Enumerator<T> : IEnumerator<Data<T>> where T : IComparable<T>, IEquatable<T>
    {
        IData_Reader<T> _reader;
        T _current_Key;
        Node<T> _current_Node;
        Node<T> _root;

        public Values_Enumerator(Node<T> root, IData_Reader<T> reader)
        {
            _reader = reader;
            _root = root;

            Find_First_Key();
        }


        public Data<T> Current { get; protected set; }

        public void Dispose()
        {
            
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
            Find_First_Key();
            Current = null;
        }

        protected void Find_First_Key()
        {
            var node = _root;

            while (!node.IsLeaf)
                node = _reader.Read_Node_From_Parent_Pointer(node, 0);

            _current_Node = node;
            _current_Key = node.Keys[0];
        }

    }


    public class Values_Enumerable<T> : IEnumerable<T> where T : IComparable<T>, IEquatable<T>
    {
        IData_Reader<T> _reader;
        Node<T> _root;

        public Values_Enumerable(Node<T> root, IData_Reader<T> reader)
        {
            _reader = reader;
            _root = root;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new Keys_Enumerator<T>(_root, _reader);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new Keys_Enumerator<T>(_root, _reader);
        }
    }
}
