using System;
using System.IO;
using System.Text.RegularExpressions;

namespace RandomizerSharp.NDS
{
    public class Ndsy9Entry
    {

        public int BssSize;
        public int CompressFlag;
        public int CompressedSize;
        public byte[] Data;
        private bool _decompressedData;
        public string ExtFilename;
        public int FileId;
        public int Offset, Size, OriginalSize;
        public int OverlayId;

        private readonly NdsRom _parent;
        public int RamAddress, RamSize;
        public int StaticStart, StaticEnd;

        public Ndsy9Entry(NdsRom parent)
        {
            _parent = parent;
        }

        // returns null if no override
        public ArraySlice<byte> OverrideContents()
        {
            var buf = GetContents();

            if (!_decompressedData)
                return buf;

            Data = BlzCoder.Encode(buf, false, "overlay " + OverlayId);
            _decompressedData = false;

            // update our compressed size
            CompressedSize = Data.Length;
            return Data;
        }
        
        public ArraySlice<byte> GetContents()
        {
            if (Data != null)
                return Data;

            var rom = _parent.BaseRom;
            Data = new byte[OriginalSize];
            rom.Seek(Offset);
            rom.ReadFully(Data);

            return Data;
        }
        
        public virtual void WriteOverride(ArraySlice<byte> data)
        {
            GetContents();

            Size = data.Length;
            
            // Compression?
            if (CompressFlag != 0 && OriginalSize == CompressedSize && CompressedSize != 0)
                Data = BlzCoder.Decode(Data, "overlay " + OverlayId);

            _decompressedData = true;

            if (Data.Length == data.Length)
            {
                data.CopyTo(Data, 0);
            }
            else
            {
                var newData = new byte[data.Length];
                Array.Copy(data, 0, Data, 0, data.Length);

                Data = newData;
            }
        }
    }
}