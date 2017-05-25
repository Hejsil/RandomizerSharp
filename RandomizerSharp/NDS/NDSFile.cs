using System;
using System.IO;

namespace RandomizerSharp.NDS
{
    public class NdsFile
    {

        private readonly Stream _baseRom;

        public NdsFile(Stream baseRom) => _baseRom = baseRom;

        public byte[] Data { get; set; }
        public string ExtFilename { get; set; }
        public int FileId { get; set; }
        public string FullPath { get; set; }
        public int Offset { get; set; }
        public int Size { get; set; }


        public byte[] LoadContents()
        {
            if (Data != null)
                return Data;

            LoadIn();
            return Data;
        }
        
        // returns null if no override
        public byte[] GetWithoutLoad() => Data;

        public void WriteOverride(byte[] data)
        {
            if (Data == null)
                LoadIn();

            if (Data.Length == data.Length)
            {
                for (var i = 0; i < data.Length; i++)
                    Data[i] = data[i];
            }
            else
            {
                var newData = new byte[data.Length];
                Array.Copy(data, 0, newData, 0, data.Length);
                Size = newData.Length;
                Data = newData;
            }
        }
        
        private void LoadIn()
        {
            var buf = new byte[Size];
            _baseRom.Seek(Offset);
            _baseRom.ReadFully(buf);
            Data = buf;
        }
    }
}