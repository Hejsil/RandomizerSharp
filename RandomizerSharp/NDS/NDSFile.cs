using System;

namespace RandomizerSharp.NDS
{
    public class NdsFile
    {
        public byte[] Data { get; set; } = Array.Empty<byte>();
        public string ExtFilename { get; set; }
        public int FileId { get; set; }
        public string FullPath { get; set; }
        public int Offset { get; set; }
        public int Size { get; set; }

        public byte[] LoadContents() => Data;

        public void WriteOverride(byte[] data)
        {
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