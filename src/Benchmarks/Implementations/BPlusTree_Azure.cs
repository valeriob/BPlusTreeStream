using BPlusTree;
using BPlusTree.Core;
using BPlusTree.Core.Serializers;
using BPlustTree.Azure.TapeStorage;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Benchmarks
{
    public class BPlusTree_Azure : Benchmark
    {
        String_BPlusTree<int> tree;
        ISerializer<int> serializer = new Int_Serializer();
        Random random = new Random(DateTime.Now.Millisecond);

        public BPlusTree_Azure()
        {
            var container_Name = "bplustree";
            var indexFile = "index.dat";
            var metadataFile = "metadata.dat";
            var dataFile = "data.dat";

            //if (File.Exists(indexFile))
            //    File.Delete(indexFile);
            //if (File.Exists(metadataFile))
            //    File.Delete(metadataFile);
            //if (File.Exists(dataFile))
            //    File.Delete(dataFile);

            long maxValue = uint.MaxValue - uint.MaxValue % 512;

            var account = CloudStorageAccount.DevelopmentStorageAccount;
            
            account = new CloudStorageAccount(new StorageCredentialsAccountAndKey("valeriob", "2SzgTAaG11U0M1gQ19SNus/vv1f0efwYOwZHL1w9YhTKEYsU1ul+s/ke92DOE1wIeCKYz5CuaowtDceUvZW2Rw=="),true);
            var blobClient =  account.CreateCloudBlobClient();

            var container = blobClient.GetContainerReference(container_Name);
            container.CreateIfNotExist();

            var indexBlob = container.GetPageBlobReference(indexFile);
            indexBlob.DeleteIfExists();
            indexBlob.Create(maxValue);
            var indexStream = new Aligned_PageBlobStream(indexBlob);


            var metadataBlob = container.GetPageBlobReference(metadataFile);
            metadataBlob.DeleteIfExists();
            metadataBlob.Create(maxValue);
            var metadataStream = new Aligned_PageBlobStream(metadataBlob);


            var dataBlob = container.GetPageBlobReference(dataFile);
            dataBlob.DeleteIfExists();
            dataBlob.Create(maxValue);
            var dataStream = new Aligned_PageBlobStream(dataBlob);

            

            var appendBpTree = new BPlusTree<int>(metadataStream, indexStream, dataStream, 128, 512, 0 , serializer);
            tree = new String_BPlusTree<int>(appendBpTree);
        }

        public override void Prepare(int count, int batch)
        {
            return;
            for (int i = 0; i < count; i += batch)
            {
                for (var j = i; j < i + batch; j++)
                {
                   // var g = Guid.NewGuid();
                    tree.Put(j , "text about " + j);
                }
                tree.Commit();
            }
        }

        public override void Run(int number_Of_Inserts, int batch)
        {
            string result;


            for (int i = 0; i < number_Of_Inserts; i += batch)
            {
                for (var j = i; j < i + batch; j++)
                {
                    var g = Guid.NewGuid();
                    tree.Put(j, "text about " + j);
                    //result = tree.Get(j);
                    //for (int k = j; k >= 0; k--)
                    //    result = tree.Get(k);
                }
                tree.Commit();

                //for (int k = i + batch - 1; k >= 0; k--)
                //    result = tree.Get(k);
            }


            ///  Read Only
            //for (int i = 0; i < number_Of_Inserts; i++)
            //{
            //    var rnd = random.Next(number_Of_Inserts - 1);
            //    result = tree.Get(rnd);
            //}

            var inner = tree.BPlusTree as BPlusTree<int>;
        }

    }


    public class Azure_Stream_Factory : IStream_Factory
    {
        CloudBlobClient _blobClient;
        CloudBlobContainer container;
        string _container_Name = "bplustree";

        string indexBlobName = "index.dat";
        string metadataBlobName = "metadata.dat";
        string dataBlobName = "data.dat";
        uint maxValue = uint.MaxValue - uint.MaxValue % 512;

        public Azure_Stream_Factory(CloudStorageAccount account, string container_Name)
        {
            _container_Name = container_Name;

            _blobClient = account.CreateCloudBlobClient();

            container = _blobClient.GetContainerReference(container_Name);
            container.CreateIfNotExist();
        }


        public Stream Create_ReadOnly_Index_Stream()
        {
            var blob = container.GetPageBlobReference(indexBlobName);
            blob.DeleteIfExists();
            blob.Create(maxValue);
            var stream = new Lokad.Cqrs.TapeStorage.PageBlobReadStream(blob);
            return stream;
        }

        public Stream Create_ReadWrite_Index_Stream()
        {
            var blob = container.GetPageBlobReference(indexBlobName);
            blob.DeleteIfExists();
            blob.Create(maxValue);
            var stream = new Lokad.Cqrs.TapeStorage.PageBlobAppendStream(blob);
            return stream;
        }



        public Stream Create_ReadWrite_Data_Stream()
        {
            var blob = container.GetPageBlobReference(dataBlobName);
            blob.DeleteIfExists();
            blob.Create(maxValue);
            var stream = new Lokad.Cqrs.TapeStorage.PageBlobReadStream(blob);
            return stream;
        }


        public Stream Create_ReadOnly_Data_Stream()
        {
            var blob = container.GetPageBlobReference(dataBlobName);
            blob.DeleteIfExists();
            blob.Create(maxValue);
            var stream = new Lokad.Cqrs.TapeStorage.PageBlobAppendStream(blob);
            return stream;
        }


        public Stream Create_ReadWrite_Metadata_Stream()
        {
            var blob = container.GetPageBlobReference(metadataBlobName);
            blob.DeleteIfExists();
            blob.Create(maxValue);
            var stream = new Lokad.Cqrs.TapeStorage.PageBlobReadStream(blob);
            return stream;
        }



        public void Clear()
        {
            var blobs = container.ListBlobs();
            foreach (var blob in blobs)
                _blobClient.GetBlobReference(blob.Uri+"").DeleteIfExists();
        }
    }

}
