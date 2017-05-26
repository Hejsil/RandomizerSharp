using System;
using System.Collections.Generic;

namespace RandomizerSharp
{
    public class DsDecmp
    {
        public static byte[] Decompress(ArraySlice<byte> data, int offset = 0)
        {
            switch (data[offset] & 0xFF)
            {
                case 0x10:
                    return Decompress10Lz(data, offset);
                case 0x11:
                    return Decompress11Lz(data, offset);
                default:
                    return null;
            }
        }

        private static byte[] PerpareData(IList<byte> data, ref int offset)
        {
            offset++;
            var length = (data[offset] & 0xFF) | ((data[offset + 1] & 0xFF) << 8) | ((data[offset + 2] & 0xFF) << 16);
            offset += 3;

            if (length != 0)
                return new byte[length];

            length = PpTxtHandler.ReadInt(data, offset);
            offset += 4;

            return new byte[length];
        }

        private static byte[] Decompress10Lz(IList<byte> data, int offset)
        {
            var outData = PerpareData(data, ref offset);
            var currSize = 0;
            while (currSize < outData.Length)
            {
                var flags = data[offset++] & 0xFF;
                for (var i = 0; i < 8; i++)
                {
                    var flag = (flags & (0x80 >> i)) > 0;
                    int b;
                    if (flag)
                    {
                        b = data[offset++] & 0xFF;
                        var n = b >> 4;
                        var disp = (b & 0x0F) << 8;
                        disp |= data[offset++] & 0xFF;
                        n += 3;
                        var cdest = currSize;
                        if (disp > currSize)
                            throw new IndexOutOfRangeException("Cannot go back more than already written");
                        for (var j = 0; j < n; j++)
                            outData[currSize++] = outData[cdest - disp - 1 + j];
                        if (currSize > outData.Length)
                            break;
                    }
                    else
                    {
                        b = data[offset++] & 0xFF;
                        try
                        {
                            outData[currSize++] = (byte) b;
                        }
                        catch (IndexOutOfRangeException)
                        {
                            if (b == 0)
                                break;
                        }
                        if (currSize > outData.Length)
                            break;
                    }
                }
            }
            return outData;
        }

        private static byte[] Decompress11Lz(IList<byte> data, int offset)
        {
            var outData = PerpareData(data, ref offset);
            var currSize = 0;
            while (currSize < outData.Length)
            {
                var flags = data[offset++] & 0xFF;
                for (var i = 0; i < 8 && currSize < outData.Length; i++)
                {
                    var flag = (flags & (0x80 >> i)) > 0;
                    if (flag)
                    {
                        var b1 = data[offset++] & 0xFF;
                        int bt;
                        int b2;
                        int len;
                        int disp;
                        switch (b1 >> 4)
                        {
                            case 0:
                                len = b1 << 4;
                                bt = data[offset++] & 0xFF;
                                len |= bt >> 4;
                                len += 0x11;
                                disp = (bt & 0x0F) << 8;
                                b2 = data[offset++] & 0xFF;
                                disp |= b2;
                                break;
                            case 1:
                                bt = data[offset++] & 0xFF;
                                b2 = data[offset++] & 0xFF;
                                var b3 = data[offset++] & 0xFF;
                                len = (b1 & 0xF) << 12;
                                len |= bt << 4;
                                len |= b2 >> 4;
                                len += 0x111;
                                disp = (b2 & 0x0F) << 8;
                                disp |= b3;
                                break;
                            default:
                                len = (b1 >> 4) + 1;
                                disp = (b1 & 0x0F) << 8;
                                b2 = data[offset++] & 0xFF;
                                disp |= b2;
                                break;
                        }
                        if (disp > currSize)
                            throw new IndexOutOfRangeException("Cannot go back more than already written");
                        var cdest = currSize;
                        for (var j = 0; j < len && currSize < outData.Length; j++)
                            outData[currSize++] = outData[cdest - disp - 1 + j];
                        if (currSize > outData.Length)
                            break;
                    }
                    else
                    {
                        outData[currSize++] = data[offset++];
                        if (currSize > outData.Length)
                            break;
                    }
                }
            }
            return outData;
        }
    }
}