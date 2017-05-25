using System;
using System.Collections.Generic;
using System.Globalization;
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
        private static readonly List<int> LastKeys = new List<int>();
        private static readonly List<int> LastUnknowns = new List<int>();
        
        public static IDictionary<string, string> PokeToText = new Dictionary<string, string>();
        public static IDictionary<string, string> TextToPoke = new Dictionary<string, string>();

        public static string PokeToTextPattern, TextToPokePattern;
        
        static PpTxtHandler()
        {
            try
            {

                using (var memStr = new MemoryStream(Resources.Generation5))
                using (var strReader = new StreamReader(memStr))
                {
                    for (var q = strReader.ReadLine(); q != null; q = strReader.ReadLine())
                    {
                        if (q.Trim().Length > 0)
                        {
                            string[] r = q.Split("=", 2);
                            if (r[1].EndsWith("\r\n", StringComparison.Ordinal))
                            {
                                r[1] = r[1].Substring(0, r[1].Length - 2);
                            }
                            PokeToText[Convert.ToString((char) Convert.ToInt32(r[0], 16))] =
                                r[1].Replace("\\", "\\\\").Replace("$", "\\$");
                            TextToPoke[r[1]] = "\\\\x" + r[0];
                        }
                    }
                }

                PokeToTextPattern = makePattern(PokeToText.Keys);
                TextToPokePattern = makePattern(TextToPoke.Keys);
            }
            catch (FileNotFoundException)
            {
            }
        }

        public static string makePattern(IEnumerable<string> tokens)
        {
            return "(" + implode(tokens, "|").Replace("\\", "\\\\").Replace("[", "\\[").Replace("]", "\\]").Replace("(", "\\(").Replace(")", "\\)") + ")";
        }

        public static string implode(IEnumerable<string> tokens, string sep)
        {
            StringBuilder sb = new StringBuilder();
            bool first = true;
            foreach (string token in tokens)
            {
                if (!first)
                {
                    sb.Append(sep);
                }
                sb.Append(token);
                first = false;
            }
            return sb.ToString();
        }
        
        private static IList<int> Decompress(IList<int> chars)
        {
            IList<int> uncomp = new List<int>();
            int j = 1;
            int shift1 = 0;
            int trans = 0;
            while (true)
            {
                int tmp = chars[j];
                tmp = tmp >> shift1;
                int tmp1 = tmp;
                if (shift1 >= 0x10)
                {
                    shift1 -= 0x10;
                    if (shift1 > 0)
                    {
                        tmp1 = (trans | ((chars[j] << (9 - shift1)) & 0x1FF));
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
                    tmp1 = ((chars[j] >> shift1) & 0x1FF);
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
                        trans = ((chars[j] >> shift1) & 0x1FF);
                        shift1 += 9;
                    }
                    j += 1;
                }
            }
            return uncomp;
        }


    public static string[] ReadTexts(IList<byte> ds)
        {
            int pos = 0;
            int i = 0;

            LastKeys.Clear(); 
            LastUnknowns.Clear();
            IList<string> strings = new List<string>();
            int numSections, numEntries, tmpCharCount, tmpUnknown, tmpChar;
            int tmpOffset;
            int[] sizeSections = new int[] { 0, 0, 0 };
            int[] sectionOffset = new int[] { 0, 0, 0 };
            var tableOffsets = new Dictionary<int, IList<int>>();
            var characterCount = new Dictionary<int, IList<int>>();
            var unknown = new Dictionary<int, IList<int>>();
            var encText = new Dictionary<int, IList<IList<int>>>();
            string @string = "";
            int key;

            numSections = ReadWord(ds, 0);
            numEntries = ReadWord(ds, 2);
            sizeSections[0] = ReadWord(ds, 4);
            // unk1 = readLong(ds, 8);
            pos += 12;
            if (numSections > i)
            {
                for (int z = 0; z < numSections; z++)
                {
                    sectionOffset[z] = ReadInt(ds, pos);
                    pos += 4;
                }
                pos = sectionOffset[i];
                sizeSections[i] = ReadInt(ds, pos);
                pos += 4;
                tableOffsets[i] = new List<int>();
                characterCount[i] = new List<int>();
                unknown[i] = new List<int>();
                encText[i] = new List<IList<int>>();
                for (int j = 0; j < numEntries; j++)
                {
                    tmpOffset = ReadInt(ds, pos);
                    pos += 4;
                    tmpCharCount = ReadInt(ds, pos);
                    pos += 2;
                    tmpUnknown = ReadInt(ds, pos);
                    pos += 2;
                    tableOffsets[i].Add(tmpOffset);
                    characterCount[i].Add(tmpCharCount);
                    unknown[i].Add(tmpUnknown);
                    LastUnknowns.Add(tmpUnknown);
                }
                for (int j = 0; j < numEntries; j++)
                {
                    IList<int> tmpEncChars = new List<int>();
                    pos = sectionOffset[i] + tableOffsets[i][j];
                    for (int k = 0; k < characterCount[i][j]; k++)
                    {
                        tmpChar = ReadWord(ds, pos);
                        pos += 2;
                        tmpEncChars.Add(tmpChar);
                    }
                    encText[i].Add(tmpEncChars);
                    key = encText[i][j][characterCount[i][j] - 1] ^ 0xFFFF;
                    for (int k = characterCount[i][j] - 1; k >= 0; k--)
                    {
                        encText[i][j][k] = (encText[i][j][k]) ^ key;
                        if (k == 0)
                        {
                            LastKeys.Add(key);
                        }
                        key = (((int)((uint)key >> 3)) | (key << 13)) & 0xffff;
                    }
                    if (encText[i][j][0] == 0xF100)
                    {
                        encText[i][j] = Decompress(encText[i][j]);
                        characterCount[i][j] = encText[i][j].Count;
                    }
                    IList<string> chars = new List<string>();
                    @string = "";

                    for (int k = 0; k < characterCount[i][j]; k++)
                    {
                        @string += (char)encText[i][j][k];
                    }

                    strings.Add(@string);
                }
            }

            // Parse strings against the table
            for (int sn = 0; sn < strings.Count; sn++)
            {
                strings[sn] = BulkReplace(strings[sn], PokeToTextPattern, PokeToText);
            }
            return strings.ToArray();
        }


    private static string BulkReplace(string @string, string pattern, IDictionary<string, string> replacements)
        {
            return Regex.Replace(@string, pattern, match => replacements[match.Groups[1].Value]);
        }
        

        public static byte[] SaveEntry(IList<byte> od, IList<string> text)
        {
            var originalData = od.ToArray();

            // Parse strings against the reverse table
            for (int sn = 0; sn < text.Count; sn++)
            {
                text[sn] = BulkReplace(text[sn], TextToPokePattern, TextToPoke);
            }

            // Make sure we have the original unknowns etc
            ReadTexts(originalData);

            // Start getting stuff
            int numSections, numEntries;
            int[] sizeSections = new int[] { 0, 0, 0 };
            int[] sectionOffset = new int[] { 0, 0, 0 };
            int[] newsizeSections = new int[] { 0, 0, 0 };
            int[] newsectionOffset = new int[] { 0, 0, 0 };

            // Data-Stream
            byte[] ds = originalData;
            int pos = 0;

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
            else
            {
                byte[] newEntry = MakeSection(text, numEntries);
                for (int z = 0; z < numSections; z++)
                {
                    sectionOffset[z] = ReadInt(ds, pos);
                    pos += 4;
                }
                for (int z = 0; z < numSections; z++)
                {
                    pos = sectionOffset[z];
                    sizeSections[z] = ReadInt(ds, pos);
                    pos += 4;
                }
                newsizeSections[0] = newEntry.Length;

                byte[] newData = new byte[ds.Length - sizeSections[0] + newsizeSections[0]];
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
        }

       

        private static byte[] MakeSection(IList<string> strings, int numEntries)
        {
            IList<IList<int>> data = new List<IList<int>>();
            int size = 0;
            int offset = 4 + 8 * numEntries;
            int charCount;
            for (int i = 0; i < numEntries; i++)
            {
                data.Add(ParseString(strings[i], i));
                size += (data[i].Count * 2);
            }
            if (size % 4 == 2)
            {
                size += 2;
                int tmpKey = LastKeys[numEntries - 1];
                for (int i = 0; i < data[numEntries - 1].Count; i++)
                {
                    tmpKey = ((tmpKey << 3) | (tmpKey >> 13)) & 0xFFFF;
                }
                data[numEntries - 1].Add(0xFFFF ^ tmpKey);
            }
            size += offset;
            byte[] section = new byte[size];
            int pos = 0;
            WriteInt(section, pos, size);
            pos += 4;
            for (int i = 0; i < numEntries; i++)
            {
                charCount = data[i].Count;
                WriteInt(section, pos, offset);
                pos += 4;
                WriteWord(section, pos, charCount);
                pos += 2;
                WriteWord(section, pos, LastUnknowns[i]);
                pos += 2;
                offset += (charCount * 2);
            }
            for (int i = 0; i < numEntries; i++)
            {
                foreach (int word in data[i])
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
            for (int i = 0; i < @string.Length; i++)
            {
                if (@string[i] != '\\')
                {
                    chars.Add((int)@string[i]);
                }
                else
                {
                    if (((i + 2) < @string.Length) && @string[i + 2] == '{')
                    {
                        chars.Add((int)@string[i]);
                    }
                    else
                    {
                        chars.Add(Convert.ToInt32(@string.Substring(i + 2, (i + 6) - (i + 2)), 16));
                        i += 5;
                    }
                }
            }
            chars.Add(0xFFFF);
            int key = LastKeys[entry_id];
            for (int i = 0; i < chars.Count; i++)
            {
                chars[i] = (chars[i] ^ key) & 0xFFFF;
                key = ((key << 3) | ((int)((uint)key >> 13))) & 0xFFFF;
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