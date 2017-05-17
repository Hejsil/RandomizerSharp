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
        private readonly IList<string> _doublesTrainerClasses;
        private readonly IList<string> _doublesTrainerNames;
        private readonly IList<string> _pokemonNicknames;
        private readonly IList<string> _trainerClasses;
        private readonly IList<string> _trainerNames;

        public CustomNamesSet(Stream data)
        {
            if (data.ReadByte() != CustomNamesVersion) throw new IOException("Invalid custom names file provided.");

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

            if (@in.Length < size) throw new IOException("Invalid size specified.");

            var namesData = FileFunctions.ReadFullyIntoBuffer(@in, size);
            IList<string> names = new List<string>();

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