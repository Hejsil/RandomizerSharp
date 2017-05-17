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
        public byte[] Data;
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
        public virtual byte[] OverrideContents
        {
            get
            {
                if (Status == Extracted.Not)
                    return null;
                var buf = GetContents();
                if (_decompressedData)
                {
                    buf = BlzCoder.BLZ_EncodePub(buf, false, "overlay " + OverlayId);
                    // update our compressed size
                    CompressedSize = buf.Length;
                }
                return buf;
            }
        }
        
        public virtual byte[] GetContents()
        {
            if (Status == Extracted.Not)
            {
                // extract file
                _parent.ReopenRom();
                var rom = _parent.BaseRom;
                var buf = new byte[OriginalSize];
                rom.Seek(Offset);
                rom.ReadFully(buf);
                // Compression?
                if (CompressFlag != 0 && OriginalSize == CompressedSize && CompressedSize != 0)
                {
                    buf = BlzCoder.BLZ_DecodePub(buf, "overlay " + OverlayId);
                    _decompressedData = true;
                }
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
                var newcopy = new byte[buf.Length];
                Array.Copy(buf, 0, newcopy, 0, buf.Length);
                return newcopy;
            }
            if (Status == Extracted.ToRam)
            {
                var newcopy = new byte[Data.Length];
                Array.Copy(Data, 0, newcopy, 0, Data.Length);
                return newcopy;
            }
            {
                var tmpDir = _parent.TmpFolder;
                var file = FileFunctions.ReadFileFullyIntoBuffer(tmpDir + ExtFilename);
                return file;
            }
        }
        
        public virtual void WriteOverride(byte[] data)
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
                if (Data.Length == data.Length)
                {
                    // copy new in
                    Array.Copy(data, 0, Data, 0, data.Length);
                }
                else
                {
                    // make new array
                    Data = null;
                    Data = new byte[data.Length];
                    Array.Copy(data, 0, Data, 0, data.Length);
                }
            }
        }
    }
}