using BPlusTree;
using BPlusTree.Core;
using BPlusTree.Core.Serializers;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Benchmarks
{
    public class BPlusTree : Benchmark
    {
        String_BPlusTree<int> tree;
        Stream indexStream;
        IKey_Serializer<int> serializer = new Int_Serializer();
        Random random = new Random(DateTime.Now.Millisecond);

        public BPlusTree()
        {
            var indexFile = "index.dat";
            var metadataFile = "metadata.dat";
            var dataFile = "data.dat";

            //if (File.Exists(indexFile))
            //    File.Delete(indexFile);
            //if (File.Exists(metadataFile))
            //    File.Delete(metadataFile);
            //if (File.Exists(dataFile))
            //    File.Delete(dataFile);

            //indexStream = new MemoryStream();
            //var dataStream = new MemoryStream();

            var metadataStream = new FileStream(metadataFile, FileMode.OpenOrCreate);
            indexStream = new FileStream(indexFile, FileMode.OpenOrCreate);
            //indexStream = new FileStream(indexFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, 4096,
            //    FileOptions.WriteThrough | FileOptions.RandomAccess);

            //var mmf = MemoryMappedFile.CreateFromFile(indexFile, FileMode.Open, "index", 0, MemoryMappedFileAccess.ReadWrite);
            //indexStream = mmf.CreateViewStream();

            var dataStream = new FileStream(dataFile, FileMode.OpenOrCreate);

            var appendBpTree = new BPlusTree<int>(metadataStream, indexStream, 
                dataStream, 128, 0, 0, serializer);
            tree = new String_BPlusTree<int>(appendBpTree);
            
        }

        public override void Prepare(int count, int batch)
        {
            return;

            for (int i = 0; i < count/10; i += batch)
            {
                for (var j = i; j < i + batch; j++)
                {
                    var g = Guid.NewGuid();
                    tree.Put(j, "abcdefghilmnopqrstuvz0123456789+abcdefghilmnopqrstuvz0123456789+abcdefghilmnopqrstuvz0123456789+abcdefghilmnopqrstuvz0123456789+");
                    //tree.Put(j , "text about " + j);
                }
                tree.Commit();
            }
        }

        public override void Run(int number_Of_Inserts, int batch)
        {
            string result;

            /// Random
            //var random = new Random();
            //for (int i = 0; i <= number_Of_Inserts; i += batch)
            //{
            //    for (var j = i; j < i + batch; j++)
            //    {
            //        int value = random.Next();
            //        tree.Put(value, "text about " + value);
            //       // result = tree.Get(value);
            //    }
            //    tree.Commit();
            //}

            //int count = number_Of_Inserts;
            //var random = new Random();
            //while (count > 0)
            //{
            //    int value = random.Next();
            //    tree.Put(value, "text about " + value);

            //    if (count % 100 == 0)
            //        tree.Commit();
            //    count--;
            //}

            //tree.Commit();
            /// Reverse
            //for (int i = number_Of_Inserts; i >= 0; i -= batch)
            //{
            //    for (var j = i; j > i - batch; j--)
            //    {
            //        tree.Put(j, "text about " + j);
            //        tree.Get(j);
            //        //for (int k = i; k <= number_Of_Inserts; k++)
            //        //    result = tree.Get(k);
            //    }
            //    tree.Commit();
            //}

            //for (int i = 0; i < number_Of_Inserts; i += batch)
            //{
            //    for (var j = i; j < i + batch; j++)
            //    {
            //        var g = Guid.NewGuid();
            //        tree.Put(j, "text about " + j);
            //        //tree.Put(j, "abcdefghilmnopqrstuvz0123456789+abcdefghilmnopqrstuvz0123456789+abcdefghilmnopqrstuvz0123456789+abcdefghilmnopqrstuvz0123456789+");

            //        //result = tree.Get(j);
            //        //for (int k = j; k >= 0; k--)
            //        //    result = tree.Get(k);
            //    }
            //    tree.Commit();

            //    //for (int k = i + batch - 1; k >= 0; k--)
            //    //    result = tree.Get(k);
            //}


            ///  Read Only

            for (int i = 0; i < number_Of_Inserts; i++)
            {
                var index = random.Next(1000000 - 1);
                result = tree.Get(index);
            }

            var inner = tree.BPlusTree as BPlusTree<int>;
            //var rgps = File_System_ES.Append.Pending_Changes._statistics_blocks_found.GroupBy(g => g).ToList();
            //int wasted = inner.Empty_Slots.Sum(s => s.Length * s.Blocks.Count);
            //var stats = inner.Empty_Slots.GroupBy(g => g.Length).ToList();

            //var usage = inner Count_Empty_Slots();
        }

    }

    public class FileStream_With_Hard_Flush : FileStream
    {
        public FileStream_With_Hard_Flush(string path, FileMode fileMode)
            : base(path, fileMode)
        { }

        public override void Flush()
        {
            base.Flush(true);
           // FlushFileBuffers(base.SafeFileHandle.DangerousGetHandle());
        }

        [DllImport("kernel32.dll")]
        static extern bool FlushFileBuffers(IntPtr hFile);
    }


}
