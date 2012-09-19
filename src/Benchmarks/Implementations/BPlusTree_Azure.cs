﻿using BPlusTree;
using BPlusTree.Core;
using BPlusTree.Core.Serializers;
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

        public BPlusTree_Azure()
        {
            var container_Name = "bplustree";
            var indexFile = "index.dat";
            var metadataFile = "metadata.dat";
            var dataFile = "data.dat";

            if (File.Exists(indexFile))
                File.Delete(indexFile);
            if (File.Exists(metadataFile))
                File.Delete(metadataFile);
            if (File.Exists(dataFile))
                File.Delete(dataFile);

            var account = CloudStorageAccount.DevelopmentStorageAccount;
            var blobClient =  account.CreateCloudBlobClient();

            var container = blobClient.GetContainerReference(container_Name);
            container.CreateIfNotExist();

            var indexBlob = container.GetPageBlobReference(indexFile);
            indexBlob.DeleteIfExists();
            indexBlob.Create(10 * 1024 * 1024);
            var indexStream = new Lokad.Cqrs.TapeStorage.PageBlobAppendStream(indexBlob);


            var metadataBlob = container.GetPageBlobReference(indexFile);
            metadataBlob.DeleteIfExists();
            metadataBlob.Create(1024);
            var metadataStream = new Lokad.Cqrs.TapeStorage.PageBlobAppendStream(metadataBlob);


            var dataBlob = container.GetPageBlobReference(indexFile);
            dataBlob.DeleteIfExists();
            dataBlob.Create(1024);
            var dataStream = new Lokad.Cqrs.TapeStorage.PageBlobAppendStream(dataBlob);


            var appendBpTree = new BPlusTree<int>(metadataStream, indexStream, dataStream, 128, serializer);
            tree = new String_BPlusTree<int>(appendBpTree);
        }

        public override void Prepare(int count, int batch)
        {
            return;

            for (int i = 0; i < count; i += batch)
            {
                for (var j = i; j < i + batch; j++)
                {
                    var g = Guid.NewGuid();
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
                    tree.Put(j , "text about " + j);
                    //result = tree.Get(j);
                    //for (int k = j; k >= 0; k--)
                    //    result = tree.Get(k +"");
                }
                tree.Commit();

                //for (int k = i + batch - 1; k >= 0; k--)
                //    result = tree.Get(k +"");
            }


            ///  Read Only
            //for (int i = 0; i < number_Of_Inserts; i++)
            //{
            //    result = tree.Get(i);
            //}

            var inner = tree.BPlusTree as BPlusTree<int>;
        }

    }




}
