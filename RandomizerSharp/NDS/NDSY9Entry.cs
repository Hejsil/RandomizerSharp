using System;

namespace RandomizerSharp.NDS
{
    public class Ndsy9Entry
    {
        private readonly NdsRom _parent;
        private bool _decompressedData;
        public string ExtFilename { get; }
        public int BssSize { get; set; }
        public int CompressedSize { get; set; }
        public int CompressFlag { get; set; }
        public byte[] Data { get; set; }
        public int FileId { get; set; }
        public int Offset { get; set; }
        public int OriginalSize { get; set; }
        public int OverlayId { get; set; }
        public int RamAddress { get; set; }
        public int RamSize { get; set; }
        public int Size { get; set; }
        public int StaticEnd { get; set; }
        public int StaticStart { get; set; }

        public Ndsy9Entry(NdsRom parent) => _parent = parent;

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