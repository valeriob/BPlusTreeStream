
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BPlusTree.Core
{
    public interface ISerializer<T>
    {
        byte[] Get_Bytes(T instance);
        T Get_Instance(byte[] bytes, int startIndex);
    }
}
