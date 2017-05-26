using System;
using System.Collections.Generic;
using System.IO;

namespace RandomizerSharp
{
    public class FileFunctions
    {
        public static string FixFilename(string original, string defaultExtension) => FixFilename(
            original,
            defaultExtension,
            null);

        public static string FixFilename(string original, string defaultExtension, IList<string> bannedExtensions)
        {
            var filename = Path.GetFileName(original);
            if (filename != null &&
                filename.LastIndexOf('.') >= filename.Length - 5 &&
                filename.LastIndexOf('.') != filename.Length - 1 &&
                filename.Length > 4 &&
                filename.LastIndexOf('.') != -1)
            {
                var ext = filename.Substring(filename.LastIndexOf('.') + 1).ToLower();
                if (bannedExtensions != null && bannedExtensions.Contains(ext))
                    filename = filename.Substring(0, filename.LastIndexOf('.') + 1) + defaultExtension;
            }
            else
            {
                filename += "." + defaultExtension;
            }
            return Path.GetFullPath(original).Replace(filename, "") + filename;
        }

        public static byte[] ReadFullyIntoBuffer(Stream @in, int bytes)
        {
            var buf = new byte[bytes];
            @in.Read(buf, 0, bytes);
            return buf;
        }

        public static void ApplyPatch(ArraySlice<byte> rom, byte[] patch)
        {
            var patchlen = patch.Length;
            if (patchlen < 8 ||
                patch[0] != 'P' ||
                patch[1] != 'A' ||
                patch[2] != 'T' ||
                patch[3] != 'C' ||
                patch[4] != 'H')
                throw new IOException("not a valid IPS file");
            var offset = 5;
            while (offset + 2 < patchlen)
            {
                var writeOffset = ReadIpsOffset(patch, offset);
                if (writeOffset == 0x454f46)
                    return;
                offset += 3;
                if (offset + 1 >= patchlen)
                    throw new IOException("abrupt ending to IPS file, entry cut off before size");
                var size = ReadIpsSize(patch, offset);
                offset += 2;
                if (size == 0)
                {
                    if (offset + 1 >= patchlen)
                        throw new IOException("abrupt ending to IPS file, entry cut off before RLE size");
                    var rleSize = ReadIpsSize(patch, offset);
                    if (writeOffset + rleSize > rom.Length)
                        throw new IOException("trying to patch data past the end of the ROM file");
                    offset += 2;
                    if (offset >= patchlen)
                        throw new IOException("abrupt ending to IPS file, entry cut off before RLE byte");
                    var rleByte = patch[offset++];
                    for (var i = writeOffset; i < writeOffset + rleSize; i++)
                        rom[i] = rleByte;
                }
                else
                {
                    if (offset + size > patchlen)
                        throw new IOException("abrupt ending to IPS file, entry cut off before end of data block");
                    if (writeOffset + size > rom.Length)
                        throw new IOException("trying to patch data past the end of the ROM file");
                    ArraySlice.Copy(patch, offset, rom, writeOffset, size);
                    offset += size;
                }
            }
            throw new IOException("improperly terminated IPS file");
        }

        private static int ReadIpsOffset(byte[] data, int offset) => ((data[offset] & 0xFF) << 16) |
                                                                     ((data[offset + 1] & 0xFF) << 8) |
                                                                     (data[offset + 2] & 0xFF);

        private static int ReadIpsSize(byte[] data, int offset) => ((data[offset] & 0xFF) << 8) |
                                                                   (data[offset + 1] & 0xFF);

        public static byte[] ConvIntArrToByteArr(int[] arg)
        {
            var @out = new byte[arg.Length];
            for (var i = 0; i < arg.Length; i++)
                @out[i] = (byte) arg[i];
            return @out;
        }
    }
}