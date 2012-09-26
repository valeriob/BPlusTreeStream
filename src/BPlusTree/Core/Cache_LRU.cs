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
        ConcurrentDictionary<TKey, Cache_Line<TKey, TValue>> _store;
        int _max_Size;
        int _batch;
        Task _cleanup;
        AutoResetEvent _gate;
        Func<DateTime> _currentTimeProvider;

        public int Hits { get; protected set; }
        public int Misses { get; protected set; }

        public Cache_LRU(Func<DateTime> currentTimeProvider)
        {
            _store = new ConcurrentDictionary<TKey, Cache_Line<TKey, TValue>>();
            _max_Size = 10000;
            _batch = 100;
            _gate = new AutoResetEvent(false);
            _currentTimeProvider = currentTimeProvider;

            _cleanup = Task.Factory.StartNew(Cleanup);
        }

        public TValue Get(TKey key)
        {
            Cache_Line<TKey,TValue> value;
            if (_store.TryGetValue(key, out value))
            {
                Hits++;
                value.Last_Used = _currentTimeProvider();
                value.Used_Count++;
                return value.Value;
            }
            else
                Misses++;

            //if (Monitor.TryEnter(this, 1))
            //{
            //    try
            //    {
            //        if (_store.TryGetValue(key, out value))
            //        {
            //            Hits++;
            //            value.Last_Used = _currentTimeProvider();
            //            value.Used_Count++;
            //            return value.Value;
            //        }
            //        else
            //            Misses++;
            //    }
            //    finally
            //    {
            //        Monitor.Exit(this);
            //    }
            //}

            return default(TValue);
        }

        public void Set(TKey key, TValue value)
        {
            _store[key] = new Cache_Line<TKey, TValue> { Added = DateTime.Now, Key = key, Value = value, Last_Used = DateTime.Now, Used_Count = 1 };
           // Evict_If_Necessary();
        }

        public void Invalidate(TKey key)
        {
            Cache_Line<TKey, TValue> value;
            _store.TryRemove(key, out value);
        }


        protected bool Max_Size_Exceeded()
        {
            return _max_Size + _batch <= _store.Count;
        }
        protected void Evict_If_Necessary()
        {
            if (Max_Size_Exceeded())
                _gate.Set();
        }

        bool cleaning;
        protected void Cleanup()
        {
            while (true)
            {
                _gate.WaitOne();

                //lock (this)
                {
                    cleaning = true;

                var items = _store.Select(s => s.Value).OrderBy(d => d.Used_Count).Take(_batch).ToArray();
                for (int i = 0; i < _batch; i++)
                {
                    //_store.Remove(items[i].Key);
                    Cache_Line<TKey, TValue> value;
                    _store.TryRemove(items[i].Key, out value);

                }


                //lock (this)
                //{
                    _gate.Reset();
                    cleaning = false;
                }
            }
        }

        public override string ToString()
        {
            return string.Format(" Occupation {0} / {1}, Hits : {2}, Misses {3}", _store.Count, _max_Size, Hits, Misses );
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
