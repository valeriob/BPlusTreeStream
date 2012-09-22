using System;
using System.IO;
using System.Linq;
using System.ComponentModel;
using System.Collections.Generic;
using Microsoft.WindowsAzure.StorageClient;
using Lokad.Cqrs.TapeStorage;
using System.Threading;

namespace BPlustTree.Azure.TapeStorage
{
    public class Aligned_PageBlobStream : Stream
    {
        readonly CloudPageBlob _blob;
        //readonly BinaryReader _reader;
        //readonly Stream _stream;

        public const Int16 PageSize = 512;
        public const Int16 SizeOfPageDataSize = sizeof(Int16);
        public const Int16 PageDataSize = PageSize - SizeOfPageDataSize;


        public Aligned_PageBlobStream(CloudPageBlob blob)
        {
            _blob = blob;

            if (!_blob.Exists())
                throw new ArgumentException();
            //_stream = _blob.OpenRead();
           // _reader = new BinaryReader(_blob.OpenRead());
        }


        long _position;
        public override long Position { get { return _position; } set { throw new NotImplementedException(); } }
        public override bool CanRead { get { return true; } }
        public override bool CanSeek { get { return true; } }
        public override bool CanWrite { get { return true; } }
        public override void Flush() { }

        long? _length;
        public override long Length
        {
            get
            {
                if (_length == null)
                {
                    _blob.FetchAttributes();
                    _length = _blob.Attributes.Properties.Length;
                    return _blob.Attributes.Properties.Length;

                    //if (_blob.Attributes.Properties.Length == 0)
                    //    return 0;

                    //var pageIndex = (_blob.Attributes.Properties.Length / PageBlobAppendStream.PageSize) - 1;

                    ////_reader.BaseStream.Seek(pageIndex * PageBlobAppendStream.PageSize, SeekOrigin.Begin);
                    ////var offset = _reader.ReadInt16(); // Must read PageDataLengthType type

                    //return pageIndex * PageBlobAppendStream.PageSize;// +offset;
                }
                return _length.Value;
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            long streamOffset;
            long length = -1; // To cache Length's HTTP request
            switch (origin)
            {
                case SeekOrigin.Begin:
                    streamOffset = offset;
                    break;
                //case SeekOrigin.Current:
                //    streamOffset = Position + offset;
                //    break;
                //case SeekOrigin.End:
                //    length = Length; // Length will send HTTP request
                //    streamOffset = length - offset;
                //    break;
                default:
                    throw new InvalidEnumArgumentException("origin", (int)origin, typeof(SeekOrigin));
            }

            if (length == -1)
                length = Length; // Length will send HTTP request

            if (streamOffset < 0 || streamOffset > length)
                throw new ArgumentOutOfRangeException("offset");

            if (offset % PageSize != 0)
                throw new Exception("offset not page aligned");

            //_reader.BaseStream.Seek(streamOffset, SeekOrigin.Begin);
            //_stream.Seek(streamOffset, SeekOrigin.Begin);
            _position = streamOffset;

            return Position;
        }


        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (count % PageSize != 0)
                throw new Exception("offset not page aligned");

            if (count == 0)
                return 0;

            long len = Length;
            if (Position + count > len) // Length will send HTTP request
                return 0;

            using (var _stream = _blob.OpenRead())
            {
                _stream.Seek(Position, SeekOrigin.Begin);
                int readBytes = _stream.Read(buffer, offset, count);

                _position += readBytes;
                return readBytes;
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (count % PageSize != 0)
                throw new Exception("offset not page aligned");

            using (var stream = new MemoryStream(buffer, offset, count))
            {
                stream.Seek(0, SeekOrigin.Begin);
                _blob.WritePages(stream, Position);
                //_blob.FetchAttributes();
            }

            _position += count;
        }

    }
}
