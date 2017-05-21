using System;

namespace RandomizerSharp.NDS
{
    public class NdsFile
    {
        public byte[] Data { get; private set; } = Array.Empty<byte>();
        public string ExtFilename { get; set; }
        public int FileId { get; set; }
        public string FullPath { get; set; }
        public int Offset { get; set; }
        public int Size { get; set; }

        private readonly NdsRom _parent;
   

        public NdsFile(NdsRom parent)
        {
            _parent = parent;
        }

        public byte[] LoadContents()
        {
            if (Data.Length != 0)
                return Data;

            var rom = _parent.BaseRom;
            var buf = new byte[Size];

            rom.Seek(Offset);
            rom.ReadFully(buf);

            Data = buf;

            return Data;
        }
        
        public void WriteOverride(byte[] data)
        {
            LoadContents();
            
            if (Data.Length == data.Length)
            {
                for (var i = 0; i < data.Length; i++)
                    Data[i] = data[i];
            }
            else
            {
                var newData = new byte[data.Length];
                Array.Copy(data, 0, newData, 0, data.Length);

                Data = newData;
            }
        }
    }
}