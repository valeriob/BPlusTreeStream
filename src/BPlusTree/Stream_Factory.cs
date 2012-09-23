using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BPlusTree
{
    public interface IStream_Factory
    {
        Stream Create_ReadOnly_Index_Stream();
        Stream Create_ReadWrite_Index_Stream();

        Stream Create_ReadOnly_Data_Stream();
        Stream Create_ReadWrite_Data_Stream();

        Stream Create_ReadWrite_Metadata_Stream();

        void Clear();
    }

    public class File_Stream_Factory : IStream_Factory
    {
        string _directory;
        string indexFile = "index.dat";
        string metadataFile = "metadata.dat";
        string dataFile = "data.dat";

        public File_Stream_Factory(string folder_path)
        {
            if (!Directory.Exists(folder_path))
                Directory.CreateDirectory(folder_path);
            _directory = folder_path;
        }

        private void Init()
        {
            if (!File.Exists(indexFile))
                File.Delete(indexFile);
            if (File.Exists(metadataFile))
                File.Delete(metadataFile);
            if (File.Exists(dataFile))
                File.Delete(dataFile);
        }


        public Stream Create_ReadOnly_Index_Stream()
        {
            var path = Path.Combine(_directory, indexFile);
            return new FileStream(path, FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan);
        }

        public Stream Create_ReadWrite_Index_Stream()
        {
            var path = Path.Combine(_directory, indexFile);
            return new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Write, 4096, FileOptions.SequentialScan);
        }



        public Stream Create_ReadWrite_Data_Stream()
        {
            var path = Path.Combine(_directory, dataFile);
            return new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read, 4096, FileOptions.SequentialScan);
        }


        public Stream Create_ReadOnly_Data_Stream()
        {
            var path = Path.Combine(_directory, dataFile);
            return new FileStream(path, FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan);
        }


        public Stream Create_ReadWrite_Metadata_Stream()
        {
            var path = Path.Combine(_directory, metadataFile);
            return new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read, 4096, FileOptions.SequentialScan);
        }


        public void Clear()
        {
            if (Directory.Exists(_directory))
            {
                foreach (var file in Directory.EnumerateFiles(_directory))
                    File.Delete(file);
            }
        }
    }
}
