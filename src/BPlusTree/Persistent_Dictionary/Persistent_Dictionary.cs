using BPlusTree.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using BPlusTree.Core;

namespace BPlusTree
{
    public class Persistent_Dictionary<TKey, TValue> : IDictionary<TKey, TValue> where TKey : IComparable<TKey>, IEquatable<TKey>
    {
        BPlusTree.Core.BPlusTree<TKey> _bptree;
        Configuration<TKey> _config;
        ISerializer<TValue> _value_Serializer;

        public Persistent_Dictionary(string name)
        {
            _value_Serializer = new Default_Binary_Serializer<TValue>();
            _config = Configuration<TKey>.Default_For(name);
            _bptree = new Core.BPlusTree<TKey>(_config);
        }

        public void Add(TKey key, TValue value)
        {
            try
            {
                _bptree.Put(key, _value_Serializer.Get_Bytes(value));
                _bptree.Commit();
            }
            catch (Exception)
            {
                _bptree.RollBack();
                throw;
            }
        }

        public bool ContainsKey(TKey key)
        {
            try
            {
                _bptree.Get(key);
                return true;
            }
            catch (Exception){ }
            return false;
        }


        public ICollection<TKey> Keys
        {
            get 
            {
                return new List<TKey>(new BPlusTree.Core.Keys_Enumerable<TKey>(_bptree.Root, _bptree));
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            try
            {
                var data = _bptree.Get(key);
                value = _value_Serializer.Get_Instance(data, 0);
                return true;
            }
            catch (Exception){ }
            value = default(TValue);
            return false;
        }

        public ICollection<TValue> Values
        {
            get 
            {
                return new List<TValue>(new BPlusTree.Core.Values_Enumerable<TKey,TValue>(_bptree.Root, _bptree, _value_Serializer));
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                var bytes = _bptree.Get(key);
                return _value_Serializer.Get_Instance(bytes, 0);
            }
            set
            {
                try
                {
                    _bptree.Put(key, _value_Serializer.Get_Bytes(value));
                    _bptree.Commit();
                }
                catch (Exception)
                {
                    _bptree.RollBack();
                    throw;
                }
            }
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            _bptree.Put(item.Key, _value_Serializer.Get_Bytes(item.Value));
        }

        public void Clear()
        {
            _bptree.Dispose();
            _config.Stream_Factory.Clear();
            _bptree = new Core.BPlusTree<TKey>(_config);
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            var data = this[item.Key];
            return data.Equals(item);
        }


        public int Count
        {
            get { return _bptree._metadata.Number_Of_Keys; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }


        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return new KeyValuePair_Enumerator<TKey, TValue>(_bptree.Root, _bptree, _value_Serializer);
        }


        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new KeyValuePair_Enumerator<TKey, TValue>(_bptree.Root, _bptree, _value_Serializer);
        }

        public bool Remove(TKey key)
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            throw new NotImplementedException();
        }

    }

}
