
using BPlusTree.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Benchmarks
{
    public class Memory_Copy : Benchmark
    {
        public Memory_Copy()
        { 

        }


        unsafe public override void Run(int count, int batch)
        {
            //var a1 = new long[] { long.MinValue, 0, long.MaxValue };
            //var buff = new byte[24];
            //var a2 = new long[3];

            var a1 = new char[] {'1','2','3'};
            var buff = new byte[24];
            var a2 = new char[3];

            for (int i = 0; i < count; i++)
            {
                fixed (char* p_a1 = &a1[0])
                fixed (byte* p_buff = &buff[0])
                fixed (char* p_a2 = &a2[0])
                {
                    Unsafe_Utilities.Memcpy(p_buff, (byte*)p_a1, 24);
                    Unsafe_Utilities.Memcpy((byte*)p_a2, p_buff, 24);
                }

                //for (int j = 0; j < a1.Length; j++)
                //{
                //    Array.Copy(BitConverter.GetBytes(a1[j]), 0, buff, j * 8, 8);
                //    //a2[j] = BitConverter.ToInt64(buff, j * 8);
                //}
                //for (int j = 0; j < a1.Length; j++)
                //{
                //    a2[j] = BitConverter.ToInt64(buff, j * 8);
                //}
            }


        }
    }
}
