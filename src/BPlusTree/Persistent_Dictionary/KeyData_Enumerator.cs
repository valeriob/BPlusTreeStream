using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BPlusTree.Core
{
    public class KeyData_Enumerator<TKey> : IEnumerator<Data<TKey>> where TKey : IComparable<TKey>, IEquatable<TKey>
    {
        IData_Reader<TKey> _reader;
        TKey _current_Key;
        Node<TKey> _current_Node;
        Node<TKey> _root;

        bool _Current_Has_Value;

        public KeyData_Enumerator(Node<TKey> root, IData_Reader<TKey> reader)
        {
            _reader = reader;
            _root = root;

            Find_First_Key();
        }


        public Data<TKey> Current { get; protected set; }

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
            Data<TKey> data;

            if (!_Current_Has_Value)
            {
                dataAddress = node.Get_Data_Address(key);
                Current = data = _reader.Read_Data(dataAddress);                
                return _Current_Has_Value = true;
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
                    {
                        Current = null;
                        return _Current_Has_Value = false;
                    }

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
            Current = data = _reader.Read_Data(dataAddress);
            return _Current_Has_Value = true;
        }

   
        public void Reset()
        {
            Find_First_Key();
            Current = null;
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


    public class KeyData_Enumerable<TKey> : IEnumerable<Data<TKey>> where TKey : IComparable<TKey>, IEquatable<TKey>
    {
        IData_Reader<TKey> _reader;
        Node<TKey> _root;

        public KeyData_Enumerable(Node<TKey> root, IData_Reader<TKey> reader)
        {
            _reader = reader;
            _root = root;
        }

        public IEnumerator<Data<TKey>> GetEnumerator()
        {
            return new KeyData_Enumerator<TKey>(_root, _reader);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new KeyData_Enumerator<TKey>(_root, _reader);
        }
    }
}
