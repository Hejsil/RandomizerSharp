using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using RandomizerSharp.Constants;
using RandomizerSharp.NDS;
using RandomizerSharp.PokemonModel;
using RandomizerSharp.Properties;

namespace RandomizerSharp.RomHandlers
{
    public class Gen5RomHandler : AbstractDsRomHandler
    {
        public static List<RomEntry> Roms = new List<RomEntry>();
        public ArraySlice<byte> Arm9 { get; }

        private readonly Dictionary<HiddenHollowEntry, HiddenHollow> _hiddenHollows = new Dictionary<HiddenHollowEntry, HiddenHollow>();

        private readonly Dictionary<int, string> _wildDictionaryNames = new Dictionary<int, string>();

        private readonly NarcArchive _pokeNarc;
        private readonly NarcArchive _moveNarc;
        private readonly NarcArchive _stringsNarc;
        private readonly NarcArchive _storyTextNarc;
        private readonly NarcArchive _scriptNarc;
        private readonly NarcArchive _hhNarc;
        private readonly NarcArchive _tradeNarc;
        private readonly NarcArchive _evoNarc;
        private readonly NarcArchive _driftveil;
        private readonly ArraySlice<string> _tradeStrings;
        private readonly ArraySlice<string> _mnames;
        private readonly ArraySlice<byte> _mtFile;

        static Gen5RomHandler()
        {
            LoadRomInfo();
        }

        public Gen5RomHandler(string filename)
            : base(filename)
        {
            REntry = EntryFor(BaseRom.Code);
            Arm9 = ReadArm9();

            _stringsNarc = ReadNarc(REntry.GetString("TextStrings"));
            _storyTextNarc = ReadNarc(REntry.GetString("TextStory"));
            _scriptNarc = ReadNarc(REntry.GetString("Scripts"));
            _pokeNarc = ReadNarc(REntry.GetString("PokemonStats"));
            _moveNarc = ReadNarc(REntry.GetString("MoveData"));
            _hhNarc = ReadNarc(REntry.GetString("HiddenHollows"));
            _tradeNarc = ReadNarc(REntry.GetString("InGameTrades"));
            _evoNarc = ReadNarc(REntry.GetString("PokemonEvolutions"));
            _driftveil = ReadNarc(REntry.GetString("DriftveilPokemon"));

            _mtFile = ReadOverlay(REntry.GetInt("MoveTutorOvlNumber"));

            AbilityNames = GetStrings(false, REntry.GetInt("AbilityNamesTextOffset"));
            ItemNames = GetStrings(false, REntry.GetInt("ItemNamesTextOffset"));
            TrainerClassNames = GetStrings(false, REntry.GetInt("TrainerClassesTextOffset")).Slice();
            _tradeStrings = GetStrings(false, REntry.GetInt("IngameTradesTextOffset"));
            _mnames = GetStrings(false, REntry.GetInt("TrainerMugshotsTextOffset"));

            RomName = "Pokemon " + REntry.Name;
            RomCode = REntry.RomCode;
            SupportLevel = REntry.StaticPokemonSupport ? "Complete" : "No Static Pokemon";

            CanChangeTrainerText = true;
            FixedTrainerClassNamesLength = false;
            HasTimeBasedEncounters = true;
            HasDVs = false;
            SupportsFourStartingMoves = true;

            GenerationOfPokemon = 5;
            AbilitiesPerPokemon = 3;
            MaxTrainerNameLength = 10;
            MaxTrainerClassNameLength = 12;

            TcNameLengthsByTrainer = Slice<int>.Empty;
            HighestAbilityIndex = Gen5Constants.HighestAbilityIndex;
            TrainerNameMode = TrainerNameMode.MaxLength;

            AllowedItems = Gen5Constants.AllowedItems;
            NonBadItems = Gen5Constants.NonBadItems;

            CanChangeStaticPokemon = REntry.StaticPokemonSupport;

            LoadPokemon();
            LoadStaticPokemon();
            LoadPokemonSprites();
            
            LoadMoves();
            LoadMovesLearnt();
            LoadTmMoves();
            LoadHmMoves();
            LoadMoveTutorMoves();

            LoadFiledItems();

            LoadHiddenHollow();
            LoadEncounters();
            LoadStarters();
            LoadMiscTweaksAvailable();
            LoadTmhmCompatibility();
            LoadMoveTutorCompatibility();
            
            LoadIngameTrades();


            LoadDoublesTrainerClasses();
            LoadTrainerNames();
            LoadTrainers();
        }

        public RomEntry REntry { get; set; }

        public IEnumerable<HiddenHollow> HiddenHollows => _hiddenHollows.Values;

        public void LoadMovesLearnt()
        {
            MovesLearnt.Clear();
            var movesLearnt = ReadNarc(REntry.GetString("PokemonMovesets"));

            for (var i = 1; i <= Gen5Constants.PokemonCount; i++)
            {
                var pkmn = AllPokemons[i];
                var movedata = movesLearnt.Files[i];
                var moveDataLoc = 0;
                var learnt = new List<MoveLearnt>();
                while (ReadWord(movedata, moveDataLoc) != 0xFFFF ||
                       ReadWord(movedata, moveDataLoc + 2) != 0xFFFF)
                {
                    var move = ReadWord(movedata, moveDataLoc);
                    var level = ReadWord(movedata, moveDataLoc + 2);
                    var ml = new MoveLearnt
                    {
                        Level = level,
                        Move = move
                    };
                    learnt.Add(ml);
                    moveDataLoc += 4;
                }
                MovesLearnt[pkmn] = learnt;
            }
        }

        public void SaveMovesLearnt()
        {
            var movesLearnt = ReadNarc(REntry.GetString("PokemonMovesets"));
            for (var i = 1; i <= Gen5Constants.PokemonCount; i++)
            {
                var pkmn = AllPokemons[i];
                var learnt = MovesLearnt[pkmn];
                var sizeNeeded = learnt.Count * 4 + 4;
                var moveset = new byte[sizeNeeded];
                var j = 0;
                for (; j < learnt.Count; j++)
                {
                    var ml = learnt[j];
                    WriteWord(moveset, j * 4, ml.Move);
                    WriteWord(moveset, j * 4 + 2, ml.Level);
                }
                WriteWord(moveset, j * 4, 0xFFFF);
                WriteWord(moveset, j * 4 + 2, 0xFFFF);
                movesLearnt.Files[i] = moveset;
            }
            // Save
            WriteNarc(REntry.GetString("PokemonMovesets"), movesLearnt);
        }

        public static void LoadRomInfo()
        {
            RomEntry current = null;
            using (var memStr = new StringReader(Resources.gen5_offsets))
            {
                for (var q = memStr.ReadLine(); q != null; q = memStr.ReadLine())
                {
                    if (q.Contains("//"))
                        q = q.Substring(0, q.IndexOf("//", StringComparison.Ordinal)).Trim();

                    if (q.IsEmpty())
                        continue;

                    if (q.StartsWith("[") && q.EndsWith("]"))
                    {
                        // New rom
                        current = new RomEntry { Name = q.Substring(1, q.Length - 1) };
                        Roms.Add(current);
                    }
                    else
                    {
                        var r = q.Split(new[] { '=' }, 2);
                        if (r.Length == 1)
                        {
                            Console.WriteLine(@"invalid entry " + q);
                            continue;
                        }
                        if (r[1].EndsWith("\r\n"))
                            r[1] = r[1].Substring(0, r[1].Length - 2);
                        r[1] = r[1].Trim();
                        if (r[0].Equals("Game"))
                        {
                            current.RomCode = r[1];
                        }
                        else if (r[0].Equals("Type"))
                        {
                            current.RomType = r[1].Equals("BW2", StringComparison.InvariantCultureIgnoreCase)
                                ? Gen5Constants.TypeBw2
                                : Gen5Constants.TypeBw;
                        }
                        else if (r[0].Equals("CopyFrom"))
                        {
                            foreach (var otherEntry in Roms)
                                if (r[1].Equals(otherEntry.RomCode, StringComparison.InvariantCultureIgnoreCase))
                                {
                                    current.ArrayEntries.AddAll(otherEntry.ArrayEntries);
                                    current.Numbers.AddAll(otherEntry.Numbers);
                                    current.Strings.AddAll(otherEntry.Strings);
                                    current.OffsetArrayEntries.AddAll(otherEntry.OffsetArrayEntries);
                                    if (current.CopyStaticPokemon)
                                    {
                                        current.StaticPokemon.AddRange(otherEntry.StaticPokemon);
                                        current.StaticPokemonSupport = true;
                                    }
                                    else
                                    {
                                        current.StaticPokemonSupport = false;
                                    }
                                }
                        }
                        else if (r[0].Equals("StaticPokemon[]"))
                        {
                            if (r[1].StartsWith("[") &&
                                r[1].EndsWith("]"))
                            {
                                var offsets = r[1].Substring(1, r[1].Length - 2).Split(',');
                                var offs = new int[offsets.Length];
                                var files = new int[offsets.Length];
                                var c = 0;
                                foreach (var off in
                                    offsets)
                                {
                                    var parts = Regex.Split(off, "\\:");
                                    files[c] = ParseRiInt(parts[0]);
                                    offs[c++] = ParseRiInt(parts[1]);
                                }
                                var sp = new Gen5StaticPokemon
                                {
                                    Files = files,
                                    Offsets = offs
                                };
                                current.StaticPokemon.Add(sp);
                            }
                            else
                            {
                                var parts = Regex.Split(r[1], "\\:");
                                var files = ParseRiInt(parts[0]);
                                var offs = ParseRiInt(parts[1]);
                                var sp = new Gen5StaticPokemon
                                {
                                    Files = new[] { files },
                                    Offsets = new[] { offs }
                                };
                                current.StaticPokemon.Add(sp);
                            }
                        }
                        else if (r[0].Equals("StaticPokemonSupport"))
                        {
                            var spsupport = ParseRiInt(r[1]);
                            current.StaticPokemonSupport = spsupport > 0;
                        }
                        else if (r[0].Equals("CopyStaticPokemon"))
                        {
                            var csp = ParseRiInt(r[1]);
                            current.CopyStaticPokemon = csp > 0;
                        }
                        else if (r[0].StartsWith("StarterOffsets") ||
                                 r[0].Equals("StaticPokemonFormValues"))
                        {
                            var offsets = r[1].Substring(1, r[1].Length - 2).Split(',');
                            var offs = new OffsetWithinEntry[offsets.Length];
                            var c = 0;
                            foreach (var off in
                                offsets)
                            {
                                var parts = Regex.Split(off, "\\:");
                                var owe = new OffsetWithinEntry
                                {
                                    Entry = ParseRiInt(parts[0]),
                                    Offset = ParseRiInt(parts[1])
                                };
                                offs[c++] = owe;
                            }
                            current.OffsetArrayEntries[r[0]] = offs;
                        }
                        else if (r[0].EndsWith("Tweak"))
                        {
                            current.TweakFiles[r[0]] = r[1];
                        }
                        else
                        {
                            if (r[1].StartsWith("[") &&
                                r[1].EndsWith("]"))
                            {
                                var offsets = r[1].Substring(1, r[1].Length - 2).Split(',');
                                if (offsets.Length == 1 &&
                                    offsets[0].Trim().IsEmpty())
                                {
                                    current.ArrayEntries[r[0]] = new int[0];
                                }
                                else
                                {
                                    var offs = new int[offsets.Length];
                                    var c = 0;
                                    foreach (var off in
                                        offsets)
                                        offs[c++] = ParseRiInt(off);
                                    current.ArrayEntries[r[0]] = offs;
                                }
                            }
                            else if (r[0].EndsWith("Offset") ||
                                     r[0].EndsWith("Count") ||
                                     r[0].EndsWith("Number"))
                            {
                                var offs = ParseRiInt(r[1]);
                                current.Numbers[r[0]] = offs;
                            }
                            else
                            {
                                current.Strings[r[0]] = r[1];
                            }
                        }
                    }
                }
            }
        }

        public static int ParseRiInt(string off)
        {
            var radix = 10;
            off = off.Trim().ToLower();

            if (!off.StartsWith("0x") && !off.StartsWith("&h"))
                return Convert.ToInt32(off, radix);

            radix = 16;
            off = off.Substring(2);

            return Convert.ToInt32(off, radix);
        }

        public static RomEntry EntryFor(string ndsCode)
        {
            return Roms.FirstOrDefault(re => ndsCode.Equals(re.RomCode));
        }

        public void LoadPokemon()
        {
            var pokeNames = GetStrings(false, REntry.GetInt("PokemonNamesTextOffset")).ToArray();
            AllPokemons = new Pokemon[pokeNames.Length];

            for (var i = 0; i < AllPokemons.Count; i++)
            {
                var pokemon = new Pokemon(i);

                LoadBasicPokeStats(pokemon, _pokeNarc.Files[pokemon.Id]);
                pokemon.Name = pokeNames[i];
                AllPokemons[i] = pokemon;
            }

            PopulateEvolutions();
            ValidPokemons = AllPokemons.SliceFrom(1, Gen5Constants.PokemonCount);
        }

        public void LoadMoves()
        {
            var moveNames = GetStrings(false, REntry.GetInt("MoveNamesTextOffset"));

            AllMoves = new Move[moveNames.Count];

            for (var i = 0; i < AllMoves.Length; i++)
            {
                var moveData = _moveNarc.Files[i];
                AllMoves[i] = new Move
                {
                    Name = moveNames[i],
                    Number = i,
                    InternalId = i,
                    Hitratio = moveData[4] & 0xFF,
                    Power = moveData[3] & 0xFF,
                    Pp = moveData[5] & 0xFF,
                    Type = Gen5Constants.TypeTable[moveData[0] & 0xFF],
                    Category = Gen5Constants.MoveCategoryIndices[moveData[2] & 0xFF]
                };

                if (GlobalConstants.NormalMultihitMoves.Contains(i))
                    AllMoves[i].HitCount = 19 / 6.0;
                else if (GlobalConstants.DoubleHitMoves.Contains(i))
                    AllMoves[i].HitCount = 2;
                else if (i == GlobalConstants.TripleKickIndex)
                    AllMoves[i].HitCount = 2.71; // this assumes the first hit
            }


            ValidMoves = AllMoves.SliceFrom(1, Gen5Constants.MoveCount);
            FieldMoves = Gen5Constants.FieldMoves;
        }

        public void LoadBasicPokeStats(Pokemon pkmn, ArraySlice<byte> stats)
        {
            pkmn.Hp = stats[Gen5Constants.BsHpOffset] & 0xFF;
            pkmn.Attack = stats[Gen5Constants.BsAttackOffset] & 0xFF;
            pkmn.Defense = stats[Gen5Constants.BsDefenseOffset] & 0xFF;
            pkmn.Speed = stats[Gen5Constants.BsSpeedOffset] & 0xFF;
            pkmn.Spatk = stats[Gen5Constants.BsSpAtkOffset] & 0xFF;
            pkmn.Spdef = stats[Gen5Constants.BsSpDefOffset] & 0xFF;

            // Type
            pkmn.PrimaryType = Gen5Constants.TypeTable[stats[Gen5Constants.BsPrimaryTypeOffset] & 0xFF];
            pkmn.SecondaryType = Gen5Constants.TypeTable[stats[Gen5Constants.BsSecondaryTypeOffset] & 0xFF];

            // Only one type?
            if (pkmn.SecondaryType == pkmn.PrimaryType)
                pkmn.SecondaryType = null;

            pkmn.CatchRate = stats[Gen5Constants.BsCatchRateOffset] & 0xFF;
            pkmn.GrowthCurve = Exp.FromByte(stats[Gen5Constants.BsGrowthCurveOffset]);

            pkmn.Ability1 = stats[Gen5Constants.BsAbility1Offset] & 0xFF;
            pkmn.Ability2 = stats[Gen5Constants.BsAbility2Offset] & 0xFF;
            pkmn.Ability3 = stats[Gen5Constants.BsAbility3Offset] & 0xFF;

            // Held Items?
            var item1 = ReadWord(stats, Gen5Constants.BsCommonHeldItemOffset);
            var item2 = ReadWord(stats, Gen5Constants.BsRareHeldItemOffset);

            if (item1 == item2)
            {
                // guaranteed
                pkmn.GuaranteedHeldItem = item1;
                pkmn.CommonHeldItem = 0;
                pkmn.RareHeldItem = 0;
                pkmn.DarkGrassHeldItem = 0;
            }
            else
            {
                pkmn.GuaranteedHeldItem = 0;
                pkmn.CommonHeldItem = item1;
                pkmn.RareHeldItem = item2;
                pkmn.DarkGrassHeldItem = ReadWord(stats, Gen5Constants.BsDarkGrassHeldItemOffset);
            }
        }
        
        public override bool SaveRom(string filename)
        {
            WriteArm9(Arm9);

            SetStrings(false, REntry.GetInt("TrainerClassesTextOffset"), TrainerClassNames);

            WriteNarc(REntry.GetString("TextStrings"), _stringsNarc);
            WriteNarc(REntry.GetString("TextStory"), _storyTextNarc);
            WriteNarc(REntry.GetString("Scripts"), _scriptNarc);

            SaveTrainerNames();
            SaveIngameTrades();
            SaveFieldItems();
            SaveMoveTutorCompatibility();
            SaveMoveTutorMoves();
            SaveTmhmCompatibility();
            SaveTmMoves();
            SaveEncounters();
            SaveMovesLearnt();
            SaveHiddenHollow();
            SaveTrainers();
            SavePokemon();
            SaveMoves();

            BaseRom.SaveTo(filename);
            return true;
        }

        public void SaveMoves()
        {
            for (var i = 1; i <= Gen5Constants.MoveCount; i++)
            {
                var data = _moveNarc.Files[i];

                data[0] = Gen5Constants.TypeToByte(AllMoves[i].Type);
                data[2] = Gen5Constants.MoveCategoryToByte(AllMoves[i].Category);
                data[3] = (byte) AllMoves[i].Power;
                
                var hitratio = (int) Math.Round(AllMoves[i].Hitratio);
                if (hitratio < 0)
                    hitratio = 0;
                if (hitratio > 101)
                    hitratio = 100;

                data[4] = (byte) hitratio;
                data[5] = (byte) AllMoves[i].Pp;
            }

            WriteNarc(REntry.GetString("MoveData"), _moveNarc);
        }

        public void SavePokemon()
        {
            var nameList = GetStrings(false, REntry.GetInt("PokemonNamesTextOffset"));

            for (var i = 0; i < nameList.Count; i++)
            {
                SaveBasicPokeStats(AllPokemons[i], _pokeNarc.Files[i]);
                nameList[i] = AllPokemons[i].Name;
            }

            SetStrings(false, REntry.GetInt("PokemonNamesTextOffset"), nameList);
            WriteNarc(REntry.GetString("PokemonStats"), _pokeNarc);
            WriteEvolutions();
        }

        public void SaveBasicPokeStats(Pokemon pkmn, ArraySlice<byte> stats)
        {
            stats[Gen5Constants.BsHpOffset] = (byte) pkmn.Hp;
            stats[Gen5Constants.BsAttackOffset] = (byte) pkmn.Attack;
            stats[Gen5Constants.BsDefenseOffset] = (byte) pkmn.Defense;
            stats[Gen5Constants.BsSpeedOffset] = (byte) pkmn.Speed;
            stats[Gen5Constants.BsSpAtkOffset] = (byte) pkmn.Spatk;
            stats[Gen5Constants.BsSpDefOffset] = (byte) pkmn.Spdef;
            stats[Gen5Constants.BsPrimaryTypeOffset] = Gen5Constants.TypeToByte(pkmn.PrimaryType);

            if (pkmn.SecondaryType == null)
                stats[Gen5Constants.BsSecondaryTypeOffset] = stats[Gen5Constants.BsPrimaryTypeOffset];
            else
                stats[Gen5Constants.BsSecondaryTypeOffset] = Gen5Constants.TypeToByte(pkmn.SecondaryType);
            stats[Gen5Constants.BsCatchRateOffset] = (byte) pkmn.CatchRate;
            stats[Gen5Constants.BsGrowthCurveOffset] = pkmn.GrowthCurve.ToByte();

            stats[Gen5Constants.BsAbility1Offset] = (byte) pkmn.Ability1;
            stats[Gen5Constants.BsAbility2Offset] = (byte) pkmn.Ability2;
            stats[Gen5Constants.BsAbility3Offset] = (byte) pkmn.Ability3;

            // Held items
            if (pkmn.GuaranteedHeldItem > 0)
            {
                WriteWord(stats, Gen5Constants.BsCommonHeldItemOffset, pkmn.GuaranteedHeldItem);
                WriteWord(stats, Gen5Constants.BsRareHeldItemOffset, pkmn.GuaranteedHeldItem);
                WriteWord(stats, Gen5Constants.BsDarkGrassHeldItemOffset, 0);
            }
            else
            {
                WriteWord(stats, Gen5Constants.BsCommonHeldItemOffset, pkmn.CommonHeldItem);
                WriteWord(stats, Gen5Constants.BsRareHeldItemOffset, pkmn.RareHeldItem);
                WriteWord(stats, Gen5Constants.BsDarkGrassHeldItemOffset, pkmn.DarkGrassHeldItem);
            }
        }


        private void LoadStarters()
        {
            var scriptNarc = _scriptNarc;
            Starters = new Pokemon[3];

            for (var i = 0; i < 3; i++)
            {
                var thisStarter = REntry.OffsetArrayEntries["StarterOffsets" + (i + 1)];
                Starters[i] = AllPokemons[ReadWord(scriptNarc.Files[thisStarter[0].Entry], thisStarter[0].Offset)];
            }

        }

        public void ReplaceStarterFiles(
            NarcArchive starterNarc,
            NarcArchive pokespritesNarc,
            int starterIndex,
            int pokeNumber)
        {
            starterNarc.Files[starterIndex * 2] = pokespritesNarc.Files[pokeNumber * 20 + 18];
            // Get the picture...
            var compressedPic = pokespritesNarc.Files[pokeNumber * 20];
            // Decompress it with JavaDSDecmp
            var uncompressedPic = DsDecmp.Decompress(compressedPic);
            starterNarc.Files[12 + starterIndex] = uncompressedPic;
        }


        public void LoadEncounters()
        {
            LoadWildDictionaryNames();

            var encounterNarc = ReadNarc(REntry.GetString("WildPokemon"));
            var encounters = new List<EncounterSet>();
            var idx = -1;
            foreach (var entry in encounterNarc.Files)
            {
                idx++;
                if (entry.Length > Gen5Constants.PerSeasonEncounterDataLength)
                    for (var i = 0; i < 4; i++)
                        ProcessEncounterEntry(encounters, entry, i * Gen5Constants.PerSeasonEncounterDataLength, idx);
                else
                    ProcessEncounterEntry(encounters, entry, 0, idx);
            }

            Encounters = encounters.Slice();


            void ProcessEncounterEntry(ICollection<EncounterSet> encounts, ArraySlice<byte> entry, int startOffset, int id)
            {
                if (!_wildDictionaryNames.ContainsKey(id))
                    _wildDictionaryNames[id] = "? Unknown ?";

                var mapName = _wildDictionaryNames[id];

                var amounts = Gen5Constants.EncountersOfEachType;

                var off = 8;
                for (var i = 0; i < 7; i++)
                {
                    var rate = entry[startOffset + i] & 0xFF;
                    if (rate != 0)
                    {
                        var encs = ReadEncounters(entry, startOffset + off, amounts[i]);

                        var area = new EncounterSet
                        {
                            Rate = rate,
                            Encounters = encs.ToArray(),
                            Offset = id,
                            DisplayName = mapName + " " + Gen5Constants.EncounterTypeNames[i]
                        };

                        encounts.Add(area);
                    }
                    off += amounts[i] * 4;
                }


                List<Encounter> ReadEncounters(ArraySlice<byte> data, int offset, int number)
                {
                    var encs = new List<Encounter>();
                    for (var i = 0; i < number; i++)
                    {
                        var enc1 = new Encounter
                        {
                            Pokemon = AllPokemons[(data[offset + i * 4] & 0xFF) +
                                                  ((data[offset + 1 + i * 4] & 0x03) << 8)],
                            Level = data[offset + 2 + i * 4] & 0xFF,
                            MaxLevel = data[offset + 3 + i * 4] & 0xFF
                        };

                        encs.Add(enc1);
                    }
                    return encs;
                }
            }
        }


        public void SaveEncounters()
        {
            var encounterNarc = ReadNarc(REntry.GetString("WildPokemon"));
            IEnumerator encounters = Encounters.GetEnumerator();
            foreach (var entry in encounterNarc.Files)
            {
                WriteEncounterEntry(encounters, entry, 0);

                if (entry.Length <= 232)
                    continue;

                for (var i = 1; i < 4; i++)
                    WriteEncounterEntry(encounters, entry, i * 232);
            }

            // Save
            WriteNarc(REntry.GetString("WildPokemon"), encounterNarc);

            // Habitat List / Area Data?
            if (REntry.RomType == Gen5Constants.TypeBw2)
            {
                var areaNarc = ReadNarc(REntry.GetString("PokemonAreaData"));
                var newFiles = new List<ArraySlice<byte>>();
                for (var i = 0; i < Gen5Constants.PokemonCount; i++)
                {
                    var nf = new byte[Gen5Constants.Bw2AreaDataEntryLength];
                    nf[0] = 1;
                    newFiles.Add(nf);
                }
                // Get data now
                for (var i = 0; i < encounterNarc.Files.Count; i++)
                {
                    var encEntry = encounterNarc.Files[i];
                    if (encEntry.Length > Gen5Constants.PerSeasonEncounterDataLength)
                        for (var s = 0; s < 4; s++)
                            ParseAreaData(encEntry, s * Gen5Constants.PerSeasonEncounterDataLength, newFiles, s, i);
                    else
                        for (var s = 0; s < 4; s++)
                            ParseAreaData(encEntry, 0, newFiles, s, i);
                }
                // Now update unobtainables & save
                for (var i = 0; i < Gen5Constants.PokemonCount; i++)
                {
                    var file = newFiles[i];
                    for (var s = 0; s < 4; s++)
                    {
                        var unobtainable = true;
                        for (var e = 0; e < Gen5Constants.Bw2EncounterAreaCount; e++)
                            if (file[s * (Gen5Constants.Bw2EncounterAreaCount + 1) + e + 2] != 0)
                            {
                                unobtainable = false;
                                break;
                            }
                        if (unobtainable)
                            file[s * (Gen5Constants.Bw2EncounterAreaCount + 1) + 1] = 1;
                    }
                    areaNarc.Files[i] = file;
                }
                // Save
                WriteNarc(REntry.GetString("PokemonAreaData"), areaNarc);
            }
        }

        public void ParseAreaData(ArraySlice<byte> entry, int startOffset, List<ArraySlice<byte>> areaData, int season, int fileNumber)
        {
            var amounts = Gen5Constants.EncountersOfEachType;

            var offset = 8;
            for (var i = 0; i < 7; i++)
            {
                var rate = entry[startOffset + i] & 0xFF;
                if (rate != 0)
                    for (var e = 0; e < amounts[i]; e++)
                    {
                        var pkmn = AllPokemons[(entry[startOffset + offset + e * 4] & 0xFF) +
                                               ((entry[startOffset + offset + 1 + e * 4] & 0x03) <<
                                                8)];
                        var pokeFile = areaData[pkmn.Id - 1];
                        var areaIndex = Gen5Constants.WildFileToAreaMap[fileNumber];
                        // Route 4?
                        if (areaIndex == Gen5Constants.Bw2Route4AreaIndex)
                            if (fileNumber == Gen5Constants.B2Route4EncounterFile && REntry.RomCode[2] == 'D' ||
                                fileNumber == Gen5Constants.W2Route4EncounterFile && REntry.RomCode[2] == 'E')
                                areaIndex = -1; // wrong version
                        // Victory Road?
                        if (areaIndex == Gen5Constants.Bw2VictoryRoadAreaIndex)
                            if (REntry.RomCode[2] == 'D')
                            {
                                // White 2
                                if (fileNumber == Gen5Constants.B2VrExclusiveRoom1 ||
                                    fileNumber == Gen5Constants.B2VrExclusiveRoom2)
                                    areaIndex = -1; // wrong version
                            }
                            else
                            {
                                // Black 2
                                if (fileNumber == Gen5Constants.W2VrExclusiveRoom1 ||
                                    fileNumber == Gen5Constants.W2VrExclusiveRoom2)
                                    areaIndex = -1; // wrong version
                            }
                        // Reversal Mountain?
                        if (areaIndex == Gen5Constants.Bw2ReversalMountainAreaIndex)
                            if (REntry.RomCode[2] == 'D')
                            {
                                // White 2
                                if (fileNumber >= Gen5Constants.B2ReversalMountainStart &&
                                    fileNumber <= Gen5Constants.B2ReversalMountainEnd)
                                    areaIndex = -1; // wrong version
                            }
                            else
                            {
                                // Black 2
                                if (fileNumber >= Gen5Constants.W2ReversalMountainStart &&
                                    fileNumber <= Gen5Constants.W2ReversalMountainEnd)
                                    areaIndex = -1; // wrong version
                            }
                        // Skip stuff that isn't on the map or is wrong version
                        if (areaIndex != -1)
                            pokeFile[season * (Gen5Constants.Bw2EncounterAreaCount + 1) + 2 + areaIndex] |=
                                (byte) (1 << i);
                    }
                offset += amounts[i] * 4;
            }
        }


        public void AddHabitats(ArraySlice<byte> entry, int startOffset, Dictionary<Pokemon, ArraySlice<byte>> pokemonHere, int season)
        {
            var amounts = Gen5Constants.EncountersOfEachType;
            var type = Gen5Constants.HabitatClassificationOfEachType;

            var offset = 8;
            for (var i = 0; i < 7; i++)
            {
                var rate = entry[startOffset + i] & 0xFF;
                if (rate != 0)
                    for (var e = 0; e < amounts[i]; e++)
                    {
                        var pkmn = AllPokemons[(entry[startOffset + offset + e * 4] & 0xFF) +
                                               ((entry[startOffset + offset + 1 + e * 4] & 0x03) <<
                                                8)];
                        if (pokemonHere.ContainsKey(pkmn))
                        {
                            pokemonHere[pkmn][type[i] + season * 3] = 1;
                        }
                        else
                        {
                            var locs = new byte[12];
                            locs[type[i] + season * 3] = 1;
                            pokemonHere[pkmn] = locs;
                        }
                    }
                offset += amounts[i] * 4;
            }
        }

        public void WriteEncounterEntry(IEnumerator encounters, ArraySlice<byte> entry, int startOffset)
        {
            var amounts = Gen5Constants.EncountersOfEachType;

            var offset = 8;
            for (var i = 0; i < 7; i++)
            {
                var rate = entry[startOffset + i] & 0xFF;
                if (rate != 0)
                {
                    encounters.MoveNext();
                    var area = (EncounterSet) encounters.Current;
                    for (var j = 0; j < amounts[i]; j++)
                    {
                        var enc = area.Encounters[j];
                        WriteWord(entry, startOffset + offset + j * 4, enc.Pokemon.Id);
                        entry[startOffset + offset + j * 4 + 2] = (byte) enc.Level;
                        entry[startOffset + offset + j * 4 + 3] = (byte) enc.MaxLevel;
                    }
                }
                offset += amounts[i] * 4;
            }
        }

        public void LoadWildDictionaryNames()
        {
            var mapHeaderData = ReadNarc(REntry.GetString("MapTableFile")).Files[0];
            var numDictionaryHeaders = mapHeaderData.Length / 48;
            var allDictionaryNames = GetStrings(false, REntry.GetInt("MapNamesTextOffset"));
            for (var map = 0; map < numDictionaryHeaders; map++)
            {
                var baseOffset = map * 48;
                var mapNameIndex = mapHeaderData[baseOffset + 26] & 0xFF;
                var mapName = allDictionaryNames[mapNameIndex];
                if (REntry.RomType == Gen5Constants.TypeBw2)
                {
                    var wildSet = mapHeaderData[baseOffset + 20] & 0xFF;
                    if (wildSet != 255)
                        _wildDictionaryNames[wildSet] = mapName;
                }
                else
                {
                    var wildSet = ReadWord(mapHeaderData, baseOffset + 20);
                    if (wildSet != 65535)
                        _wildDictionaryNames[wildSet] = mapName;
                }
            }
        }


        public void LoadTrainers()
        {
            var allTrainers = new List<Trainer>();

            var trainers = ReadNarc(REntry.GetString("TrainerData"));
            var trpokes = ReadNarc(REntry.GetString("TrainerPokemon"));
            var trainernum = trainers.Files.Count;
            var tclasses = TrainerClassNames;
            var tnames = TrainerNames;

            for (var i = 1; i < trainernum; i++)
            {
                var trainer = trainers.Files[i];
                var trpoke = trpokes.Files[i];
                var tr = new Trainer
                {
                    Poketype = trainer[0] & 0xFF,
                    Offset = i,
                    Trainerclass = trainer[1] & 0xFF
                };
                var numPokes = trainer[3] & 0xFF;
                var pokeOffs = 0;
                tr.FullDisplayName = tclasses[tr.Trainerclass] + " " + tnames[i - 1];
                // printBA(trpoke);
                for (var poke = 0; poke < numPokes; poke++)
                {
                    // Structure is
                    // AI SB LV LV SP SP FRM FRM
                    // (HI HI)
                    // (M1 M1 M2 M2 M3 M3 M4 M4)
                    // where SB = 0 0 Ab Ab 0 0 Fm Ml
                    // Ab Ab = ability number, 0 for random
                    // Fm = 1 for forced female
                    // Ml = 1 for forced male
                    // There's also a trainer flag to force gender, but
                    // this allows fixed teams with mixed genders.

                    var ailevel = trpoke[pokeOffs] & 0xFF;
                    // int secondbyte = trpoke[pokeOffs + 1] & 0xFF;
                    var level = ReadWord(trpoke, pokeOffs + 2);
                    var species = ReadWord(trpoke, pokeOffs + 4);
                    // int formnum = readWord(trpoke, pokeOffs + 6);
                    var tpk = new TrainerPokemon
                    {
                        Level = level,
                        Pokemon = AllPokemons[species],
                        AiLevel = ailevel,
                        Ability = trpoke[pokeOffs + 1] & 0xFF
                    };
                    pokeOffs += 8;
                    if ((tr.Poketype & 2) == 2)
                    {
                        var heldItem = ReadWord(trpoke, pokeOffs);
                        tpk.HeldItem = heldItem;
                        pokeOffs += 2;
                    }
                    if ((tr.Poketype & 1) == 1)
                    {
                        var attack1 = ReadWord(trpoke, pokeOffs);
                        var attack2 = ReadWord(trpoke, pokeOffs + 2);
                        var attack3 = ReadWord(trpoke, pokeOffs + 4);
                        var attack4 = ReadWord(trpoke, pokeOffs + 6);
                        tpk.Move1 = attack1;
                        tpk.Move2 = attack2;
                        tpk.Move3 = attack3;
                        tpk.Move4 = attack4;
                        pokeOffs += 8;
                    }
                    tr.Pokemon.Add(tpk);
                }
                allTrainers.Add(tr);
            }

            if (REntry.RomType == Gen5Constants.TypeBw)
            {
                Trainers = allTrainers.Slice();
                Gen5Constants.TagTrainersBw(Trainers);
            }
            else
            {
                    for (var trno = 0; trno < 2; trno++)
                    {
                        var tr = new Trainer
                        {
                            Poketype = 3,
                            Offset = 0
                        };
                        for (var poke = 0; poke < 3; poke++)
                        {
                            var pkmndata = _driftveil.Files[trno * 3 + poke + 1];
                            var tpk = new TrainerPokemon
                            {
                                Level = 25,
                                Pokemon = AllPokemons[ReadWord(pkmndata, 0)],
                                AiLevel = 255,
                                HeldItem = ReadWord(pkmndata, 12),
                                Move1 = ReadWord(pkmndata, 2),
                                Move2 = ReadWord(pkmndata, 4),
                                Move3 = ReadWord(pkmndata, 6),
                                Move4 = ReadWord(pkmndata, 8)
                            };
                            tr.Pokemon.Add(tpk);
                        }
                        allTrainers.Add(tr);
                }
                Trainers = allTrainers.Slice();
                Gen5Constants.TagTrainersBw2(Trainers);
            }
        }


        public void SaveTrainers()
        {
            using (var allTrainers = Trainers.GetEnumerator())
            {
                var trainers = ReadNarc(REntry.GetString("TrainerData"));
                var trpokes = new NarcArchive();
                // Get current movesets in case we need to reset them for certain
                // trainer mons.
                // empty entry
                trpokes.Files.Add(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 });
                var trainernum = trainers.Files.Count;
                for (var i = 1; i < trainernum; i++)
                {
                    var trainer = trainers.Files[i];
                    allTrainers.MoveNext();
                    var tr = allTrainers.Current;
                    // preserve original poketype for held item & moves
                    trainer[0] = (byte)tr.Poketype;
                    var numPokes = tr.Pokemon.Count;
                    trainer[3] = (byte)numPokes;

                    var bytesNeeded = 8 * numPokes;
                    if ((tr.Poketype & 1) == 1)
                        bytesNeeded += 8 * numPokes;
                    if ((tr.Poketype & 2) == 2)
                        bytesNeeded += 2 * numPokes;
                    var trpoke = new byte[bytesNeeded];
                    var pokeOffs = 0;

                    using (var tpokes = tr.Pokemon.GetEnumerator())
                    {
                        for (var poke = 0; poke < numPokes; poke++)
                        {
                            tpokes.MoveNext();
                            var tp = tpokes.Current;
                            trpoke[pokeOffs] = (byte)tp.AiLevel;
                            // no gender or ability info, so no byte 1
                            WriteWord(trpoke, pokeOffs + 2, tp.Level);
                            WriteWord(trpoke, pokeOffs + 4, tp.Pokemon.Id);
                            // no form info, so no byte 6/7
                            pokeOffs += 8;
                            if ((tr.Poketype & 2) == 2)
                            {
                                WriteWord(trpoke, pokeOffs, tp.HeldItem);
                                pokeOffs += 2;
                            }
                            if ((tr.Poketype & 1) == 1)
                            {
                                if (tp.ResetMoves)
                                {
                                    var pokeMoves = RomFunctions.GetMovesAtLevel(tp.Pokemon, MovesLearnt, tp.Level);
                                    for (var m = 0; m < 4; m++)
                                        WriteWord(trpoke, pokeOffs + m * 2, pokeMoves[m]);
                                }
                                else
                                {
                                    WriteWord(trpoke, pokeOffs, tp.Move1);
                                    WriteWord(trpoke, pokeOffs + 2, tp.Move2);
                                    WriteWord(trpoke, pokeOffs + 4, tp.Move3);
                                    WriteWord(trpoke, pokeOffs + 6, tp.Move4);
                                }
                                pokeOffs += 8;
                            }
                        }
                    }
                    
                    
                    trpokes.Files.Add(trpoke);
                }
                WriteNarc(REntry.GetString("TrainerData"), trainers);
                WriteNarc(REntry.GetString("TrainerPokemon"), trpokes);
                // Deal with PWT
                if (REntry.RomType != Gen5Constants.TypeBw2 || REntry.GetString("DriftveilPokemon").IsEmpty())
                    return;
                
                for (var trno = 0; trno < 2; trno++)
                {
                    allTrainers.MoveNext();
                    var tr = allTrainers.Current;

                    using (var tpks = tr.Pokemon.GetEnumerator())
                    {
                        for (var poke = 0; poke < 3; poke++)
                        {
                            var pkmndata = _driftveil.Files[trno * 3 + poke + 1];
                            tpks.MoveNext();
                            var tp = tpks.Current;
                            // pokemon and held item
                            WriteWord(pkmndata, 0, tp.Pokemon.Id);
                            WriteWord(pkmndata, 12, tp.HeldItem);
                            // handle moves
                            if (tp.ResetMoves)
                            {
                                var pokeMoves = RomFunctions.GetMovesAtLevel(tp.Pokemon, MovesLearnt, tp.Level);
                                for (var m = 0; m < 4; m++)
                                    WriteWord(pkmndata, 2 + m * 2, pokeMoves[m]);
                            }
                            else
                            {
                                WriteWord(pkmndata, 2, tp.Move1);
                                WriteWord(pkmndata, 4, tp.Move2);
                                WriteWord(pkmndata, 6, tp.Move3);
                                WriteWord(pkmndata, 8, tp.Move4);
                            }
                        }
                    }
                        
                        
                }
                WriteNarc(REntry.GetString("DriftveilPokemon"), _driftveil);
            }
        }


        private void LoadStaticPokemon()
        {
            if (!REntry.StaticPokemonSupport)
            {
                StaticPokemon = Slice<Pokemon>.Empty;
                return;
            }

            StaticPokemon = new Pokemon[REntry.StaticPokemon.Count];

            for (var i = 0; i < StaticPokemon.Length; i++)
            {
                var staticPokemon = REntry.StaticPokemon[i];
                StaticPokemon[i] = staticPokemon.GetPokemon(this, _scriptNarc);
            }
        }

        private void LoadMiscTweaksAvailable()
        {
            var available = 0;
            if (REntry.RomType == Gen5Constants.TypeBw2)
                available |= MiscTweak.RandomizeHiddenHollows.Value;
            if (REntry.TweakFiles["FastestTextTweak"] != null)
                available |= MiscTweak.FastestText.Value;
            available |= MiscTweak.BanLuckyEgg.Value;

            MiscTweaksAvailable = available;
        }

        public bool GenericIpsPatch(ArraySlice<byte> data, string ctName)
        {
            var patchName = REntry.TweakFiles[ctName];
            if (patchName == null)
                return false;

            FileFunctions.ApplyPatch(data, (ArraySlice<byte>) Resources.ResourceManager.GetObject(patchName));
            return true;
        }


        private void LoadTmMoves()
        {
            var tmDataPrefix = Gen5Constants.TmDataPrefix;
            var offset = Find(Arm9, tmDataPrefix);

            if (offset <= 0)
                return;

            offset += Gen5Constants.TmDataPrefix.Length / 2; // because it was
            // a prefix
            TmMoves = new int[Gen5Constants.TmCount];

            for (var i = 0; i < Gen5Constants.TmBlockOneCount; i++)
                TmMoves[i] = ReadWord(Arm9, offset + i * 2);

            // Skip past first 92 TMs and 6 HMs
            offset += (Gen5Constants.TmBlockOneCount + Gen5Constants.HmCount) * 2;

            for (var i = 0; i < Gen5Constants.TmCount - Gen5Constants.TmBlockOneCount; i++)
                TmMoves[i + Gen5Constants.TmBlockOneCount] = ReadWord(Arm9, offset + i * 2);

            
            RequiredFieldTMs = REntry.RomType == Gen5Constants.TypeBw ? Gen5Constants.Bw1RequiredFieldTMs : Gen5Constants.Bw2RequiredFieldTMs;
        }

        private void SaveTmMoves()
        {
            var tmDataPrefix = Gen5Constants.TmDataPrefix;
            var offset = Find(Arm9, tmDataPrefix);

            if (offset <= 0)
                return;

            offset += Gen5Constants.TmDataPrefix.Length / 2; // because it was
            // a prefix
            for (var i = 0; i < Gen5Constants.TmBlockOneCount; i++)
                WriteWord(Arm9, offset + i * 2, TmMoves[i]);
            // Skip past those 92 TMs and 6 HMs
            offset += (Gen5Constants.TmBlockOneCount + Gen5Constants.HmCount) * 2;
            for (var i = 0; i < Gen5Constants.TmCount - Gen5Constants.TmBlockOneCount; i++)
                WriteWord(Arm9, offset + i * 2, TmMoves[i + Gen5Constants.TmBlockOneCount]);

            // Update TM item descriptions
            var itemDescriptions = GetStrings(false, REntry.GetInt("ItemDescriptionsTextOffset"));
            var moveDescriptions = GetStrings(false, REntry.GetInt("MoveDescriptionsTextOffset"));
            // TM01 is item 328 and so on
            for (var i = 0; i < Gen5Constants.TmBlockOneCount; i++)
                itemDescriptions[i + Gen5Constants.TmBlockOneOffset] = moveDescriptions[TmMoves[i]];
            // TM93-95 are 618-620
            for (var i = 0; i < Gen5Constants.TmCount - Gen5Constants.TmBlockOneCount; i++)
                itemDescriptions[i + Gen5Constants.TmBlockTwoOffset] =
                    moveDescriptions[TmMoves[i + Gen5Constants.TmBlockOneCount]];
            // Save the new item descriptions
            SetStrings(false, REntry.GetInt("ItemDescriptionsTextOffset"), itemDescriptions);
            // Palettes
            var baseOfPalettes = REntry.RomType == Gen5Constants.TypeBw
                ? Gen5Constants.Bw1ItemPalettesPrefix
                : Gen5Constants.Bw2ItemPalettesPrefix;
            var offsPals = Find(Arm9, baseOfPalettes);
            if (offsPals > 0)
            {
                // Write pals
                for (var i = 0; i < Gen5Constants.TmBlockOneCount; i++)
                {
                    var itmNum = Gen5Constants.TmBlockOneOffset + i;
                    var m = AllMoves[TmMoves[i]];
                    var pal = TypeTmPaletteNumber(m.Type);
                    WriteWord(Arm9, offsPals + itmNum * 4 + 2, pal);
                }
                for (var i = 0; i < Gen5Constants.TmCount - Gen5Constants.TmBlockOneCount; i++)
                {
                    var itmNum = Gen5Constants.TmBlockTwoOffset + i;
                    var m = AllMoves[TmMoves[i + Gen5Constants.TmBlockOneCount]];
                    var pal = TypeTmPaletteNumber(m.Type);
                    WriteWord(Arm9, offsPals + itmNum * 4 + 2, pal);
                }
            }
        }

        private void LoadHmMoves()
        {
            var offset = Find(Arm9, Gen5Constants.TmDataPrefix);

            if (offset <= 0)
                return;

            offset += Gen5Constants.TmDataPrefix.Length / 2; // because it was a prefix
            offset += Gen5Constants.TmBlockOneCount * 2; // TM data

            HmMoves = new int[Gen5Constants.HmCount];

            for (var i = 0; i < Gen5Constants.HmCount; i++)
                HmMoves[i] =  ReadWord(Arm9, offset + i * 2);

            EarlyRequiredHmMoves = REntry.RomType == Gen5Constants.TypeBw2 ? Gen5Constants.Bw2EarlyRequiredHmMoves : Gen5Constants.Bw1EarlyRequiredHmMoves;

        }

        private void LoadTmhmCompatibility()
        {
            var compat = new Dictionary<Pokemon, ArraySlice<bool>>();
            for (var i = 1; i <= Gen5Constants.PokemonCount; i++)
            {
                var data = _pokeNarc.Files[i];
                var pkmn = AllPokemons[i];
                var flags = new bool[Gen5Constants.TmCount + Gen5Constants.HmCount + 1];
                for (var j = 0; j < 13; j++)
                    ReadByteIntoFlags(data, flags, j * 8 + 1, Gen5Constants.BsTmhmCompatOffset + j);
                compat[pkmn] = flags;
            }

            TmhmCompatibility = compat;
        }

        private void SaveTmhmCompatibility()
        {
            foreach (var compatEntry in TmhmCompatibility)
            {
                var pkmn = compatEntry.Key;
                var flags = compatEntry.Value;
                var data = _pokeNarc.Files[pkmn.Id];
                for (var j = 0; j < 13; j++)
                    data[Gen5Constants.BsTmhmCompatOffset + j] = GetByteFromFlags(flags, j * 8 + 1);
            }
        }


        private void LoadMoveTutorMoves()
        {
            if (!HasMoveTutors)
            {
                MoveTutorMoves = Slice<int>.Empty;
                return;
            }

            var baseOffset = REntry.GetInt("MoveTutorDataOffset");

            MoveTutorMoves = new int[Gen5Constants.Bw2MoveTutorCount];

            for (var i = 0; i < MoveTutorMoves.Length; i++)
                MoveTutorMoves[i] = ReadWord(_mtFile, baseOffset + i * Gen5Constants.Bw2MoveTutorBytesPerEntry);
        }

        private void SaveMoveTutorMoves()
        {
            if (!HasMoveTutors)
                return;
            var baseOffset = REntry.GetInt("MoveTutorDataOffset");
            var amount = Gen5Constants.Bw2MoveTutorCount;
            var bytesPer = Gen5Constants.Bw2MoveTutorBytesPerEntry;
            if (MoveTutorMoves.Count != amount)
                return;

            for (var i = 0; i < amount; i++)
                WriteWord(_mtFile, baseOffset + i * bytesPer, MoveTutorMoves[i]);

            WriteOverlay(REntry.GetInt("MoveTutorOvlNumber"), _mtFile);
        }


        private void LoadMoveTutorCompatibility()
        {
            if (!HasMoveTutors)
            {
                MoveTutorCompatibility = new Dictionary<Pokemon, ArraySlice<bool>>();
                return;
            }

            var compat = new Dictionary<Pokemon, ArraySlice<bool>>();
            ArraySlice<int> countsPersonalOrder = new[] { 15, 17, 13, 15 };
            ArraySlice<int> countsMoveOrder = new[] { 13, 15, 15, 17 };
            ArraySlice<int> personalToMoveOrder = new[] { 1, 3, 0, 2 };
            for (var i = 1; i <= Gen5Constants.PokemonCount; i++)
            {
                var data = _pokeNarc.Files[i];
                var pkmn = AllPokemons[i];
                var flags = new bool[Gen5Constants.Bw2MoveTutorCount + 1];
                for (var mt = 0; mt < 4; mt++)
                {
                    var mtflags = new bool[countsPersonalOrder[mt] + 1];
                    for (var j = 0; j < 4; j++)
                        ReadByteIntoFlags(data, mtflags, j * 8 + 1, Gen5Constants.BsMtCompatOffset + mt * 4 + j);
                    var offsetOfThisData = 0;
                    for (var cmoIndex = 0; cmoIndex < personalToMoveOrder[mt]; cmoIndex++)
                        offsetOfThisData += countsMoveOrder[cmoIndex];
                    Array.Copy(mtflags, 1, flags, offsetOfThisData + 1, countsPersonalOrder[mt]);
                }
                compat[pkmn] = flags;
            }

            MoveTutorCompatibility = compat;
            HasMoveTutors = REntry.RomType == Gen5Constants.TypeBw2;
        }

        private void SaveMoveTutorCompatibility()
        {
            if (!HasMoveTutors)
                return;
            // BW2 move tutor flags aren't using the same order as the move tutor
            // move data.
            // We unscramble them from move data order to personal.narc flag order.
            ArraySlice<int> countsPersonalOrder = new[] { 15, 17, 13, 15 };
            ArraySlice<int> countsMoveOrder = new[] { 13, 15, 15, 17 };
            ArraySlice<int> personalToMoveOrder = new[] { 1, 3, 0, 2 };
            foreach (var compatEntry in MoveTutorCompatibility)
            {
                var pkmn = compatEntry.Key;
                var flags = compatEntry.Value;
                var data = _pokeNarc.Files[pkmn.Id];
                for (var mt = 0; mt < 4; mt++)
                {
                    var offsetOfThisData = 0;
                    for (var cmoIndex = 0; cmoIndex < personalToMoveOrder[mt]; cmoIndex++)
                        offsetOfThisData += countsMoveOrder[cmoIndex];
                    var mtflags = new bool[countsPersonalOrder[mt] + 1];
                    Array.Copy(flags, offsetOfThisData + 1, mtflags, 1, countsPersonalOrder[mt]);
                    for (var j = 0; j < 4; j++)
                        data[Gen5Constants.BsMtCompatOffset + mt * 4 + j] = GetByteFromFlags(mtflags, j * 8 + 1);
                }
            }
        }

        public int Find(ArraySlice<byte> data, string hexString)
        {
            if (hexString.Length % 2 != 0)
                return -3; // error
            var searchFor = new byte[hexString.Length / 2];
            for (var i = 0; i < searchFor.Length; i++)
                searchFor[i] = (byte) Convert.ToInt32(hexString.Substring(i * 2, 2), 16);
            var found = RomFunctions.Search(data, searchFor);
            if (found.Count == 0)
                return -1; // not found
            if (found.Count > 1)
                return -2; // not unique
            return found[0];
        }

        public ArraySlice<string> GetStrings(bool isStoryText, int index)
        {
            var baseNarc = isStoryText ? _storyTextNarc : _stringsNarc;
            var rawFile = baseNarc.Files[index];
            return PpTxtHandler.ReadTexts(rawFile);
        }

        public void SetStrings(bool isStoryText, int index, ArraySlice<string> strings)
        {
            var baseNarc = isStoryText ? _storyTextNarc : _stringsNarc;
            var oldRawFile = baseNarc.Files[index];
            var newRawFile = PpTxtHandler.SaveEntry(oldRawFile, strings);
            baseNarc.Files[index] = newRawFile;
        }

        public void PopulateEvolutions()
        {
            foreach (var pkmn in AllPokemons)
                if (pkmn != null)
                {
                    pkmn.EvolutionsFrom.Clear();
                    pkmn.EvolutionsTo.Clear();
                }
            
            for (var i = 1; i <= Gen5Constants.PokemonCount; i++)
            {
                var pk = AllPokemons[i];
                var evoEntry = _evoNarc.Files[i];
                for (var evo = 0; evo < 7; evo++)
                {
                    var method = ReadWord(evoEntry, evo * 6);
                    var species = ReadWord(evoEntry, evo * 6 + 4);
                    if (method >= 1 &&
                        method <= Gen5Constants.EvolutionMethodCount &&
                        species >= 1)
                    {
                        var et = EvolutionType.FromIndex(5, method);
                        var extraInfo = ReadWord(evoEntry, evo * 6 + 2);
                        var evol = new Evolution(pk, AllPokemons[species], true, et, extraInfo);
                        if (!pk.EvolutionsFrom.Contains(evol))
                        {
                            pk.EvolutionsFrom.Add(evol);
                            AllPokemons[species].EvolutionsTo.Add(evol);
                        }
                    }
                }
                // split evos don't carry stats
                if (pk.EvolutionsFrom.Count > 1)
                    foreach (var e in pk.EvolutionsFrom)
                        e.CarryStats = false;
            }
        }

        public void WriteEvolutions()
        {
            for (var i = 1; i <= Gen5Constants.PokemonCount; i++)
            {
                var evoEntry = _evoNarc.Files[i];
                var pk = AllPokemons[i];
                var evosWritten = 0;
                foreach (var evo in pk.EvolutionsFrom)
                {
                    WriteWord(evoEntry, evosWritten * 6, evo.Type.ToIndex(5));
                    WriteWord(evoEntry, evosWritten * 6 + 2, evo.ExtraInfo);
                    WriteWord(evoEntry, evosWritten * 6 + 4, evo.To.Id);
                    evosWritten++;
                    if (evosWritten == 7)
                        break;
                }
                while (evosWritten < 7)
                {
                    WriteWord(evoEntry, evosWritten * 6, 0);
                    WriteWord(evoEntry, evosWritten * 6 + 2, 0);
                    WriteWord(evoEntry, evosWritten * 6 + 4, 0);
                    evosWritten++;
                }
            }
            WriteNarc(REntry.GetString("PokemonEvolutions"), _evoNarc);
        }


        private void LoadTrainerNames()
        {
            // blank one
            var tnames = GetStrings(false, REntry.GetInt("TrainerNamesTextOffset")).SliceFrom(1);

            // Tack the mugshot names on the end

            TrainerNames = 
                tnames.Concat(
                    _mnames.Where(mname => !mname.IsEmpty() && mname[0] >= 'A' && mname[0] <= 'Z')
                ).Slice();
        }

        private void SaveTrainerNames()
        {
            // Grab the mugshot names off the back of the list of trainer names
            // we got back
            var trNamesSize = TrainerNames.Count;
            for (var i = _mnames.Count - 1; i >= 0; i--)
            {
                var origMName = _mnames[i];

                if (origMName.IsEmpty() || origMName[0] < 'A' || origMName[0] > 'Z')
                    continue;

                // Grab replacement
                var replacement = TrainerNames[--trNamesSize];
                _mnames[i] = replacement;
            }
            // Save back mugshot names
            SetStrings(false, REntry.GetInt("TrainerMugshotsTextOffset"), _mnames);

            // Now save the rest of trainer names
            var tnames = GetStrings(false, REntry.GetInt("TrainerNamesTextOffset"));

            for (var i = 1; i < tnames.Count; i++)
                tnames[i] = TrainerNames[i - 1];

            SetStrings(false, REntry.GetInt("TrainerNamesTextOffset"), tnames);
        }


        private void LoadDoublesTrainerClasses()
        {
            var doublesClasses = REntry.ArrayEntries["DoublesTrainerClasses"];

            DoublesTrainerClasses = new int[doublesClasses.Count];

            for (var i = 0; i < DoublesTrainerClasses.Count; i++)
                DoublesTrainerClasses[i] = doublesClasses[i];
        }


        public override int InternalStringLength(string str)
        {
            var offs = 0;
            var len = str.Length;
            while (str.IndexOf("\\x", offs, StringComparison.Ordinal) != -1)
            {
                len -= 5;
                offs = str.IndexOf("\\x", offs, StringComparison.Ordinal) + 1;
            }

            return len;
        }

        private void LoadFiledItems()
        {
            var fieldItems = new List<int>();
            // normal items
            var scriptFileNormal = REntry.GetInt("ItemBallsScriptOffset");
            var scriptFileHidden = REntry.GetInt("HiddenItemsScriptOffset");
            var skipTable = REntry.ArrayEntries["ItemBallsSkip"];
            var skipTableH = REntry.ArrayEntries["HiddenItemsSkip"];
            var setVarNormal = Gen5Constants.NormalItemSetVarCommand;
            var setVarHidden = Gen5Constants.HiddenItemSetVarCommand;
            var itemScripts = _scriptNarc.Files[scriptFileNormal];
            var offset = 0;
            var skipTableOffset = 0;
            while (true)
            {
                var part1 = ReadWord(itemScripts, offset);
                if (part1 == Gen5Constants.ScriptListTerminator)
                    break;
                var offsetInFile = ReadRelativePointer(itemScripts, offset);
                offset += 4;
                if (offsetInFile > itemScripts.Length)
                    break;
                if (skipTableOffset < skipTable.Count &&
                    skipTable[skipTableOffset] == offset / 4 - 1)
                {
                    skipTableOffset++;
                    continue;
                }
                var command = ReadWord(itemScripts, offsetInFile + 2);
                var variable = ReadWord(itemScripts, offsetInFile + 4);
                if (command == setVarNormal &&
                    variable == Gen5Constants.NormalItemVarSet)
                {
                    var item = ReadWord(itemScripts, offsetInFile + 6);
                    fieldItems.Add(item);
                }
            }

            // hidden items
            var hitemScripts = _scriptNarc.Files[scriptFileHidden];
            offset = 0;
            skipTableOffset = 0;
            while (true)
            {
                var part1 = ReadWord(hitemScripts, offset);
                if (part1 == Gen5Constants.ScriptListTerminator)
                    break;
                var offsetInFile = ReadRelativePointer(hitemScripts, offset);
                if (offsetInFile > hitemScripts.Length)
                    break;
                offset += 4;
                if (skipTableOffset < skipTable.Count &&
                    skipTableH[skipTableOffset] == offset / 4 - 1)
                {
                    skipTableOffset++;
                    continue;
                }
                var command = ReadWord(hitemScripts, offsetInFile + 2);
                var variable = ReadWord(hitemScripts, offsetInFile + 4);
                if (command == setVarHidden &&
                    variable == Gen5Constants.HiddenItemVarSet)
                {
                    var item = ReadWord(hitemScripts, offsetInFile + 6);
                    fieldItems.Add(item);
                }
            }
            FieldItems = fieldItems.Slice();
            RegularFieldItems = FieldItems.Where(item => Gen5Constants.AllowedItems.IsAllowed(item) && !Gen5Constants.AllowedItems.IsTm(item)).Slice();
            CurrentFieldTMs = FieldItems.Where(Gen5Constants.AllowedItems.IsTm).Select(TmFromIndex).Slice();
        }


        private void SaveFieldItems()
        {

            // normal items
            var scriptFileNormal = REntry.GetInt("ItemBallsScriptOffset");
            var scriptFileHidden = REntry.GetInt("HiddenItemsScriptOffset");
            var skipTable = REntry.ArrayEntries["ItemBallsSkip"];
            var skipTableH = REntry.ArrayEntries["HiddenItemsSkip"];
            var itemScripts = _scriptNarc.Files[scriptFileNormal];
            var offset = 0;
            var skipTableOffset = 0;
            
            using (var iterItems = FieldItems.GetEnumerator())
            {
                while (true)
                {
                    var part1 = ReadWord(itemScripts, offset);
                    if (part1 == Gen5Constants.ScriptListTerminator)
                        break;
                    var offsetInFile = ReadRelativePointer(itemScripts, offset);
                    offset += 4;
                    if (offsetInFile > itemScripts.Length)
                        break;
                    if (skipTableOffset < skipTable.Count &&
                        skipTable[skipTableOffset] == offset / 4 - 1)
                    {
                        skipTableOffset++;
                        continue;
                    }
                    var command = ReadWord(itemScripts, offsetInFile + 2);
                    var variable = ReadWord(itemScripts, offsetInFile + 4);
                    if (command == Gen5Constants.NormalItemSetVarCommand &&
                        variable == Gen5Constants.NormalItemVarSet)
                    {
                        iterItems.MoveNext();
                        var item = iterItems.Current;
                        WriteWord(itemScripts, offsetInFile + 6, item);
                    }
                }

                // hidden items
                var hitemScripts = _scriptNarc.Files[scriptFileHidden];
                offset = 0;
                skipTableOffset = 0;
                while (true)
                {
                    var part1 = ReadWord(hitemScripts, offset);
                    if (part1 == Gen5Constants.ScriptListTerminator)
                        break;
                    var offsetInFile = ReadRelativePointer(hitemScripts, offset);
                    offset += 4;
                    if (offsetInFile > hitemScripts.Length)
                        break;
                    if (skipTableOffset < skipTable.Count &&
                        skipTableH[skipTableOffset] == offset / 4 - 1)
                    {
                        skipTableOffset++;
                        continue;
                    }
                    var command = ReadWord(hitemScripts, offsetInFile + 2);
                    var variable = ReadWord(hitemScripts, offsetInFile + 4);

                    if (command != Gen5Constants.HiddenItemSetVarCommand || variable != Gen5Constants.HiddenItemVarSet)
                        continue;

                    iterItems.MoveNext();
                    var item = iterItems.Current;
                    WriteWord(hitemScripts, offsetInFile + 6, item);
                }
            }

            
        }

        public int TmFromIndex(int index)
        {
            if (index >= Gen5Constants.TmBlockOneOffset &&
                index < Gen5Constants.TmBlockOneOffset + Gen5Constants.TmBlockOneCount)
                return index - (Gen5Constants.TmBlockOneOffset - 1);
            return index + Gen5Constants.TmBlockOneCount - (Gen5Constants.TmBlockTwoOffset - 1);
        }

        public int IndexFromTm(int tm)
        {
            if (tm >= 1 && tm <= Gen5Constants.TmBlockOneCount)
                return tm + (Gen5Constants.TmBlockOneOffset - 1);

            return tm + (Gen5Constants.TmBlockTwoOffset - 1 - Gen5Constants.TmBlockOneCount);
        }

        private void LoadIngameTrades()
        {
            var unused = REntry.ArrayEntries["TradesUnused"];
            var unusedOffset = 0;
            var tableSize = _tradeNarc.Files.Count;

            var trades = new List<IngameTrade>();

            for (var entry = 0; entry < tableSize; entry++)
            {
                if (unusedOffset < unused.Count && unused[unusedOffset] == entry)
                {
                    unusedOffset++;
                    continue;
                }

                var trade = new IngameTrade();
                var tfile = _tradeNarc.Files[entry];
                trade.Nickname = _tradeStrings[entry * 2];
                trade.GivenPokemon = AllPokemons[ReadLong(tfile, 4)];
                trade.Ivs = new int[6];

                for (var iv = 0; iv < 6; iv++)
                    trade.Ivs[iv] = ReadLong(tfile, 0x10 + iv * 4);

                trade.OtId = ReadWord(tfile, 0x34);
                trade.Item = ReadLong(tfile, 0x4C);
                trade.OtName = _tradeStrings[entry * 2 + 1];
                trade.RequestedPokemon = AllPokemons[ReadLong(tfile, 0x5C)];
                trades.Add(trade);
            }

            IngameTrades = trades.Slice();
        }

        private void SaveIngameTrades()
        {
            // info
            var tradeOffset = 0;

            var tradeCount = _tradeNarc.Files.Count;
            var unused = REntry.ArrayEntries["TradesUnused"];
            var unusedOffset = 0;
            for (var i = 0; i < tradeCount; i++)
            {
                if (unusedOffset < unused.Count && unused[unusedOffset] == i)
                {
                    unusedOffset++;
                    continue;
                }

                var tfile = _tradeNarc.Files[i];
                var trade = IngameTrades[tradeOffset++];
                _tradeStrings[i * 2] = trade.Nickname;
                _tradeStrings[i * 2 + 1] = trade.OtName;
                WriteLong(tfile, 4, trade.GivenPokemon.Id);
                WriteLong(tfile, 8, 0); // disable forme
                for (var iv = 0; iv < 6; iv++)
                    WriteLong(tfile, 0x10 + iv * 4, trade.Ivs[iv]);
                WriteLong(tfile, 0x2C, 0xFF); // random nature
                WriteWord(tfile, 0x34, trade.OtId);
                WriteLong(tfile, 0x4C, trade.Item);
                WriteLong(tfile, 0x5C, trade.RequestedPokemon.Id);
            }

            WriteNarc(REntry.GetString("InGameTrades"), _tradeNarc);
            SetStrings(false, REntry.GetInt("IngameTradesTextOffset"), _tradeStrings);
        }

        private void LoadPokemonSprites()
        {
            PokemonSprites = new Bitmap[ValidPokemons.Count];

            for (int i = 0; i < PokemonSprites.Count; i++)
            {
                var pokemon = ValidPokemons[i];
                var pokespritesNarc = ReadNarc(REntry.GetString("PokemonGraphics"));

                // First prepare the palette, it's the easy bit
                var rawPalette = pokespritesNarc.Files[pokemon.Id * 20 + 18];
                var palette = new int[16];
                for (var j = 1; j < 16; j++)
                    palette[j] = GfxFunctions.Conv16BitColorToArgb(ReadWord(rawPalette, 40 + j * 2));

                // Get the picture and uncompress it.
                var compressedPic = pokespritesNarc.Files[pokemon.Id * 20];
                var uncompressedPic = DsDecmp.Decompress(compressedPic);

                // Output to 64x144 tiled image to prepare for unscrambling
                PokemonSprites[i] = GfxFunctions.DrawTiledImage(uncompressedPic, palette, 48, 64, 144, 4);
            }
        }

        public void LoadHiddenHollow()
        {
            if (REntry.RomType != Gen5Constants.TypeBw2)
                return;

            for (var i = 0; i < _hhNarc.Files.Count; ++i)
            {
                var data = _hhNarc.Files[i];

                for (var version = 0; version < 2; version++)
                for (var rarityslot = 0; rarityslot < 3; rarityslot++)
                for (var group = 0; group < 4; group++)
                {
                    var entry = new HiddenHollowEntry(i, version, rarityslot, group);

                    var hiddenHollow = new HiddenHollow
                    {
                        GenderRatio = data[version * 78 + rarityslot * 26 + 16 + group],
                        Form = data[version * 78 + rarityslot * 26 + 20 + group],
                        Pokemon = ReadWord(data, version * 78 + rarityslot * 26 + group * 2)
                    };

                    _hiddenHollows.Add(entry, hiddenHollow);
                }
            }
        }

        public void SaveHiddenHollow()
        {
            if (REntry.RomType != Gen5Constants.TypeBw2)
                return;

            foreach (var pair in _hiddenHollows)
            {
                var entry = pair.Key;
                var hiddenHollow = pair.Value;
                var data = _hhNarc.Files[entry.Index];

                WriteWord(data, entry.Version * 78 + entry.Rarityslot * 26 + entry.Group * 2, hiddenHollow.Pokemon);
                data[entry.Version * 78 + entry.Rarityslot * 26 + 16 + entry.Group] = hiddenHollow.GenderRatio;
                data[entry.Version * 78 + entry.Rarityslot * 26 + 20 + entry.Group] = hiddenHollow.Form;
            }

            WriteNarc(REntry.GetString("HiddenHollows"), _hhNarc);
        }

        public static bool IsLoadable(string filename)
        {
            return EntryFor(GetRomCodeFromFile(filename)) != null;
        }

        private class HiddenHollowEntry
        {
            public HiddenHollowEntry(int index, int version, int rarityslot, int group)
            {
                Index = index;
                Version = version;
                Rarityslot = rarityslot;
                Group = group;
            }

            public int Index { get; }
            public int Version { get; }
            public int Rarityslot { get; }
            public int Group { get; }

            private bool Equals(HiddenHollowEntry other)
            {
                return Index == other.Index && Version == other.Version && Rarityslot == other.Rarityslot && Group == other.Group;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                return obj.GetType() == GetType() && Equals((HiddenHollowEntry) obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = Index;
                    hashCode = (hashCode * 397) ^ Version;
                    hashCode = (hashCode * 397) ^ Rarityslot;
                    hashCode = (hashCode * 397) ^ Group;
                    return hashCode;
                }
            }
        }

        public class OffsetWithinEntry
        {
            public int Entry;
            public int Offset;
        }

        public class Gen5StaticPokemon
        {
            public IReadOnlyList<int> Files;
            public IReadOnlyList<int> Offsets;

            public Pokemon GetPokemon(Gen5RomHandler parent, NarcArchive scriptNarc)
            {
                return parent.AllPokemons[parent.ReadWord(scriptNarc.Files[Files[0]], Offsets[0])];
            }

            public void SetPokemon(Gen5RomHandler parent, NarcArchive scriptNarc, Pokemon pkmn)
            {
                var value = pkmn.Id;
                for (var i = 0; i < Offsets.Count; i++)
                {
                    var file = scriptNarc.Files[Files[i]];
                    parent.WriteWord(file, Offsets[i], value);
                }
            }
        }

        public class RomEntry
        {
            public Dictionary<string, IReadOnlyList<int>> ArrayEntries = new Dictionary<string, IReadOnlyList<int>>();
            public string Name;
            public Dictionary<string, int> Numbers = new Dictionary<string, int>();

            public Dictionary<string, ArraySlice<OffsetWithinEntry>> OffsetArrayEntries =
                new Dictionary<string, ArraySlice<OffsetWithinEntry>>();

            public string RomCode;
            public int RomType;

            public List<Gen5StaticPokemon> StaticPokemon = new List<Gen5StaticPokemon>();
            public bool StaticPokemonSupport, CopyStaticPokemon;
            public Dictionary<string, string> Strings = new Dictionary<string, string>();
            public Dictionary<string, string> TweakFiles = new Dictionary<string, string>();

            public int GetInt(string key)
            {
                if (!Numbers.ContainsKey(key))
                    Numbers[key] = 0;
                return Numbers[key];
            }

            public string GetString(string key)
            {
                if (!Strings.ContainsKey(key))
                    Strings[key] = "";
                return Strings[key];
            }
        }
    }
}