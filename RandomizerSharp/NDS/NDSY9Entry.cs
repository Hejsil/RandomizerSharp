using System;
using System.IO;
using System.Text.RegularExpressions;

namespace RandomizerSharp.NDS
{
    public class Ndsy9Entry
    {
        public enum Extracted
        {
            Not,
            ToFile,
            ToRam
        }

        public int BssSize;
        public int CompressFlag;
        public int CompressedSize;
        public ArraySlice<byte> Data;
        private bool _decompressedData;
        public string ExtFilename;
        public int FileId;
        public int Offset, Size, OriginalSize;
        public int OverlayId;

        private readonly NdsRom _parent;
        public int RamAddress, RamSize;
        public int StaticStart, StaticEnd;
        public Extracted Status = Extracted.Not;

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

            buf = BlzCoder.Encode(buf, false, "overlay " + OverlayId);

            // update our compressed size
            CompressedSize = buf.Length;
            return buf;
        }
        
        public ArraySlice<byte> GetContents()
        {
            switch (Status)
            {
                case Extracted.Not:
                {
                    var rom = _parent.BaseRom;
                    var buf = new byte[OriginalSize];
                    rom.Seek(Offset);
                    rom.ReadFully(buf);

                    if (_parent.WritingEnabled)
                    {
                        // make a file
                        var tmpDir = _parent.TmpFolder;
                        var fullPath = $"overlay_{OverlayId:D4}";
                        var tmpFilename = Regex.Replace(fullPath, "[^A-Za-z0-9_]+", "");

                        ExtFilename = tmpFilename;

                        var tmpFile = tmpDir + ExtFilename;
                        var fos = new FileStream(tmpFile, FileMode.Create, FileAccess.Write);

                        fos.Write(buf, 0, buf.Length);
                        fos.Close();

                        Status = Extracted.ToFile;
                        Data = null;

                        return buf;
                    }

                    Status = Extracted.ToRam;
                    Data = buf;

                    return Data;
                }
                case Extracted.ToRam:
                {
                    return Data;
                }
                case Extracted.ToFile:
                {
                    var tmpDir = _parent.TmpFolder;
                    var file = FileFunctions.ReadFileFullyIntoBuffer(tmpDir + ExtFilename);
                    return file;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        public virtual void WriteOverride(ArraySlice<byte> data)
        {
            if (Status == Extracted.Not)
                GetContents();

            Size = data.Length;

            if (Status == Extracted.ToFile)
            {
                var tmpDir = _parent.TmpFolder;
                var fos = new FileStream(tmpDir + ExtFilename, FileMode.Create, FileAccess.Write);
                fos.Write(data, 0, data.Length);
                fos.Close();
            }
            else
            {
                // Compression?
                if (CompressFlag != 0 && OriginalSize == CompressedSize && CompressedSize != 0)
                    Data = BlzCoder.Decode(Data, "overlay " + OverlayId);

                _decompressedData = true;

                if (Data.Length == data.Length)
                {
                    for (var i = 0; i < data.Length; i++)
                        Data[i] = data[i];
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
}