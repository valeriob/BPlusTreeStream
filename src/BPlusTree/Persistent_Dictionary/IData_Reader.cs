using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BPlusTree.Core
{
    public interface IData_Reader<T> where T: IComparable<T>, IEquatable<T> 
    {
        Data<T> Read_Data(long dataAddress);
        Node<T> Read_Node_From_Parent_Pointer(Node<T> parent, int key_Index, bool forUpdate = false);
        Node<T> Find_Leaf_Node(T key, Node<T> root, bool forUpdate = false);
    }
}
