using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using RandomizerSharp.Properties;

namespace RandomizerSharp
{
    public class PpTxtHandler
    {
        public static IDictionary<string, string> PokeToText { get; } = new Dictionary<string, string>();
        public static string PokeToTextPattern { get; }
        public static IDictionary<string, string> TextToPoke { get; } = new Dictionary<string, string>();
        public static string TextToPokePattern { get; }

        private static readonly List<int> LastKeys = new List<int>();
        private static readonly List<int> LastUnknowns = new List<int>();

        static PpTxtHandler()
        {
            using (var memStr = new MemoryStream(Resources.Generation5))
            using (var strReader = new StreamReader(memStr))
            {
                for (var q = strReader.ReadLine(); q != null; q = strReader.ReadLine())
                {
                    if (q.Trim().Length <= 0)
                        continue;

                    var r = q.Split(new[] { '=' }, 2);
                    if (r[1].EndsWith("\r\n", StringComparison.Ordinal))
                        r[1] = r[1].Substring(0, r[1].Length - 2);
                    PokeToText[Convert.ToString((char) Convert.ToInt32(r[0], 16))] =
                        r[1].Replace("\\", "\\\\").Replace("$", "\\$");
                    TextToPoke[r[1]] = "\\\\x" + r[0];
                }

                PokeToTextPattern = MakePattern(PokeToText.Keys);
                TextToPokePattern = MakePattern(TextToPoke.Keys);
            }
        }

        public static string MakePattern(IEnumerable<string> tokens) => "(" +
                                                                        Implode(tokens, "|")
                                                                            .Replace("\\", "\\\\")
                                                                            .Replace("[", "\\[")
                                                                            .Replace("]", "\\]")
                                                                            .Replace("(", "\\(")
                                                                            .Replace(")", "\\)") +
                                                                        ")";

        public static string Implode(IEnumerable<string> tokens, string sep)
        {
            var sb = new StringBuilder();
            var first = true;
            foreach (var token in tokens)
            {
                if (!first)
                    sb.Append(sep);
                sb.Append(token);
                first = false;
            }
            return sb.ToString();
        }

        private static List<int> Decompress(IList<int> chars)
        {
            var uncomp = new List<int>();
            var j = 1;
            var shift1 = 0;
            var trans = 0;
            while (true)
            {
                int tmp;
                if (shift1 >= 0x10)
                {
                    shift1 -= 0x10;

                    if (shift1 <= 0)
                        continue;

                    tmp = trans | ((chars[j] << (9 - shift1)) & 0x1FF);
                    if ((tmp & 0xFF) == 0xFF)
                        break;
                    if (tmp != 0x0 && tmp != 0x1)
                        uncomp.Add(tmp);
                }
                else
                {
                    tmp = (chars[j] >> shift1) & 0x1FF;
                    if ((tmp & 0xFF) == 0xFF)
                        break;
                    if (tmp != 0x0 && tmp != 0x1)
                        uncomp.Add(tmp);
                    shift1 += 9;
                    if (shift1 < 0x10)
                    {
                        trans = (chars[j] >> shift1) & 0x1FF;
                        shift1 += 9;
                    }
                    j += 1;
                }
            }
            return uncomp;
        }

        public static string[] ReadTexts(IList<byte> ds)
        {
            var pos = 0;
            LastKeys.Clear();
            LastUnknowns.Clear();

            var sizeSections = new[] { 0, 0, 0 };
            var sectionOffset = new[] { 0, 0, 0 };


            var numSections = ReadWord(ds, 0);
            var numEntries = ReadWord(ds, 2);

            var strings = new string[numEntries];

            sizeSections[0] = ReadInt(ds, 4);
            pos += 12;
            if (numSections > 0)
            {
                for (var z = 0; z < numSections; z++)
                {
                    sectionOffset[z] = ReadInt(ds, pos);
                    pos += 4;
                }

                pos = sectionOffset[0];
                sizeSections[0] = ReadInt(ds, pos);
                pos += 4;

                var tableOffsets = new int[numEntries];
                var characterCount = new int[numEntries];

                for (var j = 0; j < numEntries; j++)
                {
                    var tmpOffset = ReadInt(ds, pos);
                    pos += 4;
                    var tmpCharCount = ReadWord(ds, pos);
                    pos += 2;
                    var tmpUnknown = ReadWord(ds, pos);
                    pos += 2;
                    tableOffsets[j] = tmpOffset;
                    characterCount[j] = tmpCharCount;
                    LastUnknowns.Add(tmpUnknown);
                }

                for (var j = 0; j < numEntries; j++)
                {
                    var tmpEncChars = new List<int>();
                    pos = sectionOffset[0] + tableOffsets[j];

                    for (var k = 0; k < characterCount[j]; k++)
                    {
                        var tmpChar = ReadWord(ds, pos);
                        pos += 2;
                        tmpEncChars.Add(tmpChar);
                    }


                    var key = tmpEncChars[characterCount[j] - 1] ^ 0xFFFF;
                    for (var k = characterCount[j] - 1; k >= 0; k--)
                    {
                        tmpEncChars[k] = tmpEncChars[k] ^ key;

                        if (k == 0)
                            LastKeys.Add(key);

                        key = ((int) ((uint) key >> 3) | (key << 13)) & 0xffff;
                    }

                    if (tmpEncChars[0] == 0xF100)
                    {
                        tmpEncChars = Decompress(tmpEncChars);
                        characterCount[j] = tmpEncChars.Count;
                    }

                    var str = new StringBuilder(characterCount[j]);

                    for (var k = 0; k < characterCount[j]; k++)
                    {
                        if (tmpEncChars[k] == 0xFFFF)
                            continue;

                        if (tmpEncChars[k] > 20 && tmpEncChars[k] <= 0xFFF0)
                            str.Append((char) tmpEncChars[k]);
                        else
                            str.AppendFormat("\\x{0:X4}", tmpEncChars[k]);
                    }

                    strings[j] = str.ToString();
                }
            }

            for (var sn = 0; sn < strings.Length; sn++)
                strings[sn] = BulkReplace(strings[sn], PokeToTextPattern, PokeToText);

            return strings;
        }

        private static string BulkReplace(string @string, string pattern, IDictionary<string, string> replacements)
        {
            return Regex.Replace(@string, pattern, match => replacements[match.Groups[1].Value]);
        }

        public static ArraySlice<byte> SaveEntry(ArraySlice<byte> originalData, IEnumerable<string> text)
        {
            var newText = text.Select(t => BulkReplace(t, TextToPokePattern, TextToPoke)).ToArray();

            ReadTexts(originalData);

            var sizeSections = new[] { 0, 0, 0 };
            var sectionOffset = new[] { 0, 0, 0 };
            var newsizeSections = new[] { 0, 0, 0 };
            var newsectionOffset = new[] { 0, 0, 0 };
            var ds = originalData;
            var pos = 0;
            var numSections = ReadWord(ds, 0);
            var numEntries = ReadWord(ds, 2);

            sizeSections[0] = ReadInt(ds, 4);
            pos += 12;

            if (newText.Length < numEntries)
            {
                Console.Error.WriteLine("Can't do anything due to too few lines");
                return originalData;
            }

            var newEntry = MakeSection(newText, numEntries);
            for (var z = 0; z < numSections; z++)
            {
                sectionOffset[z] = ReadInt(ds, pos);
                pos += 4;
            }

            for (var z = 0; z < numSections; z++)
            {
                pos = sectionOffset[z];
                sizeSections[z] = ReadInt(ds, pos);
            }

            newsizeSections[0] = newEntry.Length;

            var newDataSize = ds.Length - sizeSections[0] + newsizeSections[0];
            var newData = ds.Slice(Math.Min(ds.Length, newDataSize));

            WriteInt(newData, 4, newsizeSections[0]);
            if (numSections == 2)
            {
                newsectionOffset[1] = newsizeSections[0] + sectionOffset[0];
                WriteInt(newData, 0x10, newsectionOffset[1]);
            }

            newData = newEntry.SliceFrom(sectionOffset[0]);

            if (numSections == 2)
                newData = ds.SliceFrom(sectionOffset[1]).Slice(sizeSections[1], newsectionOffset[1]);

            return newData;
        }

        private static byte[] MakeSection(IList<string> strings, int numEntries)
        {
            var data = new List<int[]>();
            var size = 0;
            var offset = 4 + 8 * numEntries;

            for (var i = 0; i < numEntries; i++)
            {
                data.Add(ParseString(strings[i], i));
                size += data[i].Length * 2;
            }

            if (size % 4 == 2)
            {
                size += 2;
                var tmpKey = LastKeys[numEntries - 1];
                var entry = data[numEntries - 1];

                for (var i = 0; i < entry.Length; i++)
                    tmpKey = ((tmpKey << 3) | (tmpKey >> 13)) & 0xFFFF;

                data[numEntries - 1][entry.Length - 1] = 0xFFFF ^ tmpKey;
            }

            size += offset;
            var section = new byte[size];
            var pos = 0;
            WriteInt(section, pos, size);
            pos += 4;

            for (var i = 0; i < numEntries; i++)
            {
                var charCount = data[i].Length;
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

        private static int[] ParseString(string str, int entryId)
        {
            var chars = new int[str.Length + 2];
            for (var i = 0; i < str.Length; i++)
            {
                if (str[i] != '\\')
                {
                    chars[i] = str[i];
                }
                else
                {
                    if (i + 2 < str.Length && str[i + 2] == '{')
                    {
                        chars[i] = str[i];
                    }
                    else
                    {
                        var substring = str.Substring(i + 2, i + 6 - (i + 2));

                        if (substring.StartsWith("x"))
                            substring = substring.Remove(0, 1);

                        if (substring.StartsWith(@"\x"))
                            substring = substring.Remove(0, 3);

                        chars[i] = Convert.ToInt32(substring, 16);
                        i += 5;
                    }
                }
            }

            chars[str.Length] = 0xFFFF;

            var key = LastKeys[entryId];
            for (var i = 0; i < chars.Length; i++)
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