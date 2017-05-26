using System.IO;
using RandomizerSharp;
using RandomizerSharp.NDS;

public class NDSY9Entry
{
    private bool _decompressedData;
    private readonly Stream _baseRom;

    public NDSY9Entry(Stream baseRom) => _baseRom = baseRom;

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
    
    public byte[] GetContents()
    {
        if (Data == null)
            LoadIn();

        // Compression?
        if (CompressFlag == 0 || OriginalSize != CompressedSize || CompressedSize == 0)
            return Data;

        Data = BlzCoder.Decode(Data, "overlay " + OverlayId);
        _decompressedData = true;

        return Data;
    }
    
    public byte[] GetOverrideContents()
    {
        if (!_decompressedData)
            return Data;

        Data = BlzCoder.Encode(Data, false, "overlay " + OverlayId);
        // update our compressed size
        CompressedSize = Data.Length;

        return Data;
    }

    public void LoadIn()
    {
        // extract file
        var buf = new byte[OriginalSize];
        _baseRom.Seek(Offset);
        _baseRom.ReadFully(buf);
        Data = buf;
    }
}
