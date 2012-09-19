using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BPlusTree.Core.Serializers
{

    public class String_Serializer : ISerializer<string>
    {
        readonly Encoding Encoding = Encoding.UTF8;


        public byte[] GetBytes(string value)
        {
            var buffer = new byte[16];
            Encoding.GetBytes(value.PadRight(16), 0, Math.Min(value.Length, 16), buffer, 0);
            return buffer;
        }

        public string Get_Instance(byte[] value, int startIndex)
        {
            return Encoding.GetString(value, startIndex, 16);
        }

        public int Serialized_Size_For_Single_Key_In_Bytes()
        {
            return 16;
        }


        unsafe public void To_Buffer(string[] values, int end_Index, byte[] buffer, int buffer_offset)
        {
            for (int i = 0; i < end_Index; i++)
                //DRDigit.Fast.FromHexString_ToBuffer(values[i], buffer, i * 16 + buffer_offset);
                Encoding.GetBytes(values[i].PadRight(16).ToCharArray(), 0, 16, buffer, i * 16 + buffer_offset);
        }


        unsafe public string[] Get_Instances(byte[] buffer, int startIndex, int length)
        {
            var result = new string[length];

            for (int i = 0; i < length; i++)
                 result[i] = Encoding.GetString(buffer, startIndex + i * 16, 16).TrimEnd();
                //result[i] = DRDigit.Fast.ToHexString(buffer, startIndex + i * 16, 16, false);
            return result;

        }
    }

}
