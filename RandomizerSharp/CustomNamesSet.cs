using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using RandomizerSharp.Constants;

namespace RandomizerSharp
{
    public class CustomNamesSet
    {
        private const int CustomNamesVersion = 1;
        private readonly List<string> _doublesTrainerClasses;
        private readonly List<string> _doublesTrainerNames;
        private readonly List<string> _pokemonNicknames;
        private readonly List<string> _trainerClasses;
        private readonly List<string> _trainerNames;

        public CustomNamesSet(Stream data)
        {
            if (data.ReadByte() != CustomNamesVersion) throw new IOException("Invalid custom names file provided.");

            _trainerNames = ReadNamesBlock(data);
            _trainerClasses = ReadNamesBlock(data);
            _doublesTrainerNames = ReadNamesBlock(data);
            _doublesTrainerClasses = ReadNamesBlock(data);
            _pokemonNicknames = ReadNamesBlock(data);
        }

        public IReadOnlyList<byte> Bytes
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

        public IList<string> TrainerNames
        {
            get => _trainerNames;
            set
            {
                _trainerNames.Clear();
                _trainerNames.AddRange(value);
            }
        }

        public IList<string> TrainerClasses
        {
            get => _trainerClasses;
            set
            {
                _trainerClasses.Clear();
                _trainerClasses.AddRange(value);
            }
        }

        public List<string> DoublesTrainerNames
        {
            get => _doublesTrainerNames;
            set
            {
                _doublesTrainerNames.Clear();
                _doublesTrainerNames.AddRange(value);
            }
        }

        public List<string> DoublesTrainerClasses
        {
            get => _doublesTrainerClasses;
            set
            {
                _doublesTrainerClasses.Clear();
                _doublesTrainerClasses.AddRange(value);
            }
        }

        public List<string> PokemonNicknames
        {
            get => _pokemonNicknames;
            set
            {
                _pokemonNicknames.Clear();
                _pokemonNicknames.AddRange(value);
            }
        }

        private static List<string> ReadNamesBlock(Stream @in)
        {
            var szData = FileFunctions.ReadFullyIntoBuffer(@in, 4);
            var size = PpTxtHandler.ReadInt(szData, 0);

            if (@in.Length < size) throw new IOException("Invalid size specified.");

            var namesData = FileFunctions.ReadFullyIntoBuffer(@in, size);
            var names = new List<string>();

            using (var stream = new MemoryStream(namesData))
            using (var reader = new StreamReader(stream))
            {
                for (var line = reader.ReadLine(); line != null; line = reader.ReadLine())
                {
                    var name = line.Trim();
                    if (name.Length > 0) names.Add(name);
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
                if (!first) outNames.Append(newln);

                first = false;
                outNames.Append(name);
            }
            var namesData = Encoding.UTF8.GetBytes(outNames.ToString());

            @out.Write(BitConverter.GetBytes(namesData.Length), 0, 4);
            @out.Write(namesData, 0, namesData.Length);
        }
    }
}