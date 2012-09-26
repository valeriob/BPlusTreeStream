using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BPlusTree.Core
{
    public class Cache_LRU<TKey, TValue>
    {
        Dictionary<TKey, Cache_Line<TKey, TValue>> _store;
        int Max_Size;
        int Batch;
        Task cleanup;
        AutoResetEvent gate = new AutoResetEvent(false);

        public int Hits { get; protected set; }
        public int Misses { get; protected set; }

        public Cache_LRU()
        {
            _store = new Dictionary<TKey, Cache_Line<TKey, TValue>>();
            Max_Size = 1024;
            Batch = 50;

            cleanup = Task.Factory.StartNew(Cleanup);
        }

        public TValue Get(TKey key)
        {
            if (!cleaning && _store.ContainsKey(key))
            {
                Hits++;
                var value = _store[key];
                value.Last_Used = DateTime.Now;
                value.Used_Count++;
                return value.Value;
            }
            else
            {
                Misses++;
            }

            return default(TValue);
        }

        public void Put(TKey key, TValue value)
        {
            _store[key] = new Cache_Line<TKey, TValue> { Added = DateTime.Now, Key = key, Value = value, Last_Used = DateTime.Now, Used_Count = 1 };
            Evict_If_Necessary();
        }

        public void Invalidate(TKey key)
        {
            _store.Remove(key);
        }


        protected bool Max_Size_Exceeded()
        {
            return Max_Size + Batch <= _store.Count;
        }
        protected void Evict_If_Necessary()
        {
            if (!Max_Size_Exceeded())
                return;

            //gate.Set();
        }

        bool cleaning;
        protected void Cleanup()
        {
            while (true)
            {
                gate.WaitOne();

                lock (this)
                    cleaning = true;

                var items = _store.Select(s => s.Value).OrderBy(d => d.Used_Count).Take(Batch).ToList();
                for (int i = 0; i < Batch; i++)
                {
                    _store.Remove(items[i].Key);
                }


                lock (this)
                {
                    gate.Reset();
                    cleaning = false;
                }
            }
        }

        public override string ToString()
        {
            return string.Format(" Occupation {0} / {1}, Hits : {2}, Misses {3}", _store.Count, Max_Size, Hits, Misses );
        }

    }

    public class Cache_Line<TKey, TValue>
    {
        public TKey Key { get; set; }
        public TValue Value { get; set; }
        public int Used_Count { get; set; }
        public DateTime Added { get; set; }
        public DateTime Last_Used { get; set; }

        public override string ToString()
        {
            return string.Format("Key {0}, Value {1}, Used {2}, Added : {3}, Last Usage {4} ", Key, Value, Used_Count, Added, Last_Used);
        }
    }
}
