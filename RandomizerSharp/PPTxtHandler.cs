using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using RandomizerSharp.Properties;

namespace RandomizerSharp
{
    public class PpTxtHandler
    {
        public static IDictionary<string, string> PokeToText = new Dictionary<string, string>();
        public static IDictionary<string, string> TextToPoke = new Dictionary<string, string>();
        public static string PokeToTextPattern, TextToPokePattern;
        private static IList<int> _lastKeys;
        private static IList<int> _lastUnknowns;

        static PpTxtHandler()
        {
            using (var memStr = new MemoryStream(Resources.Generation5))
            using (var strReader = new StreamReader(memStr))
            {
                for (var q = strReader.ReadLine(); q != null; q = strReader.ReadLine())
                    if (q.Trim().Length > 0)
                    {
                        var r = q.Split(new[] {'='}, 2);
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

        public static string MakePattern(IEnumerable<string> tokens)
        {
            return "(" + Implode(tokens, "|").Replace("\\", "\\\\").Replace("[", "\\[").Replace("]", "\\]")
                       .Replace("(", "\\(").Replace(")", "\\)") + ")";
        }

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

        private static IList<int> Decompress(IList<int> chars)
        {
            IList<int> uncomp = new List<int>();
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

        public static ArraySlice<string> ReadTexts(ArraySlice<byte> ds)
        {
            var pos = 0;
            _lastKeys = new List<int>();
            _lastUnknowns = new List<int>();

            ArraySlice<int> sizeSections = new [] { 0, 0, 0 };
            ArraySlice<int> sectionOffset = new [] { 0, 0, 0 };


            var numSections = ReadWord(ds, 0);
            var numEntries = ReadWord(ds, 2);
            
            ArraySlice<string> strings = new string[numEntries];

            sizeSections[0] = ReadLong(ds, 4);
            pos += 12;
            if (numSections > 0)
            {
                for (var z = 0; z < numSections; z++)
                {
                    sectionOffset[z] = ReadLong(ds, pos);
                    pos += 4;
                }

                pos = sectionOffset[0];
                sizeSections[0] = ReadLong(ds, pos);
                pos += 4;

                var tableOffsets = new List<int>();
                var characterCount = new List<int>();
                var encText = new List<IList<int>>();

                for (var j = 0; j < numEntries; j++)
                {
                    var tmpOffset = ReadLong(ds, pos);
                    pos += 4;
                    var tmpCharCount = ReadWord(ds, pos);
                    pos += 2;
                    var tmpUnknown = ReadWord(ds, pos);
                    pos += 2;
                    tableOffsets.Add(tmpOffset);
                    characterCount.Add(tmpCharCount);
                    _lastUnknowns.Add(tmpUnknown);
                }
                for (var j = 0; j < numEntries; j++)
                {
                    IList<int> tmpEncChars = new List<int>();
                    pos = sectionOffset[0] + tableOffsets[j];
                    for (var k = 0; k < characterCount[j]; k++)
                    {
                        var tmpChar = ReadWord(ds, pos);
                        pos += 2;
                        tmpEncChars.Add(tmpChar);
                    }
                    encText.Add(tmpEncChars);
                    var key = encText[j][characterCount[j] - 1] ^ 0xFFFF;
                    for (var k = characterCount[j] - 1; k >= 0; k--)
                    {
                        encText[j][k] = encText[j][k] ^ key;
                        if (k == 0)
                            _lastKeys.Add(key);
                        key = ((int) ((uint) key >> 3) | (key << 13)) & 0xffff;
                    }
                    if (encText[j][0] == 0xF100)
                    {
                        encText[j] = Decompress(encText[j]);
                        characterCount[j] = encText[j].Count;
                    }

                    var chars = new List<string>();
                    var str = new StringBuilder();

                    for (var k = 0; k < characterCount[j]; k++)
                        if (encText[j][k] == 0xFFFF)
                        {
                            chars.Add("\\xFFFF");
                        }
                        else
                        {
                            if (encText[j][k] > 20 && encText[j][k] <= 0xFFF0)
                            {
                                chars.Add("" + (char) encText[j][k]);
                            }
                            else
                            {
                                var num = $"{encText[j][k]:X4}";
                                chars.Add("\\x" + num);
                            }

                            str.Append(chars[k]);
                        }

                    strings[j] = str.ToString();
                }
            }
            for (var sn = 0; sn < strings.Count; sn++)
                strings[sn] = BulkReplace(strings[sn], PokeToTextPattern, PokeToText);

            return strings.Slice();
        }

        private static string BulkReplace(string @string, string pattern, IDictionary<string, string> replacements)
        {
            return Regex.Replace(@string, pattern, match => replacements[match.Groups[1].Value]);
        }

        public static ArraySlice<byte> SaveEntry(ArraySlice<byte> originalData, ArraySlice<string> text)
        {
            for (var sn = 0; sn < text.Count; sn++)
                text[sn] = BulkReplace(text[sn], TextToPokePattern, TextToPoke);

            ReadTexts(originalData);
            ArraySlice<int> sizeSections = new[] { 0, 0, 0};
            ArraySlice<int> sectionOffset = new[] { 0, 0, 0};
            ArraySlice<int> newsizeSections = new[] { 0, 0, 0};
            ArraySlice<int> newsectionOffset = new[] { 0, 0, 0};
            var ds = originalData;
            var pos = 0;
            var numSections = ReadWord(ds, 0);
            var numEntries = ReadWord(ds, 2);
            sizeSections[0] = ReadLong(ds, 4);
            pos += 12;
            if (text.Count < numEntries)
            {
                Console.Error.WriteLine("Can't do anything due to too few lines");
                return originalData;
            }
            var newEntry = MakeSection(text, numEntries);
            for (var z = 0; z < numSections; z++)
            {
                sectionOffset[z] = ReadLong(ds, pos);
                pos += 4;
            }
            for (var z = 0; z < numSections; z++)
            {
                pos = sectionOffset[z];
                sizeSections[z] = ReadLong(ds, pos);
            }
            newsizeSections[0] = newEntry.Length;
            var newDataSize = ds.Length - sizeSections[0] + newsizeSections[0];
            var newData = ds.Slice(Math.Min(ds.Length, newDataSize));

            WriteLong(newData, 4, newsizeSections[0]);
            if (numSections == 2)
            {
                newsectionOffset[1] = newsizeSections[0] + sectionOffset[0];
                WriteLong(newData, 0x10, newsectionOffset[1]);
            }

            newData = newEntry.SliceFrom(sectionOffset[0]);

            if (numSections == 2)
                newData = ds.SliceFrom(sectionOffset[1]).Slice(sizeSections[1], newsectionOffset[1]);

            return newData;
        }

        private static ArraySlice<byte> MakeSection(IList<string> strings, int numEntries)
        {
            IList<IList<int>> data = new List<IList<int>>();
            var size = 0;
            var offset = 4 + 8 * numEntries;
            for (var i = 0; i < numEntries; i++)
            {
                data.Add(ParseString(strings[i], i));
                size += data[i].Count * 2;
            }
            if (size % 4 == 2)
            {
                size += 2;
                var tmpKey = _lastKeys[numEntries - 1];
                for (var i = 0; i < data[numEntries - 1].Count; i++)
                    tmpKey = ((tmpKey << 3) | (tmpKey >> 13)) & 0xFFFF;
                data[numEntries - 1].Add(0xFFFF ^ tmpKey);
            }
            size += offset;
            var section = new byte[size];
            var pos = 0;
            WriteLong(section, pos, size);
            pos += 4;
            for (var i = 0; i < numEntries; i++)
            {
                var charCount = data[i].Count;
                WriteLong(section, pos, offset);
                pos += 4;
                WriteWord(section, pos, charCount);
                pos += 2;
                WriteWord(section, pos, _lastUnknowns[i]);
                pos += 2;
                offset += charCount * 2;
            }
            for (var i = 0; i < numEntries; i++)
                foreach (var word in data[i])
                {
                    WriteWord(section, pos, word);
                    pos += 2;
                }

            return section;
        }

        private static IList<int> ParseString(string @string, int entryId)
        {
            IList<int> chars = new List<int>();
            for (var i = 0; i < @string.Length; i++)
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
                        var substring = @string.Substring(i + 2, i + 6 - (i + 2));

                        if (substring.StartsWith("x"))
                            substring = substring.Remove(0, 1);

                        if (substring.StartsWith(@"\x"))
                            substring = substring.Remove(0, 3);

                        chars.Add(Convert.ToInt32(substring, 16));
                        i += 5;
                    }
                }
            chars.Add(0xFFFF);
            var key = _lastKeys[entryId];
            for (var i = 0; i < chars.Count; i++)
            {
                chars[i] = (chars[i] ^ key) & 0xFFFF;
                key = ((key << 3) | (int) ((uint) key >> 13)) & 0xFFFF;
            }
            return chars;
        }

        private static int ReadWord(ArraySlice<byte> data, int offset)
        {
            return (data[offset] & 0xFF) + ((data[offset + 1] & 0xFF) << 8);
        }

        private static int ReadLong(ArraySlice<byte> data, int offset)
        {
            return (data[offset] & 0xFF) + ((data[offset + 1] & 0xFF) << 8) + ((data[offset + 2] & 0xFF) << 16) +
                   ((data[offset + 3] & 0xFF) << 24);
        }

        protected internal static void WriteWord(ArraySlice<byte> data, int offset, int value)
        {
            data[offset] = unchecked((byte) (value & 0xFF));
            data[offset + 1] = unchecked((byte) ((value >> 8) & 0xFF));
        }

        protected internal static void WriteLong(ArraySlice<byte> data, int offset, int value)
        {
            data[offset] = unchecked((byte) (value & 0xFF));
            data[offset + 1] = unchecked((byte) ((value >> 8) & 0xFF));
            data[offset + 2] = unchecked((byte) ((value >> 16) & 0xFF));
            data[offset + 3] = unchecked((byte) ((value >> 24) & 0xFF));
        }
    }
}