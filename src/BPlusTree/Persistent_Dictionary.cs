﻿using BPlusTree.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace BPlusTree
{
    public class Persistent_Dictionary<TKey, TValue> : IDictionary<TKey, TValue> where TKey : IComparable<TKey>, IEquatable<TKey>
    {
        IBPlusTree<TKey> _bptree;
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
            catch (Exception)
            { }
            return false;
        }


        public ICollection<TKey> Keys
        {
            // TODO enumerate keys
            get { throw new NotImplementedException(); }
        }

        public bool Remove(TKey key)
        {
            throw new NotImplementedException();
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            throw new NotImplementedException();
        }

        public ICollection<TValue> Values
        {
            get { throw new NotImplementedException(); }
        }

        public TValue this[TKey key]
        {
            get
            {
                var bytes = _bptree.Get(key);
                return _value_Serializer.Get_Instance(bytes);
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
            throw new NotImplementedException();
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}