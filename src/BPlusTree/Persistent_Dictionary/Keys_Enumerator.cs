using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BPlusTree.Core
{
    public class Keys_Enumerator<T> : IEnumerator<T> where T : IComparable<T>, IEquatable<T>
    {
        IData_Reader<T> _reader;
        T _current_Key;
        Node<T> _current_Node;
        Node<T> _root;
        bool _Current_Has_Value;

        public Keys_Enumerator(Node<T> root, IData_Reader<T> reader)
        {
            _reader = reader;
            _root = root;

            Find_First_Key();
        }


        public T Current { get; protected set; }

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

            if (!_Current_Has_Value)
            {
                Current = _current_Key;
                _Current_Has_Value = true;
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

            Current = key;
            _Current_Has_Value = true;
            return true;
        }

   
        public void Reset()
        {
            Find_First_Key();
            Current = default(T);
            _Current_Has_Value = false;
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

    public class Keys_Enumerable<T> : IEnumerable<T> where T : IComparable<T>, IEquatable<T>
    {
        IData_Reader<T> _reader;
        Node<T> _root;

        public Keys_Enumerable(Node<T> root, IData_Reader<T> reader)
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
