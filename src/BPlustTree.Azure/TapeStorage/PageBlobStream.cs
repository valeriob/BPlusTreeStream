//using System;
//using System.IO;
//using System.Linq;
//using System.ComponentModel;
//using System.Collections.Generic;
//using Microsoft.WindowsAzure.StorageClient;
//using Lokad.Cqrs.TapeStorage;

//namespace BPlustTree.Azure.TapeStorage
//{
//    public class PageBlobStream : Stream
//    {
//        readonly CloudPageBlob _blob;
//        readonly BinaryReader _reader;
//        long _pageIndex;
//        Int16 _offset;

//        public const Int16 PageSize = 512;
//        public const Int16 SizeOfPageDataSize = sizeof(Int16);
//        public const Int16 PageDataSize = PageSize - SizeOfPageDataSize;


//        public PageBlobStream(CloudPageBlob blob)
//        {
//            _blob = blob;

//            if (!_blob.Exists())
//                throw new ArgumentException();

//            _reader = new BinaryReader(_blob.OpenRead());
//        }


//        long _position;
//        public override long Position { get { return _position; } set { throw new NotImplementedException(); } }

//        public override bool CanRead { get { return true; } }
//        public override bool CanSeek{ get { return true; } }
//        public override bool CanWrite { get { return true; } }
//        public override void Flush() { }
//        public override long Length
//        {
//            get
//            {
//                _blob.FetchAttributes();

//                if (_blob.Attributes.Properties.Length == 0)
//                    return 0;

//                var pageIndex = (_blob.Attributes.Properties.Length / PageBlobAppendStream.PageSize) - 1;

//                _reader.BaseStream.Seek(pageIndex * PageBlobAppendStream.PageSize, SeekOrigin.Begin);
//                var offset = _reader.ReadInt16(); // Must read PageDataLengthType type

//                return pageIndex * PageBlobAppendStream.PageDataSize + offset;
//            }
//        }

//        public override long Seek(long offset, SeekOrigin origin)
//        {
//            long streamOffset;
//            long length = -1; // To cache Length's HTTP request
//            switch (origin)
//            {
//                case SeekOrigin.Begin:
//                    streamOffset = offset;
//                    break;
//                case SeekOrigin.Current:
//                    streamOffset = Position + offset;
//                    break;
//                case SeekOrigin.End:
//                    length = Length; // Length will send HTTP request
//                    streamOffset = length - offset;
//                    break;
//                default:
//                    throw new InvalidEnumArgumentException("origin", (int)origin, typeof(SeekOrigin));
//            }

//            if (length == -1)
//                length = Length; // Length will send HTTP request

//            if (streamOffset < 0 || streamOffset > length)
//                throw new ArgumentOutOfRangeException("offset");

//            Position = streamOffset;

//            return streamOffset;
//        }


//        public override void SetLength(long value)
//        {
//            throw new NotImplementedException();
//        }

//        public override int Read(byte[] buffer, int offset, int count)
//        {
//            if (count == 0)
//                return 0;

//            if (Position + count > Length) // Length will send HTTP request
//                return 0;

//            int rem;
//            var pagesToRead = Math.DivRem(_offset + count, PageBlobAppendStream.PageDataSize, out rem);
//            if (rem > 0)
//                pagesToRead++;

//            var pagesBuffer = ReadPages(pagesToRead);

//            using (var targetWriter = new BinaryWriter(new MemoryStream(buffer)))
//            using (var pageReader = new BinaryReader(new MemoryStream(pagesBuffer)))
//            {
//                targetWriter.BaseStream.Seek(offset, SeekOrigin.Begin);

//                var rest = count;

//                do
//                {
//                    var pageDataSize = pageReader.ReadInt16(); // Must read PageDataLengthType type
//                    // Move to current reading position
//                    if (_offset != 0)
//                        pageReader.ReadBytes(_offset);

//                    var bytesToRead = (Int16)(Math.Min(_offset + rest, pageDataSize) - _offset);
//                    rest -= bytesToRead;

//                    var bytes = pageReader.ReadBytes(bytesToRead);
//                    targetWriter.Write(bytes);

//                    _offset += bytesToRead;
//                    if (_offset != PageBlobAppendStream.PageDataSize)
//                        continue;

//                    _offset = 0;
//                    _pageIndex++;
//                } while (rest > 0);
//            }

//            return count;
//        }

//        byte[] ReadPages(int count)
//        {
//            _reader.BaseStream.Seek(_pageIndex * PageBlobAppendStream.PageSize, SeekOrigin.Begin);
//            var buffer = new byte[count * PageBlobAppendStream.PageSize];
//            _reader.Read(buffer, 0, buffer.Length);

//            return buffer;
//        }




//        public override void Write(byte[] buffer, int offset, int count)
//        {
//            var page = ReadPage(Position);

//            var writePageIndex = Position;
//            if (page == null || page.FreeSpace == 0)
//            {
//                page = new Page();
//                writePageIndex++;
//            }

//            var rest = count;
//            var bufferOffset = offset;

//            var pages = new List<Page> { page };

//            do
//            {
//                var bytesToWrite = (Int16)Math.Min(rest, page.FreeSpace);
//                rest -= bytesToWrite;

//                page.Append(buffer, bufferOffset, bytesToWrite);
//                bufferOffset += bytesToWrite;

//                var needMorePages = rest > 0 && page.FreeSpace == 0;
//                if (!needMorePages)
//                    continue;

//                page = new Page();
//                pages.Add(page);
//            } while (rest > 0);

//            if (pages.Count * PageSize > 4 * 1024 * 1024)
//                throw new NotSupportedException("Writing more than 4 Mb not supported.");

//            var pagesAdded = pages.Count - 1 + writePageIndex - Position;
//            if (pagesAdded > 0)
//            {
//                _blobLength += pagesAdded * PageSize;
//                _blob.SetLength(_blobLength);
//            }

//            WritePages(pages, writePageIndex);
//            _position += pagesAdded;
//        }

//        void WritePages(ICollection<Page> pages, long startPageIndex)
//        {
//            var buffer = new byte[pages.Count * PageSize];

//            using (var ms = new MemoryStream(buffer))
//            {
//                foreach (var pageBuffer in pages.Select(page => page.GetBuffer()))
//                    ms.Write(pageBuffer, 0, pageBuffer.Length);

//                ms.Seek(0, SeekOrigin.Begin);
//                var offset = startPageIndex * PageSize;
//                _blob.WritePages(ms, offset);

//            }

//            _cachedPage = pages.Last();
//            _cachedPageIndex = startPageIndex + pages.Count - 1;
//        }

//        Page ReadPage(long index)
//        {
//            if (index < 0)
//                return null;

//            if (index == _cachedPageIndex)
//                return _cachedPage;

//            var buffer = new byte[PageSize];

//            _reader.BaseStream.Seek(index * PageSize, SeekOrigin.Begin);
//            _reader.Read(buffer, 0, PageSize);

//            return new Page(buffer);
//        }



//        class Page
//        {
//            readonly byte[] _data = new byte[PageDataSize];

//            public Page()
//            {
//                Length = 0;
//            }

//            public Page(byte[] buffer)
//            {
//                using (var br = new BinaryReader(new MemoryStream(buffer)))
//                {
//                    br.BaseStream.Seek(0, SeekOrigin.Begin);
//                    Length = br.ReadInt16(); // Must read PageDataLengthType type
//                    br.Read(_data, 0, PageDataSize);
//                }
//            }

//            public short Length { get; private set; }

//            public Int16 FreeSpace
//            {
//                get { return (Int16)(PageDataSize - Length); }
//            }

//            public void Append(byte[] buffer, int offset, Int16 count)
//            {
//                if (Length + count > PageDataSize)
//                    throw new ArgumentOutOfRangeException("count");

//                using (var bw = new BinaryWriter(new MemoryStream(_data)))
//                {
//                    bw.BaseStream.Seek(Length, SeekOrigin.Begin);
//                    bw.Write(buffer, offset, count);
//                    Length += count;
//                }
//            }

//            public byte[] GetBuffer()
//            {
//                var bytes = new byte[PageSize];
//                using (var bw = new BinaryWriter(new MemoryStream(bytes)))
//                {
//                    bw.Write(Length);
//                    bw.Write(_data);

//                    return bytes;
//                }
//            }
//        }
//    }
//}
