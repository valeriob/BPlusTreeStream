using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BPlusTree.Core
{
    public class Values_Enumerator<TKey,TValue> : IEnumerator<TValue> where TKey : IComparable<TKey>, IEquatable<TKey>
    {
        IData_Reader<TKey> _reader;
        TKey _current_Key;
        Node<TKey> _current_Node;
        Node<TKey> _root;
        ISerializer<TValue> _data_serializer;
        bool _Current_Has_Value;

        public Values_Enumerator(Node<TKey> root, IData_Reader<TKey> reader, ISerializer<TValue> serializer)
        {
            _reader = reader;
            _root = root;
            _data_serializer = serializer;

            Find_First_Key();
        }


        public TValue Current { get; protected set; }

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
                data = _reader.Read_Data(dataAddress);
                Current = _data_serializer.Get_Instance(data.Payload, 0);
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
                        Current = default(TValue);
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
            data = _reader.Read_Data(dataAddress);
            Current = _data_serializer.Get_Instance(data.Payload, 0);
            return _Current_Has_Value = true;
        }

   
        public void Reset()
        {
            Find_First_Key();
            Current = default(TValue);
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


    public class Values_Enumerable<TKey, TValue> : IEnumerable<TValue> where TKey : IComparable<TKey>, IEquatable<TKey>
    {
        IData_Reader<TKey> _reader;
        Node<TKey> _root;
        ISerializer<TValue> _data_serializer;

        public Values_Enumerable(Node<TKey> root, IData_Reader<TKey> reader, ISerializer<TValue> serializer)
        {
            _reader = reader;
            _root = root;
            _data_serializer = serializer;
        }

        public IEnumerator<TValue> GetEnumerator()
        {
            return new Values_Enumerator<TKey, TValue>(_root, _reader, _data_serializer);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new Values_Enumerator<TKey, TValue>(_root, _reader, _data_serializer);
        }
    }
}
