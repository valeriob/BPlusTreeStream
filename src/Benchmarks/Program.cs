﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Benchmarks
{
    public class Program
    {
        static void Main(string[] args)
        {
            var results = RunAll(100000, 1);
            foreach (var result in results)
            {
                Console.WriteLine(result.ToString());
            }

            Console.ReadLine();
        }



        public static IEnumerable<Result> RunAll(int count, int? batch = null)
        {
            //var benchmarks = new Benchmark[] 
            //{ 
            //  // new Ravendb(),
            //   new BPlusTree_WriteOnly(),
            //   new BPlusTree_ReadOnly(),
            //  //  new BPlusTree_Azure(),
            //   //new Memory_Copy(),
            //  // new File_Flush_Benchmark(),
            // //  new CSharpTest_BPlusTree(),
            ////  new Esent(),
            ////   new SqlServer()
            //};

            var results = new List<Result>();

            foreach (var b in Get_Lazy_Benchmarks())
            {
                try
                {
                    b.Prepare(count, batch.GetValueOrDefault(1));

                    var result = new Result { Name = b.GetType().Name, Start = DateTime.Now, Count = count, Batch = batch };
                    b.Run(count, batch.GetValueOrDefault(1));
                    result.Stop = DateTime.Now;
                    results.Add(result);
                }
                finally
                {
                    b.Dispose();
                }

            }

            return results;
        }

        static IEnumerable<Benchmark> Get_Lazy_Benchmarks()
        {
            yield return new BPlusTree_WriteOnly();
            //yield return new BPlusTree_ReadOnly();
        }


    }

}
