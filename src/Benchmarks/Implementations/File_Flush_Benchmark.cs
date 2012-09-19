using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Benchmarks
{
    public class File_Flush_Benchmark : Benchmark
    {
       // File
            Stream stream;
        public File_Flush_Benchmark()
        { 
            var fileName = @"flush_Test.dat";
            if (File.Exists(fileName))
                File.Delete(fileName);

            //stream = File.Open(fileName, FileMode.OpenOrCreate);

            //stream = new MemoryStream();
            stream = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, 4096,
                FileOptions.WriteThrough | FileOptions.SequentialScan);
        }


        public override void Run(int count, int batch)
        {
            for (int i = 0; i < count; i += batch)
            {
                for (int j = i; j < i + batch; j++)
                {
                    var buff = BitConverter.GetBytes(j);
                    stream.Write(buff, 0, 4);
                }
                stream.Flush();
            }
        }

    }
}
