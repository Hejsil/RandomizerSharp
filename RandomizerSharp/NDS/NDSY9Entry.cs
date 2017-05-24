using System;
using System.IO;
using RandomizerSharp;
using RandomizerSharp.NDS;

public class NDSY9Entry
{
    private bool _decompressedData;
    private readonly NdsRom _parent;
    public int Offset { get; set; }
    public int Size { get; set; }
    public int OriginalSize { get; set; }
    public int FileId { get; set; }
    public int OverlayId { get; set; }
    public int RamAddress { get; set; }
    public int RamSize { get; set; }
    public int BssSize { get; set; }
    public int StaticStart { get; set; }
    public int StaticEnd { get; set; }
    public int CompressedSize { get; set; }
    public int CompressFlag { get; set; }
    public string ExtFilename { get; }
    public byte[] Data { get; set; }

    public NDSY9Entry(NdsRom parent)
    {
        _parent = parent;
    }
    
    public byte[] GetContents()
    {
        if (Data == null)
            Data = Read();

        // Compression?
        if (CompressFlag == 0 || OriginalSize != CompressedSize || CompressedSize == 0)
            return Data;

        Data = BlzCoder.Decode(Data, "overlay " + OverlayId);
        _decompressedData = true;

        return Data;
    }
    
    public void WriteOverride(byte[] data)
    {
        if (Data == null || CompressFlag == 0 || OriginalSize != CompressedSize || CompressedSize == 0)
            GetContents();

        Size = data.Length;

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
    
    public byte[] GetOverrideContents()
    {
        if (Data == null)
            Data = Read();

        if (!_decompressedData)
            return Data;

        Data = BlzCoder.Encode(Data, false, "overlay " + OverlayId);
        // update our compressed size
        CompressedSize = Data.Length;

        return Data;
    }

    private byte[] Read()
    {
        // extract file
        Stream rom = _parent.BaseRom;
        byte[] buf = new byte[OriginalSize];
        rom.Seek(Offset);
        rom.ReadFully(buf);

        return buf;
    }

}
