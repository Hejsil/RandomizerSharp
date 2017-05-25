using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using RandomizerSharp.Properties;
using Tools.ExtensionsT;

namespace RandomizerSharp
{
    public class PpTxtHandler
    {
        public static string PokeToTextPattern, TextToPokePattern;
        private static readonly List<int> LastKeys = new List<int>();
        private static readonly List<int> LastUnknowns = new List<int>();

        private static int[] Decompress(IList<int> chars)
        {
            var uncomp = new List<int>();
            var j = 1;
            var shift1 = 0;
            var trans = 0;
            while (true)
            {
                var tmp = chars[j];
                tmp = tmp >> shift1;
                var tmp1 = tmp;
                if (shift1 >= 0x10)
                {
                    shift1 -= 0x10;
                    if (shift1 > 0)
                    {
                        tmp1 = trans | ((chars[j] << (9 - shift1)) & 0x1FF);
                        if ((tmp1 & 0xFF) == 0xFF)
                        {
                            break;
                        }
                        if (tmp1 != 0x0 && tmp1 != 0x1)
                        {
                            uncomp.Add(tmp1);
                        }
                    }
                }
                else
                {
                    tmp1 = (chars[j] >> shift1) & 0x1FF;
                    if ((tmp1 & 0xFF) == 0xFF)
                    {
                        break;
                    }
                    if (tmp1 != 0x0 && tmp1 != 0x1)
                    {
                        uncomp.Add(tmp1);
                    }
                    shift1 += 9;
                    if (shift1 < 0x10)
                    {
                        trans = (chars[j] >> shift1) & 0x1FF;
                        shift1 += 9;
                    }
                    j += 1;
                }
            }
            return uncomp.ToArray();
        }


        public static string[] ReadTexts(IList<byte> ds)
        {
            var pos = 0;

            LastKeys.Clear();
            LastUnknowns.Clear();
            int[] sizeSections = { 0, 0, 0 };
            int[] sectionOffset = { 0, 0, 0 };

            var numSections = ReadWord(ds, 0);
            var numEntries = ReadWord(ds, 2);
            var strings = new string[numEntries];
            sizeSections[0] = ReadWord(ds, 4);
            // unk1 = readLong(ds, 8);
            pos += 12;

            if (numSections <= 0)
                return strings;

            for (var z = 0; z < numSections; z++)
            {
                sectionOffset[z] = ReadInt(ds, pos);
                pos += 4;
            }

            pos = sectionOffset[0];
            sizeSections[0] = ReadInt(ds, pos);
            pos += 4;

            var tableOffsets = new List<int>();
            var characterCount = new List<int>();

            for (var j = 0; j < numEntries; j++)
            {
                var tmpOffset = ReadInt(ds, pos);
                pos += 4;
                var tmpCharCount = ReadInt(ds, pos);
                pos += 2;
                var tmpUnknown = ReadInt(ds, pos);
                pos += 2;
                tableOffsets.Add(tmpOffset);
                characterCount.Add(tmpCharCount);
                LastUnknowns.Add(tmpUnknown);
            }

            for (var j = 0; j < numEntries; j++)
            {
                var tmpEncChars = new int[characterCount[j]];
                pos = sectionOffset[0] + tableOffsets[j];
                for (var k = 0; k < characterCount[j]; k++)
                {
                    var tmpChar = ReadWord(ds, pos);
                    pos += 2;
                    tmpEncChars[k] = tmpChar;
                }

                var key = tmpEncChars[characterCount[j] - 1] ^ 0xFFFF;
                for (var k = characterCount[j] - 1; k >= 0; k--)
                {
                    tmpEncChars[k] = tmpEncChars[k] ^ key;
                    if (k == 0)
                    {
                        LastKeys.Add(key);
                    }
                    key = ((int) ((uint) key >> 3) | (key << 13)) & 0xffff;
                }
                if (tmpEncChars[0] == 0xF100)
                {
                    tmpEncChars = Decompress(tmpEncChars);
                    characterCount[j] = tmpEncChars.Length;
                }

                var strBuilder = new StringBuilder();

                foreach (var chr in tmpEncChars)
                {
                    if (chr == '\uffff')
                        continue;

                    strBuilder.Append((char)chr);
                }

                strings[j] = strBuilder.ToString();
            }

            return strings;
        }
        
        public static byte[] SaveEntry(IList<byte> od, IList<string> text)
        {
            var originalData = od.ToArray();

            // Make sure we have the original unknowns etc
            ReadTexts(originalData);

            // Start getting stuff
            int numSections, numEntries;
            int[] sizeSections = { 0, 0, 0 };
            int[] sectionOffset = { 0, 0, 0 };
            int[] newsizeSections = { 0, 0, 0 };
            int[] newsectionOffset = { 0, 0, 0 };

            // Data-Stream
            var ds = originalData;
            var pos = 0;

            numSections = ReadWord(ds, 0);
            numEntries = ReadWord(ds, 2);
            sizeSections[0] = ReadWord(ds, 4);
            // unk1 readLong(ds, 8);
            pos += 12;

            if (text.Count < numEntries)
            {
                Console.Error.WriteLine("Can't do anything due to too few lines");
                return originalData;
            }
            var newEntry = MakeSection(text, numEntries);
            for (var z = 0; z < numSections; z++)
            {
                sectionOffset[z] = ReadInt(ds, pos);
                pos += 4;
            }
            for (var z = 0; z < numSections; z++)
            {
                pos = sectionOffset[z];
                sizeSections[z] = ReadInt(ds, pos);
                pos += 4;
            }
            newsizeSections[0] = newEntry.Length;

            var newData = new byte[ds.Length - sizeSections[0] + newsizeSections[0]];
            Array.Copy(ds, 0, newData, 0, Math.Min(ds.Length, newData.Length));
            WriteInt(newData, 4, newsizeSections[0]);
            if (numSections == 2)
            {
                newsectionOffset[1] = newsizeSections[0] + sectionOffset[0];
                WriteInt(newData, 0x10, newsectionOffset[1]);
            }
            Array.Copy(newEntry, 0, newData, sectionOffset[0], newEntry.Length);
            if (numSections == 2)
            {
                Array.Copy(ds, sectionOffset[1], newData, newsectionOffset[1], sizeSections[1]);
            }
            return newData;
        }


        private static byte[] MakeSection(IList<string> strings, int numEntries)
        {
            IList<IList<int>> data = new List<IList<int>>();
            var size = 0;
            var offset = 4 + 8 * numEntries;
            int charCount;
            for (var i = 0; i < numEntries; i++)
            {
                data.Add(ParseString(strings[i], i));
                size += data[i].Count * 2;
            }
            if (size % 4 == 2)
            {
                size += 2;
                var tmpKey = LastKeys[numEntries - 1];
                for (var i = 0; i < data[numEntries - 1].Count; i++)
                {
                    tmpKey = ((tmpKey << 3) | (tmpKey >> 13)) & 0xFFFF;
                }
                data[numEntries - 1].Add(0xFFFF ^ tmpKey);
            }
            size += offset;
            var section = new byte[size];
            var pos = 0;
            WriteInt(section, pos, size);
            pos += 4;
            for (var i = 0; i < numEntries; i++)
            {
                charCount = data[i].Count;
                WriteInt(section, pos, offset);
                pos += 4;
                WriteWord(section, pos, charCount);
                pos += 2;
                WriteWord(section, pos, LastUnknowns[i]);
                pos += 2;
                offset += charCount * 2;
            }
            for (var i = 0; i < numEntries; i++)
            {
                foreach (var word in data[i])
                {
                    WriteWord(section, pos, word);
                    pos += 2;
                }
            }
            return section;
        }

        private static IList<int> ParseString(string @string, int entry_id)
        {
            IList<int> chars = new List<int>();
            for (var i = 0; i < @string.Length; i++)
            {
                if (@string[i] != '\\')
                {
                    chars.Add(@string[i]);
                }
                else
                {
                    if (i + 2 < @string.Length && @string[i + 2] == '{')
                    {
                        chars.Add(@string[i]);
                    }
                    else
                    {
                        chars.Add(Convert.ToInt32(@string.Substring(i + 2, i + 6 - (i + 2)), 16));
                        i += 5;
                    }
                }
            }
            chars.Add(0xFFFF);
            var key = LastKeys[entry_id];
            for (var i = 0; i < chars.Count; i++)
            {
                chars[i] = (chars[i] ^ key) & 0xFFFF;
                key = ((key << 3) | (int) ((uint) key >> 13)) & 0xFFFF;
            }
            return chars;
        }


        public static int ReadWord(IList<byte> data, int offset) => (data[offset] & 0xFF) +
                                                                    ((data[offset + 1] & 0xFF) << 8);

        public static int ReadInt(IList<byte> data, int offset) => (data[offset] & 0xFF) +
                                                                   ((data[offset + 1] & 0xFF) << 8) +
                                                                   ((data[offset + 2] & 0xFF) << 16) +
                                                                   ((data[offset + 3] & 0xFF) << 24);

        public static void WriteWord(IList<byte> data, int offset, int value)
        {
            data[offset] = unchecked((byte) (value & 0xFF));
            data[offset + 1] = unchecked((byte) ((value >> 8) & 0xFF));
        }

        public static void WriteInt(IList<byte> data, int offset, int value)
        {
            data[offset] = unchecked((byte) (value & 0xFF));
            data[offset + 1] = unchecked((byte) ((value >> 8) & 0xFF));
            data[offset + 2] = unchecked((byte) ((value >> 16) & 0xFF));
            data[offset + 3] = unchecked((byte) ((value >> 24) & 0xFF));
        }
    }
}