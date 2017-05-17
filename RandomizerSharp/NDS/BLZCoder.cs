﻿using System;
using System.IO;

namespace RandomizerSharp.NDS
{
    public static class BlzCoder
    {
        private const int BlzShift = 1;
        private const int BlzMask = 0x80;
        private const int BlzThreshold = 2;
        private const int BlzN = 0x1002;
        private const int BlzF = 0x12;
        private const int RawMaxim = 0x00FFFFFF;
        private const int BlzMaxim = 0x01400000;


        public static void Exit(string text)
        {
            Console.Write(text);
            Environment.Exit(0);
        }

        public static byte[] BLZ_DecodePub(byte[] data, string reference)
        {
            var result = BLZ_Decode(data);
            if (result != null)
            {
                var retbuf = new byte[result.Length];
                for (var i = 0; i < result.Length; i++)
                    retbuf[i] = (byte) result.Buffer[i];
                return retbuf;
            }
            return null;
        }

        private static BlzResult BLZ_Decode(byte[] data)
        {
            int rawLen, len;
            int decLen;
            int flags = 0;
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
                    Exit("\nFile has a bad header\n");
                    return null;
                }
                var hdrLen = pakBuffer[pakLen - 5];
                if (hdrLen < 8 || hdrLen > 0xB)
                {
                    Exit("\nBad header length\n");
                    return null;
                }
                if (pakLen <= hdrLen)
                {
                    Exit("\nBad length\n");
                    return null;
                }
                var encLen = ReadUnsigned(pakBuffer, pakLen - 8) & 0x00FFFFFF;
                decLen = pakLen - encLen;
                pakLen = encLen - hdrLen;
                rawLen = decLen + encLen + incLen;
                if (rawLen > RawMaxim)
                {
                    Exit("\nBad decoded length\n");
                    return null;
                }
            }
            var rawBuffer = new int[rawLen];
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
            rawLen = raw;
            if (raw != rawEnd)
                Console.Write(@", WARNING: unexpected end of encoded file!");
            return new BlzResult(rawBuffer, rawLen);
        }

        private static int[] PrepareData(byte[] data)
        {
            var fs = data.Length;
            var fb = new int[fs + 3];
            for (var i = 0; i < fs; i++)
                fb[i] = data[i] & 0xFF;
            return fb;
        }

        private static int ReadUnsigned(int[] buffer, int offset)
        {
            return buffer[offset] | (buffer[offset + 1] << 8) | (buffer[offset + 2] << 16) |
                   ((buffer[offset + 3] & 0x7F) << 24);
        }

        private static void WriteUnsigned(int[] buffer, int offset, int value)
        {
            buffer[offset] = value & 0xFF;
            buffer[offset + 1] = (value >> 8) & 0xFF;
            buffer[offset + 2] = (value >> 16) & 0xFF;
            buffer[offset + 3] = (value >> 24) & 0x7F;
        }

        public static byte[] BLZ_EncodePub(byte[] data, bool arm9, string reference)
        {

            Console.Write(@"- encoding '{0}' (memory)", reference);
            var startTime = DateTimeHelperClass.CurrentUnixTimeMillis();
            var result = BLZ_Encode(data, arm9);
            Console.Write(@" - done, time=" + (DateTimeHelperClass.CurrentUnixTimeMillis() - startTime) + @"ms");
            Console.WriteLine();
            if (result != null)
            {
                var retbuf = new byte[result.Length];
                for (var i = 0; i < result.Length; i++)
                    retbuf[i] = (byte) result.Buffer[i];
                return retbuf;
            }
            return null;
        }

        private static BlzResult BLZ_Encode(byte[] data, bool arm9)
        {
            var rawBuffer = PrepareData(data);
            var rawLen = rawBuffer.Length - 3;
            var pakLen = BlzMaxim + 1;
            var (newBuffer, newlen) = BLZ_Code(rawBuffer, rawLen, arm9);

            if (newlen >= pakLen)
                return new BlzResult(null, pakLen);

            var pakBuffer = newBuffer;
            pakLen = newlen;
            return new BlzResult(pakBuffer, pakLen);
        }

        private static (int[], int) BLZ_Code(int[] rawBuffer, int rawLen, bool arm9)
        {
            var flag = 0;
            var bestPosition = 0;
            var pakTempPos = 0;
            var rawTmpPos = rawLen;
            var pakLength = rawLen + (rawLen + 7) / 8 + 11;
            var rawNew = rawLen;

            var pakBuffer = new int[pakLength];

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

                var tempBestPos = bestPosition;
                var tempBestLength = BlzThreshold;

                var max = raw >= BlzN ? BlzN : raw;
                for (var pos = 3; pos <= max; pos++)
                {
                    var length = 0;
                    for (; length < BlzF; length++)
                    {
                        if (raw + length == rawEnd || length >= pos || rawBuffer[raw + length] != rawBuffer[raw + length - pos])
                            break;
                    }

                    if (length <= tempBestLength)
                        continue;

                    tempBestPos = pos;
                    tempBestLength = length;

                    if (tempBestLength == BlzF)
                        break;
                }

                var lenBest1 = tempBestLength;
                bestPosition = tempBestPos;

                pakBuffer[flag] = pakBuffer[flag] << 1;

                if (lenBest1 > BlzThreshold)
                {
                    raw += lenBest1;
                    pakBuffer[flag] |= 1;
                    pakBuffer[pak++] = ((lenBest1 - (BlzThreshold + 1)) << 4) | ((bestPosition - 3) >> 8);
                    pakBuffer[pak++] = (bestPosition - 3) & 0xFF;
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
                pakBuffer[flag] = pakBuffer[flag] << 1;
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
                var tmp = new int[rawTmpPos + pakTempPos + 11];
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
                pakBuffer[pak++] = hdrLen;
                WriteUnsigned(pakBuffer, pak, incLen - hdrLen);
                pak += 4;
            }

            return (pakBuffer, pak);
        }

        private static void BLZ_Invert(int[] buffer, int offset, int length)
        {
            var bottom = offset + length - 1;
            while (offset < bottom)
            {
                var ch = buffer[offset];
                buffer[offset++] = buffer[bottom];
                buffer[bottom--] = ch;
            }
        }

        private class BlzResult
        {
            public readonly int[] Buffer;
            public readonly int Length;

            public BlzResult(int[] rawBuffer, int rawLen)
            {
                Buffer = rawBuffer;
                Length = rawLen;
            }
        }
    }

//---------------------------------------------------------------------------------------------------------
//	Copyright © 2007 - 2017 Tangible Software Solutions Inc.
//	This class can be used by anyone provided that the copyright notice remains intact.
//
//	This class is used to replace calls to Java's System.currentTimeMillis with the C# equivalent.
//	Unix time is defined as the number of seconds that have elapsed since midnight UTC, 1 January 1970.
//---------------------------------------------------------------------------------------------------------
    internal static class DateTimeHelperClass
    {
        private static readonly DateTime Jan1St1970 =
            new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        internal static long CurrentUnixTimeMillis()
        {
            return (long) (DateTime.UtcNow - Jan1St1970).TotalMilliseconds;
        }
    }
}