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
    }

    public class File_Stream_Factory : IStream_Factory
    {
        DirectoryInfo _directory;
        string indexFile = "index.dat";
        string metadataFile = "metadata.dat";
        string dataFile = "data.dat";


        public File_Stream_Factory(DirectoryInfo directory)
        {
            _directory = directory;
        }
        public File_Stream_Factory(string folder_path)
        {
            _directory = new DirectoryInfo(folder_path);
        }


        public Stream Create_ReadOnly_Index_Stream()
        {
            return new FileStream(indexFile, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan);
        }

        public Stream Create_ReadWrite_Index_Stream()
        {
            return new FileStream(indexFile, FileMode.Open, FileAccess.ReadWrite, FileShare.Write, 4096, FileOptions.SequentialScan);
        }



        public Stream Create_ReadWrite_Data_Stream()
        {
            return new FileStream(dataFile, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan);
        }


        public Stream Create_ReadOnly_Data_Stream()
        {
            return new FileStream(dataFile, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan);
        }


        public Stream Create_ReadWrite_Metadata_Stream()
        {
            return new FileStream(metadataFile, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan);
        }
    }
}
