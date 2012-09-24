using BPlusTree.Core;
using BPlusTree.Core.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace BPlusTree.Config
{
    public class Configuration<TKey> where TKey : IComparable<TKey>, IEquatable<TKey>
    {
        public IStream_Factory Stream_Factory { get; set; }
        public int BPTree_Order { get; set; }
        public int Alignment { get; set; }
        public int Clustering_Data_Length { get; set; }
        public BPlusTree.Core.IKey_Serializer<TKey> Key_Serializer { get; set; }

        public static Configuration<TKey> Default_For(string id)
        {
            return new Configuration<TKey> 
            {
                Stream_Factory = new File_Stream_Factory(id), 
                BPTree_Order = 128,
                Alignment = 0, 
                Clustering_Data_Length = 0,
                Key_Serializer = Best_Serializer(),
            };
        }


        public static BPlusTree.Core.IKey_Serializer<TKey> Best_Serializer()
        {
            Type tkey = typeof(TKey);
            if(tkey == typeof(int))
                return (BPlusTree.Core.IKey_Serializer<TKey>)new Int_Serializer();

            if (tkey == typeof(long))
                return (BPlusTree.Core.IKey_Serializer<TKey>)new Long_Serializer();

            if (tkey == typeof(Guid))
                return (BPlusTree.Core.IKey_Serializer<TKey>)new Guid_Serializer();

            if (tkey == typeof(string))
                return (BPlusTree.Core.IKey_Serializer<TKey>)new String_Serializer();

            throw new NotSupportedException("");
        }
    }
}
