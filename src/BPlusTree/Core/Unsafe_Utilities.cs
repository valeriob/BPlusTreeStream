using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace BPlusTree.Core
{
    public static class Unsafe_Utilities
    {
        // http://code4k.blogspot.it/2010/10/high-performance-memcpy-gotchas-in-c.html

        [DllImport("msvcrt.dll", EntryPoint = "memcpy",
            CallingConvention = CallingConvention.Cdecl, SetLastError = false), 
        SuppressUnmanagedCodeSecurity]
        public static unsafe extern void* CopyMemory(void* dest, void* src, ulong count);

        public unsafe static void Memcpy(byte* dest, byte* src, int len)
        {
            //CopyMemory((void*)dest, (void*)src, (ulong)len);
            //return;
            if (len >= 64)
            {
                do
                {
                    var ad= *dest << 1;

                    *(long*)dest = *(long*)src;
                    *(long*)(dest + 8) = *(long*)(src + 8);
                    *(long*)(dest + 16) = *(long*)(src + 16);
                    *(long*)(dest + 24) = *(long*)(src + 24);
                    *(long*)(dest + 32) = *(long*)(src + 32);
                    *(long*)(dest + 40) = *(long*)(src + 40);
                    *(long*)(dest + 48) = *(long*)(src + 48);
                    *(long*)(dest + 56) = *(long*)(src + 56);

                    dest += 64;
                    src += 64;
                }
                while ((len -= 64) >= 64);
            }
            if (len >= 32)
            {
                do
                {
                    *(long*)dest = *(long*)src;
                    *(long*)(dest + 8) = *(long*)(src + 8);
                    *(long*)(dest + 16) = *(long*)(src + 16);
                    *(long*)(dest + 24) = *(long*)(src + 24);

                    dest += 32;
                    src += 32;
                }
                while ((len -= 32) >= 32);
            }

            if (len >= 16)
            {
                do
                {
                    *(long*)dest = *(long*)src;
                    *(long*)(dest + 8) = *(long*)(src + 8);
                    dest += 16;
                    src += 16;
                }
                while ((len -= 16) >= 16);
            }
            if (len > 0)
            {
                if ((len & 8) != 0)
                {
                    *(long*)dest = *(long*)src;
                    dest += 8;
                    src += 8;
                }
                if ((len & 4) != 0)
                {
                    *(int*)dest = *(int*)src;
                    dest += 4;
                    src += 4;
                }
                if ((len & 2) != 0)
                {
                    *(short*)dest = *(short*)src;
                    dest += 2;
                    src += 2;
                }
                if ((len & 1) != 0)
                {
                    byte* expr_75 = dest;
                    dest = expr_75 + 1;
                    byte* expr_7C = src;
                    src = expr_7C + 1;
                    *expr_75 = *expr_7C;
                }
            }
        }
    }
}
