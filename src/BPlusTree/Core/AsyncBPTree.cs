using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BPlusTree.Config;

namespace BPlusTree.Core
{
    public class AsyncBPTree<TKey> :BPlusTree<TKey>
        where TKey : IComparable<TKey>, IEquatable<TKey>
    {
        ConcurrentQueue<Operation<TKey>> _operations;

        public AsyncBPTree(Configuration<TKey> config) : base(config)
        {
            _operations = new ConcurrentQueue<Operation<TKey>>();
        }

        //public Task PutAsync(TKey key, byte[] value)
        //{
        //    var result = new Task();
        //    var operation = new Operation<TKey> { Task =  result};
        //    _operations.Enqueue(operation);

        //    return result;
        //}
    }

    public class Operation<TKey>  where TKey : IComparable<TKey>, IEquatable<TKey>
    {
        public Task Task { get; set; }
    }
}
