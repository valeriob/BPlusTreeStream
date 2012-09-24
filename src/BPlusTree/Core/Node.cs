using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace BPlusTree.Core
{
    public class Node<T> : IDisposable where T: IComparable<T>, IEquatable<T>
    {
        public bool IsLeaf { get; set; }

        public T[] Keys { get; set; }
        public long[] Pointers { get; set; }
        public int Key_Num { get; set; }
        public int[] Versions { get; set; }
        public byte[,] Data { get; set; }

        public bool IsClustered { get; set; }
        public Node<T> Parent { get; set; }
        public long Address { get; set; }
        public Node<T>[] Children { get; protected set; }
        public bool Is_Volatile { get; set; }

        Node_Factory<T> _Factory;

        public void Dispose()
        {
            Key_Num = -2;
            Array.Clear(Keys, 0, Keys.Length);
            Array.Clear(Pointers, 0, Pointers.Length);
            Array.Clear(Versions, 0, Versions.Length);
            Array.Clear(Children, 0, Children.Length);
            Array.Clear(Data, 0, Data.Length);
            Address = 0;
            Parent = null;
            IsLeaf = false;
            Is_Volatile = true;
            _Factory.Return(this);
            GC.SuppressFinalize(this);
        }

        int _clustered_Data_Length;
        public Node(Node_Factory<T> factory, int order, bool isLeaf, bool isClustered, int clustered_Data_Length) 
        {
            _Factory = factory;
            IsLeaf = isLeaf;
            IsClustered = isClustered;
            Pointers = new long[order + 1];
            Keys = new T[order];
            Versions = new int[order];
            Children = new Node<T>[order + 1];
            Is_Volatile = true;

            if (isClustered)
            {
                Data = new byte[order + 1, clustered_Data_Length + 12];
                _clustered_Data_Length = clustered_Data_Length;
            }
            else Data = new byte[0, 0];
        }

        public void Insert_Key(T key, long address, Node<T> child)
        {
            var index = Array.BinarySearch(Keys, 0, Key_Num, key);

            if(index >=0)
                throw new Exception("Key already exists ?");
           
            int x = ~index;

            for (int i = Key_Num; i > x; i--)
                Keys[i] = Keys[i - 1];

            for (int i = Key_Num + 1; i > x + 1; i--)
            {
                Pointers[i] = Pointers[i - 1];
                Children[i] = Children[i - 1];
                if (IsLeaf)
                {
                   // Data[i] = Data[i - 1];
                    for (int j = 0; j < _clustered_Data_Length; j++)
                        Data[i, j] = Data[i - 1, j];
                }
            }

            Keys[x] = key;
            Pointers[x + 1] = address;
            Children[x + 1] = child;
            
            Key_Num++;

            if (child != null)
                child.Parent = this;

        }

        public bool Needs_To_Be_Splitted()
        {
            return Key_Num == Keys.Length;
        }

        public void Update_Child_Address(long previousAddress, long newAddress)
        {
            for (int i = 0; i < Key_Num + 1; i++)
                if (Pointers[i] == previousAddress)
                {
                    Pointers[i] = newAddress;
                    return;
                }

            throw new Exception("this should not happen");
        }

        public Node_Split_Result<T> Split(Node_Factory<T> node_Factory)
        {
            int size = Keys.Length;

            var node_Left = this;
            var node_Right = node_Factory.Create_New(node_Left.IsLeaf);

            node_Right.Parent = node_Left.Parent;
            var mid_Key = node_Left.Keys[size / 2];

            node_Right.Key_Num = size - size / 2 - 1;
            for (int i = 0; i < node_Right.Key_Num; i++)
            {
                node_Right.Keys[i] = node_Left.Keys[i + (size / 2 + 1)];
                node_Right.Pointers[i] = node_Left.Pointers[i + (size / 2 + 1)];
                if (!IsLeaf)
                    node_Right.Children[i] = node_Left.Children[i + (size / 2 + 1)];
                else
                {
                    //node_Right.Data[i] = node_Left.Data[i + (size / 2 + 1)];
                    for (int j = 0; j < _clustered_Data_Length; j++)
                        node_Right.Data[i, j] = node_Left.Data[i + (size / 2 + 1), j];
                }
                if (node_Right.Children[i] != null)
                    node_Right.Children[i].Parent = node_Right;
            }

            node_Right.Pointers[node_Right.Key_Num] = node_Left.Pointers[size];
            node_Right.Children[node_Right.Key_Num] = node_Left.Children[size];

            //node_Right.Data[node_Right.Key_Num] = node_Left.Data[size];
            
            for (int j = 0; j < _clustered_Data_Length; j++)
                node_Right.Data[node_Right.Key_Num, j] = node_Left.Data[size, j];
            
            if (node_Right.Children[node_Right.Key_Num] != null)
                node_Right.Children[node_Right.Key_Num].Parent = node_Right;
            node_Left.Key_Num = size / 2;

            if (node_Left.IsLeaf)
            {
                node_Left.Key_Num++;
                node_Right.Pointers[0] = node_Left.Pointers[0];
                node_Right.Children[0] = node_Left.Children[0];

                node_Left.Pointers[0] = node_Right.Address;  //TODO double linked list
                mid_Key = node_Left.Keys[size / 2 + 1];
            }

            return new Node_Split_Result<T> { Node_Left= node_Left,  Node_Right = node_Right, Mid_Key = mid_Key };
        }

      

        public void Insert_Clustered_Data(byte[] data, T key)
        {
            var index = Array.BinarySearch(Keys, 0, Key_Num, key);

            for (int i = 0; i < data.Length; i++)
                Data[index + 1, i] = data[i];
            //Data[index + 1] = data;
        }
        public long Get_Data_Address(T key)
        {
            if (!IsLeaf)
                throw new BpTreeException("Cannot retrieve data address from middle node.");

            var index = Array.BinarySearch(Keys, 0, Key_Num, key);

            if (index >= 0)
                return Pointers[index];
            else
                throw new Key_Not_Found(key);
            //for (int i = 0; i < Keys.Length; i++)
            //    if (Keys[i].Equals(key))
            //    {
            //        Debug.Assert(index == i);
            //        return Pointers[i + 1];
            //    }
            //throw new Key_Not_Found(key);//"Key " + key + " not found !");
        }
        public byte[] Get_Clustered_Data(T key)
        {
            var index = Array.BinarySearch(Keys, 0, Key_Num, key);
            var data = new byte[_clustered_Data_Length];
            for (int i = 0; i < data.Length; i++)
                data[i] = Data[index + 1, i + 12];
            //return Data[index + 1];
            return data;

        }

        public bool IsValid
        {
            get { return Key_Num > 0; } 
        }

        public int Index_Of_Child(Node<T> child)
        {
            for (int i = 0; i < Key_Num + 1; i++)
                if (Children[i] == child)
                    return i;
            throw new Exception("nodes are not parent-child related");
        }


        public string Print_Keys()
        {
            var keys = "";
            for (int i = 0; i < Key_Num; i++)
                keys += Keys[i] + ", ";
            keys = keys.TrimEnd(' ', ',');
            return keys;
        }
        public override string ToString()
        {
            if (Key_Num <= 0)
                return string.Format("{0} Invalid", Address);

            var keys = Print_Keys();

            string root = Parent == null ? "(Root)" : "";
            if (IsLeaf)
                return string.Format("{2} {1} Leaf : {0}", keys, root, Address);
            else
            {
                string children = "";
                for (int i = 0; i < Key_Num + 1; i++)
                    if (Children[i] != null)
                        children += " { " + Children[i].Print_Keys() + " } , ";
                    else
                        children += " { N/L }, ";
                children = children.TrimEnd(',', ' ');

                return string.Format("{2} {1} Node : {0}.  Children : [ {3} ]", keys, root, Address, children);
            }
        }

        public override bool Equals(object obj)
        {
            var other = obj as Node<T>;
            if (Key_Num != other.Key_Num)
                return false;
            for (int i = 0; i < Key_Num + 1; i++)
            {
                if (i < Key_Num && !Keys[i].Equals(other.Keys[i]))
                    return false;
                 if (Pointers[i] != other.Pointers[i])
                     return false;
            }
            return true;
        }

    }

    public struct Node_Split_Result<T> where T:IComparable<T>, IEquatable<T>
    {
        public Node<T> Node_Left { get; set; }
        public Node<T> Node_Right { get; set; }
        public T Mid_Key { get; set; }
    }


    public struct Node_Reference<T> where T : IComparable<T>, IEquatable<T>
    {
        public Node<T> Node { get; set; }

        public long Address { get; set; }
    }
}
