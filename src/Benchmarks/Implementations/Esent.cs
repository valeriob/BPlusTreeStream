using Microsoft.Isam.Esent.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Benchmarks
{
    public class Esent : Benchmark
    {
        PersistentDictionary<int, string> dictionary;
        public Esent()
        {
            dictionary = new PersistentDictionary<int, string>("Names");
            
            dictionary.Clear();
        }
        public override void Run(int count, int batch)
        {
            for (int i = 0; i < count; i += batch)
            {
                for (var j = i; j < i + batch; j++)
                {
                    dictionary[j] = "test" + j;
                }
                dictionary.Flush();
            }
        }
    }
}
