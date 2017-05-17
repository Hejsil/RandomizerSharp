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

        public ArraySlice<byte> Data { get; private set; }
        public string ExtFilename { get; set; }
        public int FileId { get; set; }
        public string FullPath { get; set; }
        public int Offset { get; set; }
        public int Size { get; set; }

        private readonly NdsRom _parent;
        private Extracted _status = Extracted.Not;

        public NdsFile(NdsRom parent)
        {
            _parent = parent;
        }

        public virtual ArraySlice<byte> LoadContents()
        {
            switch (_status)
            {
                case Extracted.Not:
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
                        _status = Extracted.ToFile;
                        Data = null;
                        return buf;
                    }
                    _status = Extracted.ToRam;

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
            if (_status == Extracted.Not)
                LoadContents();

            if (_status == Extracted.ToFile)
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