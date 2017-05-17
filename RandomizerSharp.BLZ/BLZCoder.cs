using System;
using System.IO;

namespace RandomizerSharp.NDS
{
    public static class BlzCoder
    {
        const int CMD_DECODE = 0x00; // decode
        const int CMD_ENCODE = 0x01; // encode

        const int BLZ_NORMAL = 0; // normal mode
        const int BLZ_BEST = 1; // best mode

        const int BLZ_SHIFT = 1; // bits to shift

        const int BLZ_MASK = 0x80; // bits to check:
        // ((((1 << BLZ_SHIFT) - 1) << (8 - BLZ_SHIFT)

        const int BLZ_THRESHOLD = 2; // max number of bytes to not encode
        const int BLZ_N = 0x1002; // max offset ((1 << 12) + 2)
        const int BLZ_F = 0x12; // max coded ((1 << 4) + BLZ_THRESHOLD)

        const int RAW_MINIM = 0x00000000; // empty file, 0 bytes
        const int RAW_MAXIM = 0x00FFFFFF; // 3-bytes length, 16MB - 1

        const int BLZ_MINIM = 0x00000004; // header only (empty RAW file)

        const int BLZ_MAXIM = 0x01400000; // 0x0120000A, padded to 20MB:
        // * length, RAW_MAXIM
        // * flags, (RAW_MAXIM + 7) / 8
        // * header, 11
        // 0x00FFFFFF + 0x00200000 + 12 + padding

        static byte[] BLZ_Decode(byte[] pak_buffer)
        {
                int raw_len;
                int enc_len, dec_len;
                byte flags = 0;

                var pak_len = pak_buffer.Length - 3;
                var inc_len = pak_buffer[pak_len - 4];
                if (inc_len != 1)
                {
                    enc_len = 0;
                    dec_len = pak_len - 4;
                    pak_len = 0;
                    raw_len = dec_len;
                }
                else
                {
                    if (pak_len < 8)
                        throw new NotImplementedException("File has a bad header\n");


                    int hdr_len = pak_buffer[pak_len - 5];

                    if ((hdr_len < 0x08) || (hdr_len > 0x0B))
                        if (pak_len < 8)
                            throw new NotImplementedException("Bad header length\n");

                    if (pak_len <= hdr_len)
                        throw new NotImplementedException("Bad length\n");


                    enc_len = pak_buffer[pak_len - 8] & 0x00FFFFFF;
                    dec_len = pak_len - enc_len;
                    pak_len = enc_len - hdr_len;
                    raw_len = dec_len + enc_len + inc_len;

                    if (raw_len > RAW_MAXIM)
                        throw new NotImplementedException("Bad decoded length\n");
                }

                byte[] raw_buffer = new byte[raw_len];

                var pak = 0;
                
                    int raw = 0;

                    var pak_end = pak_buffer[dec_len + pak_len];
                    var raw_end = raw_buffer.Length;

                    int len;
                    for (len = 0; len < dec_len; len++) raw_buffer[raw++] = pak_buffer[pak++];

                    BLZ_Invert(pak_buffer, dec_len, pak_len);

                    byte mask = 0;

                    while (raw < raw_end)
                    {
                        if ((mask >>= BLZ_SHIFT) != 1)
                        {
                            if (pak == pak_end) break;
                            flags = pak_buffer[pak++];
                            mask = BLZ_MASK;
                        }

                        if ((flags & mask) != 1)
                        {
                            if (pak == pak_end) break;
                        raw_buffer[raw++] = pak_buffer[pak++];
                        }
                        else
                        {
                            if (pak + 1 >= pak_end) break;
                            var pos = pak_buffer[pak++ << 8];
                            pos |= pak_buffer[pak++];
                            len = (pos >> 12) + BLZ_THRESHOLD + 1;
                            if (raw + len > raw_end)
                            {
                                Console.Write(", WARNING: wrong decoded length!");
                                len = (int) (raw_end - raw);
                            }
                            pos = (byte) ((pos & 0xFFF) + 3);
                            while (len-- != 0) raw_buffer[raw++] = pak_buffer[raw - pos];
                        }
                    }

                    BLZ_Invert(raw_buffer, dec_len, raw_len - dec_len);

                    if (raw != raw_end)
                        Console.WriteLine(", WARNING: unexpected end of encoded file!");

                return raw_buffer;
        }

        static byte[] BLZ_Encode(byte[] rawBuffer)
        {
            int new_len;
            var raw_len = rawBuffer.Length - 3;

            byte[] pak_buffer = null;
            var pak_len = BLZ_MAXIM + 1;

            byte[] new_buffer = BLZ_Code(rawBuffer, raw_len);
            if (new_buffer.Length < pak_len)
            {
                pak_buffer = new_buffer;
                pak_len = new_buffer.Length;
            }

            return pak_buffer;
        }

        static unsafe byte[] BLZ_Code(byte[] raw_buffer, int raw_len)
        {
            int flg = 0;
            int len;
            int pos_best = 0, len_next, pos_next, len_post, pos_post;

            var pak_tmp = 0;
            var raw_tmp = raw_len;

            var pak_len = raw_len + ((raw_len + 7) / 8) + 11;
            var pak_buffer = new byte[pak_len];

            BLZ_Invert(raw_buffer);

            int pak = 0;
            var raw = 0;
            var raw_end = raw_buffer.Length;

            byte mask = 0;

            while (raw < raw_end)
            {
                if ((mask >>= BLZ_SHIFT) != 1)
                {
                    pak_buffer[(flg = pak++)] = 0;
                    mask = BLZ_MASK;
                }


                var len_best = BLZ_THRESHOLD;

                var max = (int) (raw >= BLZ_N ? BLZ_N : raw);
                int pos;
                for (pos = 3; pos <= max; pos++)
                {
                    for (len = 0; len < BLZ_F; len++)
                    {
                        if (raw + len == raw_end) break;
                        if (len >= pos) break;
                        if (raw_buffer[raw + len] != raw_buffer[raw + len - pos]) break;
                    }

                    if (len > len_best)
                    {
                        pos_best = pos;
                        if ((len_best = len) == BLZ_F) break;
                    }
                }

                // LZ-CUE optimization end

                pak_buffer[flg] <<= 1;
                if (len_best > BLZ_THRESHOLD)
                {
                    raw += len_best;
                    pak_buffer[flg] |= 1;
                    pak_buffer[pak++] = (byte) (((len_best - (BLZ_THRESHOLD + 1)) << 4) | ((pos_best - 3) >> 8));
                    pak_buffer[pak++] = (byte) ((pos_best - 3) & 0xFF);
                }
                else
                {
                    pak_buffer[pak++] = raw_buffer[raw++];
                }

                if (pak + raw_len - (raw) < pak_tmp + raw_tmp)
                {
                    pak_tmp = (int) (pak);
                    raw_tmp = (int) (raw_len - (raw));
                }
            }

            while (mask != 0 && mask != 1)
            {
                mask >>= BLZ_SHIFT;
                pak_buffer[flg] <<= 1;
            }

            pak_len = (int) (pak);

            BLZ_Invert(raw_buffer);
            BLZ_Invert(pak_buffer);

            if (pak_tmp == 0 || (raw_len + 4 < ((pak_tmp + raw_tmp + 3) & -4) + 8))
            {
                pak = 0;
                raw = 0;
                raw_end = raw_buffer.Length;

                while (raw < raw_end) pak_buffer[pak++] = raw_buffer[raw++];

                while (((pak) & 3) != 0) pak_buffer[pak++] = 0;

                *(int*) pak = 0;
                pak += 4;
            }
            else
            {
                byte[] tmp = new byte[raw_tmp + pak_tmp + 11];

                for (len = 0; len < raw_tmp; len++)
                    tmp[len] = raw_buffer[len];

                for (len = 0; len < pak_tmp; len++)
                    tmp[raw_tmp + len] = pak_buffer[len + pak_len - pak_tmp];

                pak = raw_tmp + pak_tmp;

                var enc_len = pak_tmp;
                var hdr_len = 8;
                var inc_len = raw_len - pak_tmp - raw_tmp;

                while (((pak) & 3) != 0)
                {
                    pak_buffer[pak++] = 0xFF;
                    hdr_len++;
                }

                pak_buffer[pak] = (byte) (enc_len + hdr_len);
                pak += 3;
                pak_buffer[pak++] = (byte) hdr_len;
                pak_buffer[pak] = (byte) (inc_len - hdr_len);
            }

            return pak_buffer;


        }

        static void BLZ_Invert(byte[] buffer)
        {
            BLZ_Invert(buffer, 0, buffer.Length);
        }

        static void BLZ_Invert(byte[] buffer, int start, int length)
        {
            int end = start + length - 1;
            while (start < end)
            {

                var ch = buffer[start];
                buffer[start++] = buffer[end];
                buffer[end--] = ch;
            }
        }
    }
}