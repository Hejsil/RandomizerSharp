using System;
using System.IO;
using System.Text.RegularExpressions;

namespace RandomizerSharp.NDS
{
    public class NdsFile
    {
        public enum Extracted
        {
            Not,
            ToFile,
            ToRam
        }

        public byte[] Data;
        public string ExtFilename;
        public int FileId;
        public string FullPath;
        public int Offset, Size;
        private readonly NdsRom _parent;
        public Extracted Status = Extracted.Not;

        public NdsFile(NdsRom parent)
        {
            _parent = parent;
        }
        
        public virtual byte[] OverrideContents
        {
            get
            {
                if (Status == Extracted.Not)
                    return null;
                return GetContents();
            }
        }
        
        public virtual byte[] GetContents()
        {
            if (Status == Extracted.Not)
            {
                // extract file
                _parent.ReopenRom();
                var rom = _parent.BaseRom;
                var buf = new byte[Size];
                rom.Seek(Offset);
                rom.ReadFully(buf);
                if (_parent.WritingEnabled)
                {
                    // make a file
                    var tmpDir = _parent.TmpFolder;
                    var tmpFilename = Regex.Replace(FullPath, "[^A-Za-z0-9_]+", "");
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