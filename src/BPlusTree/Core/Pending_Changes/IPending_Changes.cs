using System;
using System.Collections.Generic;
using System.IO;

namespace BPlusTree.Core.Pending_Changes
{
    public interface IPending_Changes<T>where T : IComparable<T>, IEquatable<T>
    {
        void Append_New_Root(Node<T> root);
        void Append_Node(Node<T> node);

        void Append_Data(Data<T> data);
        long Get_Current_Data_Pointer();
        IEnumerable<Data<T>> Get_Pending_Data();

        Node<T> Commit(Stream indexStream, Stream dataStream);

        void Free_Address(long address);
        long Get_Index_Pointer();
        Node<T> Get_Uncommitted_Root();

        IEnumerable<long> Get_Freed_Empty_Slots();
        bool Has_Pending_Changes();
        void Clean_Root();
    }
}
