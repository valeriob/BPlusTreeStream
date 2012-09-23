using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BPlusTree.Core
{
    public class BpTreeException : Exception
    {
    }

    public class Key_Not_Found : BpTreeException
    {
        public object Key { get; set; }
        public Key_Not_Found(object key)
        {
            Key = key;
        }
    }
}
