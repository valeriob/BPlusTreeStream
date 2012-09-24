using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BPlusTree.Core.Serializers
{
    public class Int_Serializer : IKey_Serializer<int>
    {
        public byte[] Get_Bytes(int value)
        {
            return BitConverter.GetBytes(value);
        }

        public int Get_Instance(byte[] value, int startIndex)
        {
            return BitConverter.ToInt32(value, startIndex);
        }

        public int Serialized_Size_For_Single_Key_In_Bytes()
        {
            return 4;
        }


        public unsafe void To_Buffer(int[] values, int end_Index, byte[] buffer, int buffer_offset)
        {
            fixed (int* p_Pointers = &values[0])
            fixed (byte* p_buff = &buffer[0])
            {
                byte* shifted = p_buff + buffer_offset;
                Unsafe_Utilities.Memcpy(shifted, (byte*)p_Pointers, 4 * (end_Index + 1));
            }
        }


        unsafe public int[] Get_Instances(byte[] value, int startIndex, int length)
        {
            int[] result = new int[length];
            fixed (int* p_Pointers = &result[0])
            fixed (byte* p_buff = &value[0])
            {
                byte* shifted = p_buff + startIndex;
                Unsafe_Utilities.Memcpy((byte*)p_Pointers, shifted, 4 * length);
            }
            return result;
        }

    }

    public class Long_Serializer : IKey_Serializer<long>
    {
        public byte[] Get_Bytes(long value)
        {
            return BitConverter.GetBytes(value);
        }

        public long Get_Instance(byte[] value, int startIndex)
        {
            return BitConverter.ToInt64(value, startIndex);
        }

        public int Serialized_Size_For_Single_Key_In_Bytes()
        {
            return 8;
        }


        public unsafe void To_Buffer(long[] values, int end_Index, byte[] buffer, int buffer_offset)
        {
            fixed (long* p_Pointers = &values[0])
            fixed (byte* p_buff = &buffer[0])
            {
                byte* shifted = p_buff + buffer_offset;
                Unsafe_Utilities.Memcpy(shifted, (byte*)p_Pointers, 8 * (end_Index + 1));
            }
        }


        public long[] Get_Instances(byte[] value, int startIndex, int length)
        {
            throw new NotImplementedException();
        }
    }

    public class Guid_Serializer : IKey_Serializer<Guid>
    {
        public byte[] Get_Bytes(Guid value)
        {
            return value.ToByteArray();
        }

        public Guid Get_Instance(byte[] value, int startIndex)
        {
            return new Guid(new byte[] {   value[startIndex], value[startIndex + 1], value[startIndex + 2], value[startIndex + 3],
                                    value[startIndex + 4], value[startIndex + 5], value[startIndex + 6], value[startIndex + 7],
                                    value[startIndex + 8], value[startIndex + 9], value[startIndex + 10], value[startIndex + 11],
                                    value[startIndex + 12], value[startIndex + 13], value[startIndex + 14], value[startIndex + 15]});
        }

        public int Serialized_Size_For_Single_Key_In_Bytes()
        {
            return 16;
        }


        unsafe public void To_Buffer(Guid[] values, int end_Index, byte[] buffer, int buffer_offset)
        {
            fixed (Guid* p_values = &values[0])
            fixed (byte* p_buffer = &buffer[0])
            {
                Unsafe_Utilities.Memcpy(p_buffer + buffer_offset, (byte*)p_values, 16 * end_Index);
            }
        }


        unsafe public Guid[] Get_Instances(byte[] buffer, int startIndex, int length)
        {
            Guid[] result = new Guid[length];

            fixed (Guid* p_result = &result[0])
            fixed (byte* p_buffer = &buffer[0])
            {
                Unsafe_Utilities.Memcpy((byte*)p_result, p_buffer, 16 * length);
            }

            return result;
        }
    }


}
