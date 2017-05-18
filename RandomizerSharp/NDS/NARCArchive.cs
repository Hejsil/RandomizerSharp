using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RandomizerSharp.NDS
{
    public class NarcArchive
    {
        private byte[] _bytes;

        public List<string> Filenames { get; } = new List<string>();
        public List<ArraySlice<byte>> Files { get; } = new List<ArraySlice<byte>>();
        public bool HasFilenames { get; }

        public NarcArchive()
        {
        }

        public NarcArchive(byte[] data)
        {
            _bytes = data;
            var frames = ReadNitroFrames(data);

            if (!frames.ContainsKey("FATB") || !frames.ContainsKey("FNTB") || !frames.ContainsKey("FIMG"))
                throw new IOException("Not a valid narc file");

            var fatbframe = frames["FATB"];
            var fimgframe = frames["FIMG"];
            var fileCount = ReadLong(fatbframe, 0);

            for (var i = 0; i < fileCount; i++)
            {
                var startOffset = ReadLong(fatbframe, 4 + i * 8);
                var endOffset = ReadLong(fatbframe, 8 + i * 8);
                var length = endOffset - startOffset;

                Files.Add(fimgframe.Slice(length, startOffset));
            }

            var fntbframe = frames["FNTB"];
            var unk1 = ReadLong(fntbframe, 0);

            if (unk1 == 8)
            {
                var offset = 8;
                HasFilenames = true;

                for (var i = 0; i < fileCount; i++)
                {
                    var fnLength = fntbframe[offset] & 0xFF;
                    offset++;

                    var array = new byte[fntbframe.Length];
                    fntbframe.CopyTo(array, 0);
                    Filenames.Add(Encoding.ASCII.GetString(array, offset, fnLength));
                }
            }
            else
            {
                HasFilenames = false;

                for (var i = 0; i < fileCount; i++) Filenames.Add(null);
            }
        }


        public virtual byte[] Bytes
        {
            get
            {
                if (_bytes != null) return _bytes;

                var offset = 0;

                var bytesRequired = Files.Sum(file => (int) Math.Ceiling(file.Length / 4.0) * 4);
                var fatbFrame = new byte[4 + Files.Count * 8 + 8];
                var fimgFrame = new byte[bytesRequired + 8];

                fatbFrame[0] = (byte) 'B';
                fatbFrame[1] = (byte) 'T';
                fatbFrame[2] = (byte) 'A';
                fatbFrame[3] = (byte) 'F';

                WriteLong(fatbFrame, 4, fatbFrame.Length);

                fimgFrame[0] = (byte) 'G';
                fimgFrame[1] = (byte) 'M';
                fimgFrame[2] = (byte) 'I';
                fimgFrame[3] = (byte) 'F';

                WriteLong(fimgFrame, 4, fimgFrame.Length);
                WriteLong(fatbFrame, 8, Files.Count);

                for (var i = 0; i < Files.Count; i++)
                {
                    var file = Files[i];
                    var bytesRequiredForFile = (int) (Math.Ceiling(file.Length / 4.0) * 4);
                    file.CopyTo(fimgFrame, offset + 8);

                    for (var filler = file.Length; filler < bytesRequiredForFile; filler++)
                        fimgFrame[offset + 8 + filler] = 0xFF;

                    WriteLong(fatbFrame, 12 + i * 8, offset);
                    WriteLong(fatbFrame, 16 + i * 8, offset + file.Length);
                    offset += bytesRequiredForFile;
                }

                var bytesForFntbFrame = 16;

                if (HasFilenames)
                    bytesForFntbFrame += Filenames.Sum(
                        filename => Encoding.ASCII.GetBytes(filename).Length + 1);

                var fntbFrame = new byte[bytesForFntbFrame];

                fntbFrame[0] = (byte) 'B';
                fntbFrame[1] = (byte) 'T';
                fntbFrame[2] = (byte) 'N';
                fntbFrame[3] = (byte) 'F';

                WriteLong(fntbFrame, 4, fntbFrame.Length);

                if (HasFilenames)
                {
                    WriteLong(fntbFrame, 8, 8);
                    WriteLong(fntbFrame, 12, 0x10000);
                    var fntbOffset = 16;
                    foreach (var filename in Filenames)
                    {
                        var fntbfilename = Encoding.ASCII.GetBytes(filename);
                        fntbFrame[fntbOffset] = (byte) fntbfilename.Length;
                        Array.Copy(fntbfilename, 0, fntbFrame, fntbOffset + 1, fntbfilename.Length);
                        fntbOffset += 1 + fntbfilename.Length;
                    }
                }
                else
                {
                    WriteLong(fntbFrame, 8, 4);
                    WriteLong(fntbFrame, 12, 0x10000);
                }
                var nitrolength = 16 + fatbFrame.Length + fntbFrame.Length + fimgFrame.Length;

                _bytes = new byte[nitrolength];
                _bytes[0] = (byte) 'N';
                _bytes[1] = (byte) 'A';
                _bytes[2] = (byte) 'R';
                _bytes[3] = (byte) 'C';
                WriteWord(_bytes, 4, 0xFFFE);
                WriteWord(_bytes, 6, 0x0100);
                WriteLong(_bytes, 8, nitrolength);
                WriteWord(_bytes, 12, 0x10);
                WriteWord(_bytes, 14, 3);
                Array.Copy(fatbFrame, 0, _bytes, 16, fatbFrame.Length);
                Array.Copy(fntbFrame, 0, _bytes, 16 + fatbFrame.Length, fntbFrame.Length);
                Array.Copy(fimgFrame, 0, _bytes, 16 + fatbFrame.Length + fntbFrame.Length, fimgFrame.Length);

                return _bytes;
            }
        }

        private static Dictionary<string, ArraySlice<byte>> ReadNitroFrames(ArraySlice<byte> data)
        {
            var frameCount = ReadWord(data, 0x0E);
            var offset = 0x10;
            var frames = new Dictionary<string, ArraySlice<byte>>();

            for (var i = 0; i < frameCount; i++)
            {
                var magic = new[] { data[offset + 3], data[offset + 2], data[offset + 1], data[offset] };
                var magicS = Encoding.ASCII.GetString(magic, 0, magic.Length);
                var frameSize = ReadLong(data, offset + 4);

                if (i == frameCount - 1 && offset + frameSize < data.Length)
                    frameSize = data.Length - offset;
                
                frames[magicS] = data.Slice(frameSize - 8, offset + 8);
                offset += frameSize;
            }
            return frames;
        }

        private static int ReadWord(IList<byte> data, int offset)
        {
            return (data[offset] & 0xFF) | ((data[offset + 1] & 0xFF) << 8);
        }

        private static int ReadLong(IList<byte> data, int offset)
        {
            return (data[offset] & 0xFF) |
                   ((data[offset + 1] & 0xFF) << 8) |
                   ((data[offset + 2] & 0xFF) << 16) |
                   ((data[offset + 3] & 0xFF) << 24);
        }

        private static void WriteWord(IList<byte> data, int offset, int value)
        {
            data[offset] = unchecked((byte) (value & 0xFF));
            data[offset + 1] = unchecked((byte) ((value >> 8) & 0xFF));
        }

        private static void WriteLong(IList<byte> data, int offset, int value)
        {
            data[offset] = unchecked((byte) (value & 0xFF));
            data[offset + 1] = unchecked((byte) ((value >> 8) & 0xFF));
            data[offset + 2] = unchecked((byte) ((value >> 16) & 0xFF));
            data[offset + 3] = unchecked((byte) ((value >> 24) & 0xFF));
        }
    }
}