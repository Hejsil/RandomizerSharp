using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace RandomizerSharp
{
    public class FileFunctions
    {
        public static string FixFilename(string original, string defaultExtension)
        {
            return FixFilename(original, defaultExtension, null);
        }

        public static string FixFilename(string original, string defaultExtension, IList<string> bannedExtensions)
        {
            var filename = Path.GetFileName(original);
            if (filename != null && filename.LastIndexOf('.') >= filename.Length - 5 && filename.LastIndexOf('.') != filename.Length - 1 && filename.Length > 4 && filename.LastIndexOf('.') != -1)
            {
                var ext = filename.Substring(filename.LastIndexOf('.') + 1).ToLower();
                if (bannedExtensions != null && bannedExtensions.Contains(ext))
                    filename = filename.Substring(0, filename.LastIndexOf('.') + 1) + defaultExtension;
            }
            else
            {
                filename += "." + defaultExtension;
            }
            return Path.GetFullPath(original).Replace(filename, "") + filename;
        }

        public static int ReadFullInt(byte[] data, int offset)
        {
            var buf = ByteBuffer.Allocate(4).Put(data, offset, 4);
            buf.Rewind();
            return buf.GetInt();
        }

        public static int Read2ByteInt(byte[] data, int index)
        {
            return (data[index] & 0xFF) | ((data[index + 1] & 0xFF) << 8);
        }

        public static void WriteFullInt(byte[] data, int offset, int value)
        {
            Array.Copy(BitConverter.GetBytes(value), 0, data, offset, 4);
        }

        public static byte[] ReadFileFullyIntoBuffer(string filename)
        {
            return File.ReadAllBytes(filename);
        }
        
        public static byte[] ReadFullyIntoBuffer(Stream @in, int bytes)
        {
            var buf = new byte[bytes];
            ReadFully(@in, buf, 0, bytes);
            return buf;
        }
        public static void ReadFully(Stream @in, byte[] buf, int offset, int length)
        {
            int offs = 0, read;
            while (offs < length && (read = @in.Read(buf, offs + offset, length - offs)) != -1)
                offs += read;
        }
        
        public static void WriteBytesToFile(string filename, byte[] data)
        {
            var fos = new FileStream(filename, FileMode.Create, FileAccess.Write);
            fos.Write(data, 0, data.Length);
            fos.Close();
        }

        public static byte[] DownloadFile(string url)
        {
            using (var client = new WebClient())
            {
                return client.DownloadData("http://example.com/file/song/a.mpeg");
            }
        }
        
        public static void ApplyPatch(byte[] rom, byte[] patch)
        {
            var patchlen = patch.Length;
            if (patchlen < 8 || patch[0] != 'P' || patch[1] != 'A' || patch[2] != 'T' || patch[3] != 'C' ||
                patch[4] != 'H')
                throw new IOException("not a valid IPS file");
            var offset = 5;
            while (offset + 2 < patchlen)
            {
                var writeOffset = ReadIpsOffset(patch, offset);
                if (writeOffset == 0x454f46)
                    return;
                offset += 3;
                if (offset + 1 >= patchlen)
                    throw new IOException("abrupt ending to IPS file, entry cut off before size");
                var size = ReadIpsSize(patch, offset);
                offset += 2;
                if (size == 0)
                {
                    if (offset + 1 >= patchlen)
                        throw new IOException("abrupt ending to IPS file, entry cut off before RLE size");
                    var rleSize = ReadIpsSize(patch, offset);
                    if (writeOffset + rleSize > rom.Length)
                        throw new IOException("trying to patch data past the end of the ROM file");
                    offset += 2;
                    if (offset >= patchlen)
                        throw new IOException("abrupt ending to IPS file, entry cut off before RLE byte");
                    var rleByte = patch[offset++];
                    for (var i = writeOffset; i < writeOffset + rleSize; i++)
                        rom[i] = rleByte;
                }
                else
                {
                    if (offset + size > patchlen)
                        throw new IOException("abrupt ending to IPS file, entry cut off before end of data block");
                    if (writeOffset + size > rom.Length)
                        throw new IOException("trying to patch data past the end of the ROM file");
                    Array.Copy(patch, offset, rom, writeOffset, size);
                    offset += size;
                }
            }
            throw new IOException("improperly terminated IPS file");
        }

        private static int ReadIpsOffset(byte[] data, int offset)
        {
            return ((data[offset] & 0xFF) << 16) | ((data[offset + 1] & 0xFF) << 8) | (data[offset + 2] & 0xFF);
        }

        private static int ReadIpsSize(byte[] data, int offset)
        {
            return ((data[offset] & 0xFF) << 8) | (data[offset + 1] & 0xFF);
        }

        public static byte[] ConvIntArrToByteArr(int[] arg)
        {
            var @out = new byte[arg.Length];
            for (var i = 0; i < arg.Length; i++)
                @out[i] = (byte) arg[i];
            return @out;
        }
    }

    //-------------------------------------------------------------------------------------------
    //	Copyright © 2007 - 2017 Tangible Software Solutions Inc.
    //	This class can be used by anyone provided that the copyright notice remains intact.
    //
    //	This class is used to replicate the java.nio.ByteBuffer class in C#.
    //
    //	Instances are only obtainable via the static 'allocate' method.
    //
    //	Some methods are not available:
    //		All methods which create shared views of the buffer such as: array,
    //		asCharBuffer, asDoubleBuffer, asFloatBuffer, asIntBuffer, asLongBuffer,
    //		asReadOnlyBuffer, asShortBuffer, duplicate, slice, & wrap.
    //
    //		Other methods such as: mark, reset, isReadOnly, order, compareTo,
    //		arrayOffset, & the limit setter method.
    //-------------------------------------------------------------------------------------------
    public class ByteBuffer
    {
        private Mode _mode;
        private readonly BinaryReader _reader;

        private MemoryStream _stream;
        private readonly BinaryWriter _writer;

        private ByteBuffer()
        {
            _stream = new MemoryStream();
            _reader = new BinaryReader(_stream);
            _writer = new BinaryWriter(_stream);
        }

        ~ByteBuffer()
        {
            _reader.Close();
            _writer.Close();
            _stream.Close();
            _stream.Dispose();
        }

        public static ByteBuffer Allocate(int capacity)
        {
            var buffer = new ByteBuffer
            {
                _stream = {Capacity = capacity},
                _mode = Mode.Write
            };
            return buffer;
        }

        public static ByteBuffer AllocateDirect(int capacity)
        {
            //this wrapper class makes no distinction between 'allocate' & 'allocateDirect'
            return Allocate(capacity);
        }

        public int Capacity()
        {
            return _stream.Capacity;
        }

        public ByteBuffer Flip()
        {
            _mode = Mode.Read;
            _stream.SetLength(_stream.Position);
            _stream.Position = 0;
            return this;
        }

        public ByteBuffer Clear()
        {
            _mode = Mode.Write;
            _stream.Position = 0;
            return this;
        }

        public ByteBuffer Compact()
        {
            _mode = Mode.Write;
            var newStream = new MemoryStream(_stream.Capacity);
            _stream.CopyTo(newStream);
            _stream = newStream;
            return this;
        }

        public ByteBuffer Rewind()
        {
            _stream.Position = 0;
            return this;
        }

        public long Limit()
        {
            if (_mode == Mode.Write)
                return _stream.Capacity;
            return _stream.Length;
        }

        public long Position()
        {
            return _stream.Position;
        }

        public ByteBuffer Position(long newPosition)
        {
            _stream.Position = newPosition;
            return this;
        }

        public long Remaining()
        {
            return Limit() - Position();
        }

        public bool HasRemaining()
        {
            return Remaining() > 0;
        }

        public int Get()
        {
            return _stream.ReadByte();
        }

        public ByteBuffer Get(byte[] dst, int offset, int length)
        {
            _stream.Read(dst, offset, length);
            return this;
        }

        public ByteBuffer Put(byte b)
        {
            _stream.WriteByte(b);
            return this;
        }

        public ByteBuffer Put(byte[] src, int offset, int length)
        {
            _stream.Write(src, offset, length);
            return this;
        }

        public bool Equals(ByteBuffer other)
        {
            if (other != null && Remaining() == other.Remaining())
            {
                var thisOriginalPosition = Position();
                var otherOriginalPosition = other.Position();

                var differenceFound = false;
                while (_stream.Position < _stream.Length)
                    if (Get() != other.Get())
                    {
                        differenceFound = true;
                        break;
                    }

                Position(thisOriginalPosition);
                other.Position(otherOriginalPosition);

                return !differenceFound;
            }
            return false;
        }

        //methods using the internal BinaryReader:
        public char GetChar()
        {
            return _reader.ReadChar();
        }

        public char GetChar(int index)
        {
            var originalPosition = _stream.Position;
            _stream.Position = index;
            var value = _reader.ReadChar();
            _stream.Position = originalPosition;
            return value;
        }

        public double GetDouble()
        {
            return _reader.ReadDouble();
        }

        public double GetDouble(int index)
        {
            var originalPosition = _stream.Position;
            _stream.Position = index;
            var value = _reader.ReadDouble();
            _stream.Position = originalPosition;
            return value;
        }

        public float GetFloat()
        {
            return _reader.ReadSingle();
        }

        public float GetFloat(int index)
        {
            var originalPosition = _stream.Position;
            _stream.Position = index;
            var value = _reader.ReadSingle();
            _stream.Position = originalPosition;
            return value;
        }

        public int GetInt()
        {
            return _reader.ReadInt32();
        }

        public int GetInt(int index)
        {
            var originalPosition = _stream.Position;
            _stream.Position = index;
            var value = _reader.ReadInt32();
            _stream.Position = originalPosition;
            return value;
        }

        public long GetLong()
        {
            return _reader.ReadInt64();
        }

        public long GetLong(int index)
        {
            var originalPosition = _stream.Position;
            _stream.Position = index;
            var value = _reader.ReadInt64();
            _stream.Position = originalPosition;
            return value;
        }

        public short GetShort()
        {
            return _reader.ReadInt16();
        }

        public short GetShort(int index)
        {
            var originalPosition = _stream.Position;
            _stream.Position = index;
            var value = _reader.ReadInt16();
            _stream.Position = originalPosition;
            return value;
        }

        //methods using the internal BinaryWriter:
        public ByteBuffer PutChar(char value)
        {
            _writer.Write(value);
            return this;
        }

        public ByteBuffer PutChar(int index, char value)
        {
            var originalPosition = _stream.Position;
            _stream.Position = index;
            _writer.Write(value);
            _stream.Position = originalPosition;
            return this;
        }

        public ByteBuffer PutDouble(double value)
        {
            _writer.Write(value);
            return this;
        }

        public ByteBuffer PutDouble(int index, double value)
        {
            var originalPosition = _stream.Position;
            _stream.Position = index;
            _writer.Write(value);
            _stream.Position = originalPosition;
            return this;
        }

        public ByteBuffer PutFloat(float value)
        {
            _writer.Write(value);
            return this;
        }

        public ByteBuffer PutFloat(int index, float value)
        {
            var originalPosition = _stream.Position;
            _stream.Position = index;
            _writer.Write(value);
            _stream.Position = originalPosition;
            return this;
        }

        public ByteBuffer PutInt(int value)
        {
            _writer.Write(value);
            return this;
        }

        public ByteBuffer PutInt(int index, int value)
        {
            var originalPosition = _stream.Position;
            _stream.Position = index;
            _writer.Write(value);
            _stream.Position = originalPosition;
            return this;
        }

        public ByteBuffer PutLong(long value)
        {
            _writer.Write(value);
            return this;
        }

        public ByteBuffer PutLong(int index, long value)
        {
            var originalPosition = _stream.Position;
            _stream.Position = index;
            _writer.Write(value);
            _stream.Position = originalPosition;
            return this;
        }

        public ByteBuffer PutShort(short value)
        {
            _writer.Write(value);
            return this;
        }

        public ByteBuffer PutShort(int index, short value)
        {
            var originalPosition = _stream.Position;
            _stream.Position = index;
            _writer.Write(value);
            _stream.Position = originalPosition;
            return this;
        }

        //'Mode' is only used to determine whether to return data length or capacity from the 'limit' method:
        private enum Mode
        {
            Read,
            Write
        }
    }
}