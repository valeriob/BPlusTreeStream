using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace BPlusTree
{
    public partial class String_BPlusTree<T>
    {
        public IBPlusTree<T> BPlusTree { get; set; }

        public String_BPlusTree(IBPlusTree<T> tree)
        {
            BPlusTree = tree;
        }
   

        public string Get(T key)
        {
            var bytes = BPlusTree.Get(key);
            return Encoding.UTF8.GetString(bytes);
        }

        public void Put(T key, string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            BPlusTree.Put(key, bytes);
        }

        public void Flush()
        {
            BPlusTree.Flush();
        }

        public void Commit()
        {
            BPlusTree.Commit();
        }

        public void RollBack()
        {
            BPlusTree.RollBack();
        }
    }
    
}
