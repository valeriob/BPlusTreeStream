using CSharpTest.Net.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Benchmarks
{
    public class CSharpTest_BPlusTree : Benchmark
    {
        public override void Run(int count, int batch)
        {
            string fileName = "CSharpTest_BPlusTree.dat";

            if (File.Exists(fileName))
                File.Delete(fileName);

            var file = File.Open(fileName, FileMode.OpenOrCreate);
            //var file = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, 4096,
            //    FileOptions.WriteThrough | FileOptions.SequentialScan);
            var opts = new CSharpTest.Net.Collections.BPlusTree<int, string>.OptionsV2(PrimitiveSerializer.Int32, PrimitiveSerializer.String)
            {
                BTreeOrder = 11,
                FileName = "file",
                CreateFile = CSharpTest.Net.Collections.CreatePolicy.Always,
                StorageType = CSharpTest.Net.Collections.StorageType.Disk,
            };
            var index = new CSharpTest.Net.Collections.BPlusTree<int, string>(opts);

            for (int i = 0; i < count; i+= batch)
            {
                for (int j = i; j < i + batch; j++)
                {
                    index.Add(j, "text about " + j);
                }
                index.Commit();
            }

        }
    }
}
