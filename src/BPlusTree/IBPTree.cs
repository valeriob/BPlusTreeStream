using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace BPlusTree
{
    public interface IBPlusTree<T>
    {
        byte[] Get(T key);
        void Put(T key, byte[] value);

        void Flush();
        void Commit();
        void RollBack();
    }
    
 

}
