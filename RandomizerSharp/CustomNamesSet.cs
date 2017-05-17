using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using RandomizerSharp.Constants;

namespace RandomizerSharp
{
    public class CustomNamesSet
    {
        private const int CustomNamesVersion = 1;
        private readonly IList<string> _doublesTrainerClasses;
        private readonly IList<string> _doublesTrainerNames;
        private readonly IList<string> _pokemonNicknames;
        private readonly IList<string> _trainerClasses;
        private readonly IList<string> _trainerNames;

        public CustomNamesSet(Stream data)
        {
            if (data.ReadByte() != CustomNamesVersion)
                throw new IOException("Invalid custom names file provided.");

            _trainerNames = ReadNamesBlock(data);
            _trainerClasses = ReadNamesBlock(data);
            _doublesTrainerNames = ReadNamesBlock(data);
            _doublesTrainerClasses = ReadNamesBlock(data);
            _pokemonNicknames = ReadNamesBlock(data);
        }

        public virtual byte[] Bytes
        {
            get
            {
                var baos = new MemoryStream();
                baos.WriteByte(CustomNamesVersion);
                WriteNamesBlock(baos, _trainerNames);
                WriteNamesBlock(baos, _trainerClasses);
                WriteNamesBlock(baos, _doublesTrainerNames);
                WriteNamesBlock(baos, _doublesTrainerClasses);
                WriteNamesBlock(baos, _pokemonNicknames);
                return baos.ToArray();
            }
        }

        public virtual IList<string> TrainerNames
        {
            get => _trainerNames;
            set
            {
                _trainerNames.Clear();
                ((List<string>) _trainerNames).AddRange(value);
            }
        }

        public virtual IList<string> TrainerClasses
        {
            get => _trainerClasses;
            set
            {
                _trainerClasses.Clear();
                ((List<string>) _trainerClasses).AddRange(value);
            }
        }

        public virtual IList<string> DoublesTrainerNames
        {
            get => _doublesTrainerNames;
            set
            {
                _doublesTrainerNames.Clear();
                ((List<string>) _doublesTrainerNames).AddRange(value);
            }
        }

        public virtual IList<string> DoublesTrainerClasses
        {
            get => _doublesTrainerClasses;
            set
            {
                _doublesTrainerClasses.Clear();
                ((List<string>) _doublesTrainerClasses).AddRange(value);
            }
        }

        public virtual IList<string> PokemonNicknames
        {
            get => _pokemonNicknames;
            set
            {
                _pokemonNicknames.Clear();
                ((List<string>) _pokemonNicknames).AddRange(value);
            }
        }

        private static IList<string> ReadNamesBlock(Stream @in)
        {
            var szData = FileFunctions.ReadFullyIntoBuffer(@in, 4);
            var size = FileFunctions.ReadFullInt(szData, 0);

            if (@in.Length < size)
                throw new IOException("Invalid size specified.");

            var namesData = FileFunctions.ReadFullyIntoBuffer(@in, size);
            IList<string> names = new List<string>();

            using (var stream = new MemoryStream(namesData))
            using (var reader = new StreamReader(stream))
            {
                for (var line = reader.ReadLine(); line != null; line = reader.ReadLine())
                {
                    var name = line.Trim();
                    if (name.Length > 0)
                        names.Add(name);
                }
            }

            return names;
        }

        private static void WriteNamesBlock(Stream @out, IEnumerable<string> names)
        {
            var newln = SysConstants.LineSep;
            var outNames = new StringBuilder();
            var first = true;
            foreach (var name in names)
            {
                if (!first)
                    outNames.Append(newln);

                first = false;
                outNames.Append(name);
            }
            var namesData = outNames.ToString().GetBytes(Encoding.UTF8);

            @out.Write(BitConverter.GetBytes(namesData.Length), 0, 4);
            @out.Write(namesData, 0, namesData.Length);
        }
    }

    //-------------------------------------------------------------------------------------------
    //	Copyright © 2007 - 2017 Tangible Software Solutions Inc.
    //	This class can be used by anyone provided that the copyright notice remains intact.
    //
    //	This class is used to convert some aspects of the Java String class.
    //-------------------------------------------------------------------------------------------
    internal static class StringHelperClass
    {
        //----------------------------------------------------------------------------------
        //	This method replaces the Java String.substring method when 'start' is a
        //	method call or calculated value to ensure that 'start' is obtained just once.
        //----------------------------------------------------------------------------------
        internal static string SubstringSpecial(this string self, int start, int end)
        {
            return self.Substring(start, end - start);
        }

        //------------------------------------------------------------------------------------
        //	This method is used to replace calls to the 2-arg Java String.startsWith method.
        //------------------------------------------------------------------------------------
        internal static bool StartsWith(this string self, string prefix, int toffset)
        {
            return self.IndexOf(prefix, toffset, StringComparison.Ordinal) == toffset;
        }

        //------------------------------------------------------------------------------
        //	This method is used to replace most calls to the Java String.split method.
        //------------------------------------------------------------------------------
        internal static string[] Split(this string self, string regexDelimiter, bool trimTrailingEmptyStrings)
        {
            var splitArray = Regex.Split(self, regexDelimiter);

            if (trimTrailingEmptyStrings)
                if (splitArray.Length > 1)
                    for (var i = splitArray.Length; i > 0; i--)
                        if (splitArray[i - 1].Length > 0)
                        {
                            if (i < splitArray.Length)
                                Array.Resize(ref splitArray, i);

                            break;
                        }

            return splitArray;
        }

        //-----------------------------------------------------------------------------
        //	These methods are used to replace calls to some Java String constructors.
        //-----------------------------------------------------------------------------
        internal static string NewString(byte[] bytes)
        {
            return NewString(bytes, 0, bytes.Length);
        }

        internal static string NewString(byte[] bytes, int index, int count)
        {
            return Encoding.UTF8.GetString(bytes, index, count);
        }

        //--------------------------------------------------------------------------------
        //	These methods are used to replace calls to the Java String.getBytes methods.
        //--------------------------------------------------------------------------------
        internal static byte[] GetBytes(this string self)
        {
            return GetSBytesForEncoding(Encoding.UTF8, self);
        }

        internal static byte[] GetBytes(this string self, Encoding encoding)
        {
            return GetSBytesForEncoding(encoding, self);
        }

        internal static byte[] GetBytes(this string self, string encoding)
        {
            return GetSBytesForEncoding(Encoding.GetEncoding(encoding), self);
        }

        private static byte[] GetSBytesForEncoding(Encoding encoding, string s)
        {
            var bytes = new byte[encoding.GetByteCount(s)];
            encoding.GetBytes(s, 0, s.Length, bytes, 0);
            return bytes;
        }
    }
}