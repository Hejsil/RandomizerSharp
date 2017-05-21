using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Timers;

namespace RandomizerSharp.NDS
{
    public static class BlzCoder
    {
        private const int BlzShift = 1;
        private const int BlzMask = 0x80;
        private const int BlzThreshold = 2;
        private const int BlzF = 0x12;
        private const int RawMaxim = 0x00FFFFFF;
        private const int BlzMaxim = 0x01400000;

        public static byte[] Decode(IList<byte> data, string reference)
        {
            int rawLen, len;
            int decLen;
            var flags = 0;
            var pakBuffer = PrepareData(data);
            var pakLen = pakBuffer.Length - 3;
            var incLen = ReadUnsigned(pakBuffer, pakLen - 4);
            if (incLen < 1)
            {
                Console.Write(@", WARNING: not coded file!");
                decLen = pakLen;
                pakLen = 0;
                rawLen = decLen;
            }
            else
            {
                if (pakLen < 8)
                {
                    throw new NotImplementedException("\nFile has a bad header\n");
                }
                var hdrLen = pakBuffer[pakLen - 5];
                if (hdrLen < 8 || hdrLen > 0xB)
                {
                    throw new NotImplementedException("\nBad header length\n");
                }
                if (pakLen <= hdrLen)
                {
                    throw new NotImplementedException("\nBad length\n");
                }
                var encLen = ReadUnsigned(pakBuffer, pakLen - 8) & 0x00FFFFFF;
                decLen = pakLen - encLen;
                pakLen = encLen - hdrLen;
                rawLen = decLen + encLen + incLen;
                if (rawLen > RawMaxim)
                {
                    throw new NotImplementedException("\nBad decoded length\n");
                }
            }
            var rawBuffer = new byte[rawLen];
            var pak = 0;
            var raw = 0;
            var pakEnd = decLen + pakLen;
            var rawEnd = rawLen;
            for (len = 0; len < decLen; len++)
                rawBuffer[raw++] = pakBuffer[pak++];
            BLZ_Invert(pakBuffer, decLen, pakLen);
            var mask = 0;
            while (raw < rawEnd)
            {
                if ((mask = (int) ((uint) mask >> BlzShift)) == 0)
                {
                    if (pak == pakEnd)
                        break;
                    flags = pakBuffer[pak++];
                    mask = BlzMask;
                }
                if ((flags & mask) == 0)
                {
                    if (pak == pakEnd)
                        break;
                    rawBuffer[raw++] = pakBuffer[pak++];
                }
                else
                {
                    if (pak + 1 >= pakEnd)
                        break;
                    var pos = pakBuffer[pak++] << 8;
                    pos |= pakBuffer[pak++];
                    len = (int) ((uint) pos >> 12) + BlzThreshold + 1;
                    if (raw + len > rawEnd)
                    {
                        Console.Write(@", WARNING: wrong decoded length!");
                        len = rawEnd - raw;
                    }
                    pos = (pos & 0xFFF) + 3;
                    while (len-- > 0)
                    {
                        var charHere = rawBuffer[raw - pos];
                        rawBuffer[raw++] = charHere;
                    }
                }
            }
            BLZ_Invert(rawBuffer, decLen, rawLen - decLen);

            if (raw != rawEnd)
                Console.Write(@", WARNING: unexpected end of encoded file!");

            return rawBuffer;
        }

        public static byte[] Encode(IList<byte> data, bool arm9, string reference)
        {
            Console.Write(@"- encoding '{0}' (memory)", reference);

            var timer = new Stopwatch();
            timer.Start();
            var (result, resultLength) = BLZ_Encode(data, arm9);
            timer.Stop();

            Console.WriteLine(@" - done, time={0}ms", TimeSpan.FromTicks(timer.ElapsedTicks).TotalMilliseconds);

            // We have to make a copy, as the encoder pads some data at the end, which is not needed later
            var retbuf = new byte[resultLength];
            Array.Copy(result, 0, retbuf, 0, resultLength);

            return retbuf;
        }

        private static byte[] PrepareData(ICollection<byte> data)
        {
            var fs = data.Count;
            var fb = new byte[fs + 3];
            data.CopyTo(fb, 0);

            for (var i = 0; i < fs; i++)
                fb[i] = (byte) (fb[i] & 0xFF);

            return fb;
        }

        private static int ReadUnsigned(byte[] buffer, int offset)
        {
            return buffer[offset] | (buffer[offset + 1] << 8) | (buffer[offset + 2] << 16) |
                   ((buffer[offset + 3] & 0x7F) << 24);
        }

        private static void WriteUnsigned(byte[] buffer, int offset, int value)
        {
            buffer[offset] = (byte) (value & 0xFF);
            buffer[offset + 1] = (byte)((value >> 8) & 0xFF);
            buffer[offset + 2] = (byte)((value >> 16) & 0xFF);
            buffer[offset + 3] = (byte)((value >> 24) & 0x7F);
        }

        private static (byte[], int) BLZ_Encode(IList<byte> data, bool arm9)
        {
            var rawBuffer = PrepareData(data);
            var rawLen = rawBuffer.Length - 3;
            var pakLen = BlzMaxim + 1;
            var (newBuffer, newlen) = BLZ_Code(rawBuffer, rawLen, arm9);

            if (newlen >= pakLen)
                return (null, pakLen);

            var pakBuffer = newBuffer;
            pakLen = newlen;
            return (pakBuffer, pakLen);
        }

        private static (byte[], int) BLZ_Code(byte[] rawBuffer, int rawLen, bool arm9)
        {
            var flag = 0;
            var bestPosition = 0;
            var pakTempPos = 0;
            var rawTmpPos = rawLen;
            var pakLength = rawLen + (rawLen + 7) / 8 + 11;
            var rawNew = rawLen;

            var pakBuffer = new byte[pakLength];

            if (arm9)
                rawNew -= 0x4000;

            BLZ_Invert(rawBuffer, 0, rawLen);
            var pak = 0;
            var raw = 0;
            var rawEnd = rawNew;
            var mask = 0;

            while (raw < rawEnd)
            {
                if ((mask = mask >> BlzShift) == 0)
                {
                    pakBuffer[flag = pak++] = 0;
                    mask = BlzMask;
                }

                int lenBest1;
                (bestPosition, lenBest1) = Search(rawBuffer, bestPosition, raw, rawEnd);

                pakBuffer[flag] = (byte) (pakBuffer[flag] << 1);

                if (lenBest1 > BlzThreshold)
                {
                    raw += lenBest1;
                    pakBuffer[flag] |= 1;
                    pakBuffer[pak++] = (byte) (((lenBest1 - (BlzThreshold + 1)) << 4) | ((bestPosition - 3) >> 8));
                    pakBuffer[pak++] = (byte) ((bestPosition - 3) & 0xFF);
                }
                else
                {
                    pakBuffer[pak++] = rawBuffer[raw++];
                }

                if (pak + rawLen - raw >= pakTempPos + rawTmpPos)
                    continue;

                pakTempPos = pak;
                rawTmpPos = rawLen - raw;
            }

            while (mask > 0 && mask != 1)
            {
                mask = mask >> BlzShift;
                pakBuffer[flag] = (byte) (pakBuffer[flag] << 1);
            }

            pakLength = pak;
            BLZ_Invert(rawBuffer, 0, rawLen);
            BLZ_Invert(pakBuffer, 0, pakLength);

            if (pakTempPos == 0 || rawLen + 4 < ((pakTempPos + rawTmpPos + 3) & 0xFFFFFFFC) + 8)
            {
                pak = 0;

                while ((pak & 3) > 0)
                    pakBuffer[pak++] = 0;

                pakBuffer[pak++] = 0;
                pakBuffer[pak++] = 0;
                pakBuffer[pak++] = 0;
                pakBuffer[pak++] = 0;
            }
            else
            {
                var tmp = new byte[rawTmpPos + pakTempPos + 11];
                int len;
                for (len = 0; len < rawTmpPos; len++)
                    tmp[len] = rawBuffer[len];
                for (len = 0; len < pakTempPos; len++)
                    tmp[rawTmpPos + len] = pakBuffer[len + pakLength - pakTempPos];
                pakBuffer = tmp;

                pak = rawTmpPos + pakTempPos;
                var encLen = pakTempPos;
                var hdrLen = 8;
                var incLen = rawLen - pakTempPos - rawTmpPos;
                while ((pak & 3) > 0)
                {
                    pakBuffer[pak++] = 0xFF;
                    hdrLen++;
                }
                WriteUnsigned(pakBuffer, pak, encLen + hdrLen);
                pak += 3;
                pakBuffer[pak++] = (byte) hdrLen;
                WriteUnsigned(pakBuffer, pak, incLen - hdrLen);
                pak += 4;
            }

            return (pakBuffer, pak);
        }
        
        public static (int, int) Search(byte[] rawBuffer, int bestPosition, int raw, int rawEnd)
        {
            var bestLength = BlzThreshold;

            var max = Math.Min(raw, BlzF);

            for (var pos = 3; pos <= max; pos++)
            {
                var length = 0;
                for (; length < BlzF; length++)
                {
                    if (raw + length == rawEnd)
                        break;
                    if (length >= pos)
                        break;
                    if (rawBuffer[raw + length] != rawBuffer[raw + length - pos])
                        break;
                }

                if (length <= bestLength)
                    continue;

                bestPosition = pos;
                bestLength = length;

                if (bestLength == BlzF)
                    break;
            }

            return (bestPosition, bestLength);
        }

        private static void BLZ_Invert(byte[] buffer, int offset, int length)
        {
            var bottom = offset + length - 1;
            while (offset < bottom)
            {
                var ch = buffer[offset];
                buffer[offset++] = buffer[bottom];
                buffer[bottom--] = ch;
            }
        }
    }
}