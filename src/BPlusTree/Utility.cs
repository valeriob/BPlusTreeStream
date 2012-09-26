using BPlusTree.Core;
using BPlusTree.Core.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BPlusTree
{
    public class Utility
    {
        public static void Rebuild_Index<TKey>(BPlusTree<TKey> source, BPlusTree<TKey> destination) 
            where TKey : IComparable<TKey>, IEquatable<TKey>
        {
            if (destination.IsClustered()) 
            {
                var enumerable = new KeyValuePair_Enumerable<TKey, byte[]>(source.Root, source, new Null_Serializer());
                foreach (var data in enumerable)
                    destination.Put(data.Key, data.Value);
            }
            else
            {
                var enumerable = new KeyData_Enumerable<TKey>(source.Root, source);
                foreach (var data in enumerable)
                    destination.Set_Data(data.Key, data);
            }
        }
    }
}
