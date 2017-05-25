using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private static readonly ICollection<RomEntry> Roms = new List<RomEntry>();
        private readonly NarcArchive _driftveilNarc;
        private readonly NarcArchive _encounterNarc;
        private readonly NarcArchive _evoNarc;
        private readonly NarcArchive _hhNarc;

        private readonly Dictionary<HiddenHollowEntry, HiddenHollow> _hiddenHollows =
            new Dictionary<HiddenHollowEntry, HiddenHollow>();

        private readonly string[] _mnames;
        private readonly NarcArchive _moveNarc;
        private readonly NarcArchive _movesLearntNarc;
        private readonly byte[] _mtFile;
        private readonly string[] _pokeNames;

        private readonly NarcArchive _pokeNarc;
        private readonly NarcArchive _pokespritesNarc;

        private readonly RomEntry _romEntry;
        private readonly NarcArchive _scriptNarc;
        private readonly NarcArchive _storyTextNarc;
        private readonly NarcArchive _stringsNarc;
        private readonly NarcArchive _tradeNarc;
        private readonly string[] _tradeStrings;

        private readonly Dictionary<int, string> _wildDictionaryNames = new Dictionary<int, string>();

        public byte[] Arm9 { get; }

        public IEnumerable<HiddenHollow> HiddenHollows => _hiddenHollows.Values;

        private bool IsBw2 => Game == Game.Black2 || Game == Game.White2;

        static Gen5RomHandler()
        {
            LoadRomInfo();
        }

        public Gen5RomHandler(string filename)
            : base(filename)
        {
            _romEntry = EntryFor(BaseRom.Code);
            Arm9 = BaseRom.GetArm9();

            if (_romEntry.Name.StartsWith("Black 2"))
                Game = Game.Black2;
            else if (_romEntry.Name.StartsWith("White 2"))
                Game = Game.White2;
            else if (_romEntry.Name.StartsWith("Black"))
                Game = Game.Black;
            else if (_romEntry.Name.StartsWith("White"))
                Game = Game.White;
            else
                throw new ArgumentOutOfRangeException(nameof(_romEntry.Name), _romEntry.Name, null);

            _stringsNarc = new NarcArchive(BaseRom.GetFile(_romEntry.GetString("TextStrings")));
            _storyTextNarc = new NarcArchive(BaseRom.GetFile(_romEntry.GetString("TextStory")));
            _scriptNarc = new NarcArchive(BaseRom.GetFile(_romEntry.GetString("Scripts")));
            _pokeNarc = new NarcArchive(BaseRom.GetFile(_romEntry.GetString("PokemonStats")));
            _moveNarc = new NarcArchive(BaseRom.GetFile(_romEntry.GetString("MoveData")));
            _tradeNarc = new NarcArchive(BaseRom.GetFile(_romEntry.GetString("InGameTrades")));
            _evoNarc = new NarcArchive(BaseRom.GetFile(_romEntry.GetString("PokemonEvolutions")));
            _movesLearntNarc = new NarcArchive(BaseRom.GetFile(_romEntry.GetString("PokemonMovesets")));
            _encounterNarc = new NarcArchive(BaseRom.GetFile(_romEntry.GetString("WildPokemon")));
            _pokespritesNarc = new NarcArchive(BaseRom.GetFile(_romEntry.GetString("PokemonGraphics")));

            if (IsBw2)
            {
                _hhNarc = new NarcArchive(BaseRom.GetFile(_romEntry.GetString("HiddenHollows")));
                _driftveilNarc = new NarcArchive(BaseRom.GetFile(_romEntry.GetString("DriftveilPokemon")));
            }

            _mtFile = BaseRom.GetOverlay(_romEntry.GetInt("MoveTutorOvlNumber"));

            AbilityNames = GetStrings(false, _romEntry.GetInt("AbilityNamesTextOffset"));
            ItemNames = GetStrings(false, _romEntry.GetInt("ItemNamesTextOffset"));
            TrainerClassNames = GetStrings(false, _romEntry.GetInt("TrainerClassesTextOffset"));
            _tradeStrings = GetStrings(false, _romEntry.GetInt("IngameTradesTextOffset"));
            _mnames = GetStrings(false, _romEntry.GetInt("TrainerMugshotsTextOffset"));
            _pokeNames = GetStrings(false, _romEntry.GetInt("PokemonNamesTextOffset"));

            CanChangeTrainerText = true;
            FixedTrainerClassNamesLength = false;
            
            MaxTrainerNameLength = 10;
            MaxTrainerClassNameLength = 12;
            
            TrainerNameMode = TrainerNameMode.MaxLength;

            AllowedItems = Gen5Constants.AllowedItems;
            NonBadItems = Gen5Constants.NonBadItems;

            LoadPokemon();
            LoadStaticPokemon();
            LoadPokemonSprites();

            LoadMoves();
            LoadTmMoves();
            LoadHmMoves();
            LoadMoveTutorMoves();

            LoadFiledItems();

            LoadHiddenHollow();
            LoadEncounters();
            LoadStarters();
            LoadMoveTutorCompatibility();

            LoadIngameTrades();

            LoadDoublesTrainerClasses();
            LoadTrainerNames();
            LoadTrainers();
        }

        private static void LoadRomInfo()
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
                            current.Code = r[1];
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
                            {
                                if (r[1].Equals(otherEntry.Code, StringComparison.InvariantCultureIgnoreCase))
                                {
                                    current.ArrayEntries.AddAll(otherEntry.ArrayEntries);
                                    current.Numbers.AddAll(otherEntry.Numbers);
                                    current.Strings.AddAll(otherEntry.Strings);
                                    current.OffsetArrayEntries.AddAll(otherEntry.OffsetArrayEntries);
                                    if (current.CopyStaticPokemon)
                                    {
                                        current.Pokemon.AddRange(otherEntry.Pokemon);
                                        current.StaticPokemonSupport = true;
                                    }
                                    else
                                    {
                                        current.StaticPokemonSupport = false;
                                    }
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

                                foreach (var off in offsets)
                                {
                                    var parts = Regex.Split(off, "\\:");
                                    files[c] = ParseRiInt(parts[0]);
                                    offs[c++] = ParseRiInt(parts[1]);
                                }

                                current.Pokemon.Add(new Gen5StaticPokemon(files, offs));
                            }
                            else
                            {
                                var parts = Regex.Split(r[1], "\\:");
                                var files = ParseRiInt(parts[0]);
                                var offs = ParseRiInt(parts[1]);

                                current.Pokemon.Add(new Gen5StaticPokemon(new[] { files }, new[] { offs }));
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

        private static int ParseRiInt(string off)
        {
            var radix = 10;
            off = off.Trim().ToLower();

            if (!off.StartsWith("0x") && !off.StartsWith("&h"))
                return Convert.ToInt32(off, radix);

            radix = 16;
            off = off.Substring(2);

            return Convert.ToInt32(off, radix);
        }

        private static RomEntry EntryFor(string ndsCode)
        {
            return Roms.FirstOrDefault(re => ndsCode.Equals(re.Code));
        }

        private void LoadPokemon()
        {
            var allPokemons = new Pokemon[_pokeNames.Length];

            for (var i = 0; i < allPokemons.Length; i++)
            {
                var stats = _pokeNarc.Files[i];
                var pokemon = new Pokemon(i)
                {
                    Name = _pokeNames[i],
                    Hp = stats[Gen5Constants.BsHpOffset] & 0xFF,
                    Attack = stats[Gen5Constants.BsAttackOffset] & 0xFF,
                    Defense = stats[Gen5Constants.BsDefenseOffset] & 0xFF,
                    Speed = stats[Gen5Constants.BsSpeedOffset] & 0xFF,
                    Spatk = stats[Gen5Constants.BsSpAtkOffset] & 0xFF,
                    Spdef = stats[Gen5Constants.BsSpDefOffset] & 0xFF,
                    PrimaryType = Gen5Constants.TypeTable[stats[Gen5Constants.BsPrimaryTypeOffset] & 0xFF],
                    SecondaryType = Gen5Constants.TypeTable[stats[Gen5Constants.BsSecondaryTypeOffset] & 0xFF],
                    CatchRate = stats[Gen5Constants.BsCatchRateOffset] & 0xFF,
                    GrowthExpCurve = Exp.FromByte(stats[Gen5Constants.BsGrowthCurveOffset]),
                    Ability1 = stats[Gen5Constants.BsAbility1Offset] & 0xFF,
                    Ability2 = stats[Gen5Constants.BsAbility2Offset] & 0xFF,
                    Ability3 = stats[Gen5Constants.BsAbility3Offset] & 0xFF
                };


                // Held Items?
                var item1 = PpTxtHandler.ReadWord(stats, Gen5Constants.BsCommonHeldItemOffset);
                var item2 = PpTxtHandler.ReadWord(stats, Gen5Constants.BsRareHeldItemOffset);

                if (item1 == item2)
                {
                    // guaranteed
                    pokemon.GuaranteedHeldItem = item1;
                    pokemon.CommonHeldItem = 0;
                    pokemon.RareHeldItem = 0;
                    pokemon.DarkGrassHeldItem = 0;
                }
                else
                {
                    pokemon.GuaranteedHeldItem = 0;
                    pokemon.CommonHeldItem = item1;
                    pokemon.RareHeldItem = item2;
                    pokemon.DarkGrassHeldItem = PpTxtHandler.ReadWord(stats, Gen5Constants.BsDarkGrassHeldItemOffset);
                }

                // Only one type?
                if (pokemon.SecondaryType == pokemon.PrimaryType)
                    pokemon.SecondaryType = null;

                // Load learnt moves
                var movedata = _movesLearntNarc.Files[i];
                var moveDataLoc = 0;

                while (PpTxtHandler.ReadWord(movedata, moveDataLoc) != 0xFFFF || PpTxtHandler.ReadWord(movedata, moveDataLoc + 2) != 0xFFFF)
                {
                    var move = PpTxtHandler.ReadWord(movedata, moveDataLoc);
                    var level = PpTxtHandler.ReadWord(movedata, moveDataLoc + 2);
                    var ml = new MoveLearnt
                    {
                        Level = level,
                        Move = move
                    };

                    pokemon.MovesLearnt.Add(ml);
                    moveDataLoc += 4;
                }

                // Load TMHM Compatibility
                var data = _pokeNarc.Files[i];
                var flags = new bool[Gen5Constants.TmCount + Gen5Constants.HmCount + 1];

                for (var j = 0; j < 13; j++)
                    ReadByteIntoFlags(data, flags, j * 8 + 1, Gen5Constants.BsTmhmCompatOffset + j);

                pokemon.TMHMCompatibility = flags;

                allPokemons[i] = pokemon;
            }

            AllPokemons = allPokemons;
            PopulateEvolutions();
        }

        private void LoadMoves()
        {
            var moveNames = GetStrings(false, _romEntry.GetInt("MoveNamesTextOffset"));
            var allMoves = new Move[moveNames.Length];

            for (var i = 0; i < allMoves.Length; i++)
            {
                var moveData = _moveNarc.Files[i];
                allMoves[i] = new Move
                {
                    Name = moveNames[i],
                    Number = i,
                    Id = i,
                    Hitratio = moveData[4] & 0xFF,
                    Power = moveData[3] & 0xFF,
                    Pp = moveData[5] & 0xFF,
                    Type = Gen5Constants.TypeTable[moveData[0] & 0xFF],
                    Category = Gen5Constants.MoveCategoryIndices[moveData[2] & 0xFF]
                };

                if (GlobalConstants.NormalMultihitMoves.Contains(i))
                    allMoves[i].HitCount = 19 / 6.0;
                else if (GlobalConstants.DoubleHitMoves.Contains(i))
                    allMoves[i].HitCount = 2;
                else if (i == GlobalConstants.TripleKickIndex)
                    allMoves[i].HitCount = 2.71; // this assumes the first hit
            }

            AllMoves = allMoves;
            FieldMoves = Gen5Constants.FieldMoves;
        }

        public override bool SaveRom(string filename)
        {
            SaveTrainerNames();
            SaveIngameTrades();
            SaveFieldItems();
            SaveMoveTutorCompatibility();
            SaveMoveTutorMoves();
            SaveTmMoves();
            SaveEncounters();
            SaveHiddenHollow();
            SaveTrainers();
            SavePokemon();
            SaveMoves();
            SaveStarters();

            SetStrings(false, _romEntry.GetInt("TrainerClassesTextOffset"), TrainerClassNames);
            
            BaseRom.WriteFile(_romEntry.GetString("TextStrings"), _stringsNarc.Bytes);
            BaseRom.WriteFile(_romEntry.GetString("TextStory"), _storyTextNarc.Bytes);
            BaseRom.WriteFile(_romEntry.GetString("Scripts"), _scriptNarc.Bytes);

            BaseRom.WriteArm9(Arm9);
            BaseRom.SaveTo(filename);
            return true;
        }

        private void SaveMoves()
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

            BaseRom.WriteFile(_romEntry.GetString("MoveData"), _moveNarc.Bytes);
        }

        private void SavePokemon()
        {
            for (var i = 0; i < _pokeNames.Length; i++)
            {
                var pokemon = AllPokemons[i];
                var data = _pokeNarc.Files[pokemon.Id];

                _pokeNames[i] = AllPokemons[i].Name;
                data[Gen5Constants.BsHpOffset] = (byte) pokemon.Hp;
                data[Gen5Constants.BsAttackOffset] = (byte) pokemon.Attack;
                data[Gen5Constants.BsDefenseOffset] = (byte) pokemon.Defense;
                data[Gen5Constants.BsSpeedOffset] = (byte) pokemon.Speed;
                data[Gen5Constants.BsSpAtkOffset] = (byte) pokemon.Spatk;
                data[Gen5Constants.BsSpDefOffset] = (byte) pokemon.Spdef;
                data[Gen5Constants.BsPrimaryTypeOffset] = Gen5Constants.TypeToByte(pokemon.PrimaryType);

                if (pokemon.SecondaryType == null)
                    data[Gen5Constants.BsSecondaryTypeOffset] = data[Gen5Constants.BsPrimaryTypeOffset];
                else
                    data[Gen5Constants.BsSecondaryTypeOffset] = Gen5Constants.TypeToByte(pokemon.SecondaryType);

                data[Gen5Constants.BsCatchRateOffset] = (byte) pokemon.CatchRate;
                data[Gen5Constants.BsGrowthCurveOffset] = pokemon.GrowthExpCurve.ToByte();

                data[Gen5Constants.BsAbility1Offset] = (byte) pokemon.Ability1;
                data[Gen5Constants.BsAbility2Offset] = (byte) pokemon.Ability2;
                data[Gen5Constants.BsAbility3Offset] = (byte) pokemon.Ability3;

                // Held items
                if (pokemon.GuaranteedHeldItem > 0)
                {
                    PpTxtHandler.WriteWord(data, Gen5Constants.BsCommonHeldItemOffset, pokemon.GuaranteedHeldItem);
                    PpTxtHandler.WriteWord(data, Gen5Constants.BsRareHeldItemOffset, pokemon.GuaranteedHeldItem);
                    PpTxtHandler.WriteWord(data, Gen5Constants.BsDarkGrassHeldItemOffset, 0);
                }
                else
                {
                    PpTxtHandler.WriteWord(data, Gen5Constants.BsCommonHeldItemOffset, pokemon.CommonHeldItem);
                    PpTxtHandler.WriteWord(data, Gen5Constants.BsRareHeldItemOffset, pokemon.RareHeldItem);
                    PpTxtHandler.WriteWord(data, Gen5Constants.BsDarkGrassHeldItemOffset, pokemon.DarkGrassHeldItem);
                }


                // Save moves learnt
                {
                    var learnt = pokemon.MovesLearnt;
                    var sizeNeeded = learnt.Count * 4 + 4;
                    var moveset = new byte[sizeNeeded];
                    var j = 0;
                    for (; j < learnt.Count; j++)
                    {
                        var ml = learnt[j];
                        PpTxtHandler.WriteWord(moveset, j * 4, ml.Move);
                        PpTxtHandler.WriteWord(moveset, j * 4 + 2, ml.Level);
                    }
                    PpTxtHandler.WriteWord(moveset, j * 4, 0xFFFF);
                    PpTxtHandler.WriteWord(moveset, j * 4 + 2, 0xFFFF);
                    _movesLearntNarc.Files[i] = moveset;
                }


                // Save TmHm compatibility

                for (var j = 0; j < 13; j++)
                    data[Gen5Constants.BsTmhmCompatOffset + j] = GetByteFromFlags(pokemon.TMHMCompatibility, j * 8 + 1);
            }


            SetStrings(false, _romEntry.GetInt("PokemonNamesTextOffset"), _pokeNames);
            BaseRom.WriteFile(_romEntry.GetString("PokemonStats"), _pokeNarc.Bytes);
            BaseRom.WriteFile(_romEntry.GetString("PokemonMovesets"), _movesLearntNarc.Bytes);
            SaveEvolutions();
        }


        private void LoadStarters()
        {
            var starters = new StarterPokemon[3];

            for (var i = 0; i < starters.Length; i++)
            {
                var thisStarter = _romEntry.OffsetArrayEntries["StarterOffsets" + (i + 1)];
                starters[i] = new StarterPokemon
                {
                    Pokemon = AllPokemons[PpTxtHandler.ReadWord(_scriptNarc.Files[thisStarter[0].Entry], thisStarter[0].Offset)]
                };
            }

            Starters = starters;
        }

        private void SaveStarters()
        {
            if (Starters.Length != 3)
                return;

            for (var i = 0; i < 3; i++)
            {
                var thisStarter = _romEntry.OffsetArrayEntries["StarterOffsets" + (i + 1)];
                foreach (var entry in thisStarter)
                {
                    PpTxtHandler.WriteWord(_scriptNarc.Files[entry.Entry], entry.Offset, Starters[i].Pokemon.Id);
                }
            }

            // GIVE ME BACK MY PURRLOIN
            ApplyStarterScript(IsBw2 ? Gen5Constants.Bw2NewStarterScript : Gen5Constants.Bw1NewStarterScript);

            // Starter sprites
            var starterNarc = new NarcArchive(BaseRom.GetFile(_romEntry.GetString("StarterGraphics")));
            var pokespritesNarc = new NarcArchive(BaseRom.GetFile(_romEntry.GetString("PokemonGraphics")));
            ReplaceStarterFiles(starterNarc, pokespritesNarc, 0, Starters[0].Pokemon.Id);
            ReplaceStarterFiles(starterNarc, pokespritesNarc, 1, Starters[1].Pokemon.Id);
            ReplaceStarterFiles(starterNarc, pokespritesNarc, 2, Starters[2].Pokemon.Id);
            BaseRom.WriteFile(_romEntry.GetString("StarterGraphics"), starterNarc.Bytes);

            // Fix text depending on version
            if (!IsBw2)
            {
                IList<string> yourHouseStrings = GetStrings(true, _romEntry.GetInt("StarterLocationTextOffset"));
                for (var i = 0; i < 3; i++)
                {
                    yourHouseStrings[Gen5Constants.Bw1StarterTextOffset - i] =
                        $"\\xF000\\xBD02\\x0000The {Starters[i].Pokemon.PrimaryType.CamelCase()}-type Pok\\x00E9mon\\xFFFE\\xF000\\xBD02\\x0000{Starters[i].Pokemon.Name}";
                }
                // Update what the friends say
                yourHouseStrings[Gen5Constants.Bw1CherenText1Offset] =
                    "Cheren: Hey, how come you get to pick\\xFFFEout my Pok\\x00E9mon?\\xF000\\xBE01\\x0000\\xFFFEOh, never mind. I wanted this one\\xFFFEfrom the start, anyway.\\xF000\\xBE01\\x0000";
                yourHouseStrings[Gen5Constants.Bw1CherenText2Offset] =
                    "It's decided. You'll be my opponent...\\xFFFEin our first Pok\\x00E9mon battle!\\xF000\\xBE01\\x0000\\xFFFELet's see what you can do, \\xFFFEmy Pok\\x00E9mon!\\xF000\\xBE01\\x0000";

                // rewrite
                SetStrings(true, _romEntry.GetInt("StarterLocationTextOffset"), yourHouseStrings);
            }
            else
            {
                IList<string> starterTownStrings = GetStrings(true, _romEntry.GetInt("StarterLocationTextOffset"));
                for (var i = 0; i < 3; i++)
                {
                    starterTownStrings[Gen5Constants.Bw2StarterTextOffset - i] =
                        $"\\xF000\\xBD02\\x0000The {Starters[i].Pokemon.PrimaryType.CamelCase()}-type Pok\\x00E9mon\\xFFFE\\xF000\\xBD02\\x0000{Starters[i].Pokemon.Name}";
                }

                // Update what the rival says
                starterTownStrings[Gen5Constants.Bw2RivalTextOffset] =
                    "\\xF000\\x0100\\x0001\\x0001: Let's see how good\\xFFFEa Trainer you are!\\xF000\\xBE01\\x0000\\xFFFEI'll use my Pok\\x00E9mon\\xFFFEthat I raised from an Egg!\\xF000\\xBE01\\x0000";

                // rewrite
                SetStrings(true, _romEntry.GetInt("StarterLocationTextOffset"), starterTownStrings);
            }
        }

        private void ApplyStarterScript(IReadOnlyCollection<byte> newScript)
        {
            var oldFile = _scriptNarc.Files[_romEntry.GetInt("PokedexGivenFileOffset")];
            var newFile = new byte[oldFile.Length + newScript.Count];
            var offset = Find(oldFile, Gen5Constants.Bw1StarterScriptMagic);

            if (offset <= 0)
                return;

            oldFile.CopyTo(newFile, 0);
            Array.Copy(newScript.ToArray(), 0, newFile, oldFile.Length, newScript.Count);
            if (_romEntry.Code[3] == 'J')
            {
                newFile[oldFile.Length + 0x4] -= 4;
                newFile[oldFile.Length + 0x8] -= 4;
            }

            newFile[offset++] = 0x04;
            newFile[offset++] = 0x0;
            WriteRelativePointer(newFile, offset, oldFile.Length);
            _scriptNarc.Files[_romEntry.GetInt("PokedexGivenFileOffset")] = newFile;
        }


        private void ReplaceStarterFiles(
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


        private void LoadEncounters()
        {
            // LoadWildDictionaryNames
            var mapHeaderData = new NarcArchive(BaseRom.GetFile(_romEntry.GetString("MapTableFile"))).Files[0];
            var allDictionaryNames = GetStrings(false, _romEntry.GetInt("MapNamesTextOffset"));
            var numDictionaryHeaders = mapHeaderData.Length / 48;
            for (var map = 0; map < numDictionaryHeaders; map++)
            {
                var baseOffset = map * 48;
                var mapNameIndex = mapHeaderData[baseOffset + 26] & 0xFF;
                var mapName1 = allDictionaryNames[mapNameIndex];

                if (IsBw2)
                {
                    var wildSet = mapHeaderData[baseOffset + 20] & 0xFF;
                    if (wildSet != 255)
                        _wildDictionaryNames[wildSet] = mapName1;
                }
                else
                {
                    var wildSet = PpTxtHandler.ReadWord(mapHeaderData, baseOffset + 20);
                    if (wildSet != 65535)
                        _wildDictionaryNames[wildSet] = mapName1;
                }
            }

            var encounters = new List<EncounterSet>();
            var idx = -1;
            foreach (var entry in _encounterNarc.Files)
            {
                idx++;
                if (entry.Length > Gen5Constants.PerSeasonEncounterDataLength)
                    for (var i = 0; i < 4; i++)
                        ProcessEncounterEntry(encounters, entry, i * Gen5Constants.PerSeasonEncounterDataLength, idx);

                else
                    ProcessEncounterEntry(encounters, entry, 0, idx);
            }

            Encounters = encounters.ToArray();

            void ProcessEncounterEntry(
                ICollection<EncounterSet> encounts,
                IList<byte> entry,
                int startOffset,
                int id)
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
                            Encounters = encs,
                            Offset = id,
                            DisplayName = mapName + " " + Gen5Constants.EncounterTypeNames[i]
                        };

                        encounts.Add(area);
                    }
                    off += amounts[i] * 4;
                }


                Encounter[] ReadEncounters(IList<byte> data, int offset, int number)
                {
                    var encs = new Encounter[number];
                    for (var i = 0; i < number; i++)
                    {
                        var offset1 = data[offset + i * 4] & 0xFF;
                        var offset2 = (data[offset + 1 + i * 4] & 0x03) << 8;

                        var enc1 = new Encounter
                        {
                            Pokemon1 = AllPokemons[offset1 + offset2],
                            Level = data[offset + 2 + i * 4] & 0xFF,
                            MaxLevel = data[offset + 3 + i * 4] & 0xFF
                        };

                        encs[i] = enc1;
                    }
                    return encs;
                }
            }
        }


        private void SaveEncounters()
        {
            var encounters = Encounters.GetEnumerator();
            foreach (var entry in _encounterNarc.Files)
            {
                WriteEncounterEntry(encounters, entry, 0);

                if (entry.Length <= 232)
                    continue;

                for (var i = 1; i < 4; i++)
                    WriteEncounterEntry(encounters, entry, i * 232);
            }

            // Habitat List / Area Data?
            if (!IsBw2)
                return;

            var areaNarc = new NarcArchive(BaseRom.GetFile(_romEntry.GetString("PokemonAreaData")));
            var newFiles = new byte[Gen5Constants.PokemonCount][];
            for (var i = 0; i < Gen5Constants.PokemonCount; i++)
            {
                var nf = new byte[Gen5Constants.Bw2AreaDataEntryLength];
                nf[0] = 1;
                newFiles[i] = nf;
            }

            // Get data now
            for (var i = 0; i < _encounterNarc.Files.Count; i++)
            {
                var encEntry = _encounterNarc.Files[i];
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
                    {
                        if (file[s * (Gen5Constants.Bw2EncounterAreaCount + 1) + e + 2] != 0)
                        {
                            unobtainable = false;
                            break;
                        }
                    }
                    if (unobtainable)
                        file[s * (Gen5Constants.Bw2EncounterAreaCount + 1) + 1] = 1;
                }

                areaNarc.Files[i] = file;
            }

            // Save
            BaseRom.WriteFile(_romEntry.GetString("PokemonAreaData"), areaNarc.Bytes);
            BaseRom.WriteFile(_romEntry.GetString("WildPokemon"), _encounterNarc.Bytes);


            void ParseAreaData(ArraySlice<byte> entry, int startOffset, byte[][] areaData, int season, int fileNumber)
            {
                var amounts = Gen5Constants.EncountersOfEachType;

                var offset = 8;
                for (var i = 0; i < 7; i++)
                {
                    var rate = entry[startOffset + i] & 0xFF;
                    if (rate != 0)
                        for (var e = 0; e < amounts[i]; e++)
                        {
                            var newOffset = startOffset + offset + e * 4;
                            var pkmn = AllPokemons[(entry[newOffset] & 0xFF) + ((entry[newOffset + 1] & 0x03) << 8)];
                            var pokeFile = areaData[pkmn.Id - 1];
                            var areaIndex = Gen5Constants.WildFileToAreaMap[fileNumber];

                            // Route 4?
                            if (areaIndex == Gen5Constants.Bw2Route4AreaIndex)
                                if (fileNumber == Gen5Constants.B2Route4EncounterFile && _romEntry.Code[2] == 'D')
                                    areaIndex = -1; // wrong version
                                else if (fileNumber == Gen5Constants.W2Route4EncounterFile && _romEntry.Code[2] == 'E')
                                    areaIndex = -1; // wrong version

                            // Victory Road?
                            if (areaIndex == Gen5Constants.Bw2VictoryRoadAreaIndex)
                                if (_romEntry.Code[2] == 'D')
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
                                if (_romEntry.Code[2] == 'D')
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
                            {
                                pokeFile[season * (Gen5Constants.Bw2EncounterAreaCount + 1) + 2 + areaIndex] |=
                                    (byte) (1 << i);
                            }
                        }
                    offset += amounts[i] * 4;
                }
            }

            void WriteEncounterEntry(IEnumerator encs, ArraySlice<byte> entry, int startOffset)
            {
                var amounts = Gen5Constants.EncountersOfEachType;

                var offset = 8;
                for (var i = 0; i < 7; i++)
                {
                    var rate = entry[startOffset + i] & 0xFF;
                    if (rate != 0)
                    {
                        encs.MoveNext();
                        var area = (EncounterSet) encs.Current;
                        for (var j = 0; j < amounts[i]; j++)
                        {
                            var enc = area.Encounters[j];
                            PpTxtHandler.WriteWord(entry, startOffset + offset + j * 4, enc.Pokemon1.Id);
                            entry[startOffset + offset + j * 4 + 2] = (byte) enc.Level;
                            entry[startOffset + offset + j * 4 + 3] = (byte) enc.MaxLevel;
                        }
                    }
                    offset += amounts[i] * 4;
                }
            }
        }


        private void AddHabitats(
            ArraySlice<byte> entry,
            int startOffset,
            Dictionary<Pokemon, ArraySlice<byte>> pokemonHere,
            int season)
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


        private void LoadTrainers()
        {
            var allTrainers = new List<Trainer>();

            var trainers = new NarcArchive(BaseRom.GetFile(_romEntry.GetString("TrainerData")));
            var trpokes = new NarcArchive(BaseRom.GetFile(_romEntry.GetString("TrainerPokemon")));
            var trainernum = trainers.Files.Count;

            for (var i = 1; i < trainernum; i++)
            {
                var trainer = trainers.Files[i];
                var trpoke = trpokes.Files[i];
                var numPokes = trainer[3] & 0xFF;
                var pokeOffs = 0;

                var tr = new Trainer
                {
                    Poketype = trainer[0] & 0xFF,
                    Offset = i,
                    Trainerclass = trainer[1] & 0xFF,
                    Pokemon = new TrainerPokemon[numPokes]
                };


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
                    var level = PpTxtHandler.ReadWord(trpoke, pokeOffs + 2);
                    var species = PpTxtHandler.ReadWord(trpoke, pokeOffs + 4);
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
                        var heldItem = PpTxtHandler.ReadWord(trpoke, pokeOffs);
                        tpk.HeldItem = heldItem;
                        pokeOffs += 2;
                    }
                    if ((tr.Poketype & 1) == 1)
                    {
                        var attack1 = PpTxtHandler.ReadWord(trpoke, pokeOffs);
                        var attack2 = PpTxtHandler.ReadWord(trpoke, pokeOffs + 2);
                        var attack3 = PpTxtHandler.ReadWord(trpoke, pokeOffs + 4);
                        var attack4 = PpTxtHandler.ReadWord(trpoke, pokeOffs + 6);
                        tpk.Move1 = attack1;
                        tpk.Move2 = attack2;
                        tpk.Move3 = attack3;
                        tpk.Move4 = attack4;
                        pokeOffs += 8;
                    }
                    tr.Pokemon[poke] = tpk;
                }
                allTrainers.Add(tr);
            }

            if (!IsBw2)
            {
                Gen5Constants.TagTrainersBw(allTrainers);
                Trainers = allTrainers.ToArray();
            }
            else
            {
                for (var trno = 0; trno < 2; trno++)
                {
                    var tr = new Trainer
                    {
                        Poketype = 3,
                        Offset = 0,
                        Pokemon = new TrainerPokemon[3]
                    };

                    for (var poke = 0; poke < tr.Pokemon.Length; poke++)
                    {
                        var pkmndata = _driftveilNarc.Files[trno * 3 + poke + 1];
                        var tpk = new TrainerPokemon
                        {
                            Level = 25,
                            Pokemon = AllPokemons[PpTxtHandler.ReadWord(pkmndata, 0)],
                            AiLevel = 255,
                            HeldItem = PpTxtHandler.ReadWord(pkmndata, 12),
                            Move1 = PpTxtHandler.ReadWord(pkmndata, 2),
                            Move2 = PpTxtHandler.ReadWord(pkmndata, 4),
                            Move3 = PpTxtHandler.ReadWord(pkmndata, 6),
                            Move4 = PpTxtHandler.ReadWord(pkmndata, 8)
                        };

                        tr.Pokemon[poke] = tpk;
                    }
                    allTrainers.Add(tr);
                }
                Gen5Constants.TagTrainersBw2(allTrainers);
                Trainers = allTrainers.ToArray();
            }
        }


        private void SaveTrainers()
        {
            using (var allTrainers = Trainers.GetEnumerator<Trainer>())
            {
                var trainers = new NarcArchive(BaseRom.GetFile(_romEntry.GetString("TrainerData")));
                var trpokes = new NarcArchive(BaseRom.GetFile(_romEntry.GetString("TrainerPokemon")));

                var trainernum = trainers.Files.Count;
                for (var i = 1; i < trainernum; i++)
                {
                    var trainer = trainers.Files[i];
                    allTrainers.MoveNext();
                    var tr = allTrainers.Current;
                    // preserve original poketype for held item & moves
                    trainer[0] = (byte) tr.Poketype;
                    var numPokes = tr.Pokemon.Length;

                    trainer[3] = (byte) numPokes;

                    var trpoke = trpokes.Files[i];
                    var pokeOffs = 0;

                    using (var tpokes = ((IEnumerable<TrainerPokemon>) tr.Pokemon).GetEnumerator())
                    {
                        for (var poke = 0; poke < numPokes; poke++)
                        {
                            tpokes.MoveNext();
                            var tp = tpokes.Current;
                            trpoke[pokeOffs] = (byte) tp.AiLevel;
                            // no gender or ability info, so no byte 1
                            PpTxtHandler.WriteWord(trpoke, pokeOffs + 2, tp.Level);
                            PpTxtHandler.WriteWord(trpoke, pokeOffs + 4, tp.Pokemon.Id);
                            // no form info, so no byte 6/7
                            pokeOffs += 8;
                            if ((tr.Poketype & 2) == 2)
                            {
                                PpTxtHandler.WriteWord(trpoke, pokeOffs, tp.HeldItem);
                                pokeOffs += 2;
                            }

                            if ((tr.Poketype & 1) != 1)
                                continue;

                            if (tp.ResetMoves)
                            {
                                var pokeMoves = RomFunctions.GetMovesAtLevel(tp.Pokemon, tp.Level);
                                for (var m = 0; m < 4; m++)
                                {
                                    PpTxtHandler.WriteWord(trpoke, pokeOffs + m * 2, pokeMoves[m]);
                                }
                            }
                            else
                            {
                                PpTxtHandler.WriteWord(trpoke, pokeOffs, tp.Move1);
                                PpTxtHandler.WriteWord(trpoke, pokeOffs + 2, tp.Move2);
                                PpTxtHandler.WriteWord(trpoke, pokeOffs + 4, tp.Move3);
                                PpTxtHandler.WriteWord(trpoke, pokeOffs + 6, tp.Move4);
                            }
                            pokeOffs += 8;
                        }
                    }
                }

                BaseRom.WriteFile(_romEntry.GetString("TrainerData"), trainers.Bytes);
                BaseRom.WriteFile(_romEntry.GetString("TrainerPokemon"), trpokes.Bytes);

                // Deal with PWT
                if (_driftveilNarc == null)
                    return;

                for (var trno = 0; trno < 2; trno++)
                {
                    allTrainers.MoveNext();
                    var tr = allTrainers.Current;

                    using (var tpks = ((IEnumerable<TrainerPokemon>) tr.Pokemon).GetEnumerator())
                    {
                        for (var poke = 0; poke < 3; poke++)
                        {
                            var pkmndata = _driftveilNarc.Files[trno * 3 + poke + 1];
                            tpks.MoveNext();
                            var tp = tpks.Current;
                            // pokemon and held item
                            PpTxtHandler.WriteWord(pkmndata, 0, tp.Pokemon.Id);
                            PpTxtHandler.WriteWord(pkmndata, 12, tp.HeldItem);
                            // handle moves
                            if (tp.ResetMoves)
                            {
                                var pokeMoves = RomFunctions.GetMovesAtLevel(tp.Pokemon, tp.Level);
                                for (var m = 0; m < 4; m++)
                                {
                                    PpTxtHandler.WriteWord(pkmndata, 2 + m * 2, pokeMoves[m]);
                                }
                            }
                            else
                            {
                                PpTxtHandler.WriteWord(pkmndata, 2, tp.Move1);
                                PpTxtHandler.WriteWord(pkmndata, 4, tp.Move2);
                                PpTxtHandler.WriteWord(pkmndata, 6, tp.Move3);
                                PpTxtHandler.WriteWord(pkmndata, 8, tp.Move4);
                            }
                        }
                    }
                }

                BaseRom.WriteFile(_romEntry.GetString("DriftveilPokemon"), _driftveilNarc.Bytes);
            }
        }


        private void LoadStaticPokemon()
        {
            if (!_romEntry.StaticPokemonSupport)
            {
                StaticPokemon = Array.Empty<Pokemon>();
                return;
            }

            StaticPokemon = new Pokemon[_romEntry.Pokemon.Count];

            for (var i = 0; i < StaticPokemon.Length; i++)
            {
                var staticPokemon = _romEntry.Pokemon[i];
                StaticPokemon[i] = staticPokemon.GetPokemon(this, _scriptNarc);
            }
        }

        public bool GenericIpsPatch(ArraySlice<byte> data, string ctName)
        {
            var patchName = _romEntry.TweakFiles[ctName];
            if (patchName == null)
                return false;

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
            {
                TmMoves[i] = PpTxtHandler.ReadWord(Arm9, offset + i * 2);
            }

            // Skip past first 92 TMs and 6 HMs
            offset += (Gen5Constants.TmBlockOneCount + Gen5Constants.HmCount) * 2;

            for (var i = 0; i < Gen5Constants.TmCount - Gen5Constants.TmBlockOneCount; i++)
            {
                TmMoves[i + Gen5Constants.TmBlockOneCount] = PpTxtHandler.ReadWord(Arm9, offset + i * 2);
            }


            RequiredFieldTMs = IsBw2 ? Gen5Constants.Bw2RequiredFieldTMs : Gen5Constants.Bw1RequiredFieldTMs;
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
            {
                PpTxtHandler.WriteWord(Arm9, offset + i * 2, TmMoves[i]);
            }
            // Skip past those 92 TMs and 6 HMs
            offset += (Gen5Constants.TmBlockOneCount + Gen5Constants.HmCount) * 2;
            for (var i = 0; i < Gen5Constants.TmCount - Gen5Constants.TmBlockOneCount; i++)
            {
                PpTxtHandler.WriteWord(Arm9, offset + i * 2, TmMoves[i + Gen5Constants.TmBlockOneCount]);
            }

            // Update TM item descriptions
            var itemDescriptions = GetStrings(false, _romEntry.GetInt("ItemDescriptionsTextOffset"));
            var moveDescriptions = GetStrings(false, _romEntry.GetInt("MoveDescriptionsTextOffset"));
            // TM01 is item 328 and so on
            for (var i = 0; i < Gen5Constants.TmBlockOneCount; i++)
                itemDescriptions[i + Gen5Constants.TmBlockOneOffset] = moveDescriptions[TmMoves[i]];
            // TM93-95 are 618-620
            for (var i = 0; i < Gen5Constants.TmCount - Gen5Constants.TmBlockOneCount; i++)
            {
                itemDescriptions[i + Gen5Constants.TmBlockTwoOffset] =
                    moveDescriptions[TmMoves[i + Gen5Constants.TmBlockOneCount]];
            }
            // Save the new item descriptions
            SetStrings(false, _romEntry.GetInt("ItemDescriptionsTextOffset"), itemDescriptions);

            // Palettes
            var baseOfPalettes = IsBw2 ? Gen5Constants.Bw2ItemPalettesPrefix : Gen5Constants.Bw1ItemPalettesPrefix;

            var offsPals = Find(Arm9, baseOfPalettes);
            if (offsPals > 0)
            {
                // Write pals
                for (var i = 0; i < Gen5Constants.TmBlockOneCount; i++)
                {
                    var itmNum = Gen5Constants.TmBlockOneOffset + i;
                    var m = AllMoves[TmMoves[i]];
                    var pal = TypeTmPaletteNumber(m.Type);
                    PpTxtHandler.WriteWord(Arm9, offsPals + itmNum * 4 + 2, pal);
                }
                for (var i = 0; i < Gen5Constants.TmCount - Gen5Constants.TmBlockOneCount; i++)
                {
                    var itmNum = Gen5Constants.TmBlockTwoOffset + i;
                    var m = AllMoves[TmMoves[i + Gen5Constants.TmBlockOneCount]];
                    var pal = TypeTmPaletteNumber(m.Type);
                    PpTxtHandler.WriteWord(Arm9, offsPals + itmNum * 4 + 2, pal);
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

            var hmMoves = new int[Gen5Constants.HmCount];

            for (var i = 0; i < hmMoves.Length; i++)
            {
                hmMoves[i] = PpTxtHandler.ReadWord(Arm9, offset + i * 2);
            }

            HmMoves = hmMoves;
            EarlyRequiredHmMoves = IsBw2 ? Gen5Constants.Bw2EarlyRequiredHmMoves : Gen5Constants.Bw1EarlyRequiredHmMoves;
        }


        private void LoadMoveTutorMoves()
        {
            if (!Game.HasMoveTutors())
            {
                MoveTutorMoves = Array.Empty<int>();
                return;
            }

            var baseOffset = _romEntry.GetInt("MoveTutorDataOffset");

            MoveTutorMoves = new int[Gen5Constants.Bw2MoveTutorCount];

            for (var i = 0; i < MoveTutorMoves.Length; i++)
            {
                MoveTutorMoves[i] = PpTxtHandler.ReadWord(_mtFile, baseOffset + i * Gen5Constants.Bw2MoveTutorBytesPerEntry);
            }
        }

        private void SaveMoveTutorMoves()
        {
            if (!Game.HasMoveTutors())
                return;
            var baseOffset = _romEntry.GetInt("MoveTutorDataOffset");
            var amount = Gen5Constants.Bw2MoveTutorCount;
            var bytesPer = Gen5Constants.Bw2MoveTutorBytesPerEntry;
            if (MoveTutorMoves.Length != amount)
                return;

            for (var i = 0; i < amount; i++)
            {
                PpTxtHandler.WriteWord(_mtFile, baseOffset + i * bytesPer, MoveTutorMoves[i]);
            }

            BaseRom.WriteOverlay(_romEntry.GetInt("MoveTutorOvlNumber"), _mtFile);
        }


        private void LoadMoveTutorCompatibility()
        {
            if (!Game.HasMoveTutors())
                return;


            var countsPersonalOrder = new[] { 15, 17, 13, 15 };
            var countsMoveOrder = new[] { 13, 15, 15, 17 };
            var personalToMoveOrder = new[] { 1, 3, 0, 2 };
            for (var i = 0; i < AllPokemons.Length; i++)
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

                pkmn.MoveTutorCompatibility = flags;
            }
        }

        private void SaveMoveTutorCompatibility()
        {
            if (!Game.HasMoveTutors())
                return;

            // BW2 move tutor flags aren't using the same order as the move tutor
            // move data.
            // We unscramble them from move data order to personal.narc flag order.
            ArraySlice<int> countsPersonalOrder = new[] { 15, 17, 13, 15 };
            ArraySlice<int> countsMoveOrder = new[] { 13, 15, 15, 17 };
            ArraySlice<int> personalToMoveOrder = new[] { 1, 3, 0, 2 };
            foreach (var pkmn in AllPokemons)
            {
                var flags = pkmn.MoveTutorCompatibility;
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

        private static int Find(IList<byte> data, string hexString)
        {
            if (hexString.Length % 2 != 0)
                return -3; // error

            var searchFor = new byte[hexString.Length / 2];
            for (var i = 0; i < searchFor.Length; i++)
                searchFor[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);

            var found = RomFunctions.Search(data, searchFor);

            if (found.Count == 0)
                return -1; // not found
            if (found.Count > 1)
                return -2; // not unique

            return found[0];
        }

        private string[] GetStrings(bool isStoryText, int index)
        {
            var baseNarc = isStoryText ? _storyTextNarc : _stringsNarc;
            var rawFile = baseNarc.Files[index];
            return PpTxtHandler.ReadTexts(rawFile);
        }

        private void SetStrings(bool isStoryText, int index, IList<string> strings)
        {
            var baseNarc = isStoryText ? _storyTextNarc : _stringsNarc;
            var oldRawFile = baseNarc.Files[index];
            var newRawFile = PpTxtHandler.SaveEntry(oldRawFile, strings);
            baseNarc.Files[index] = newRawFile;
        }

        private void PopulateEvolutions()
        {
            foreach (var pkmn in AllPokemons)
            {
                if (pkmn == null)
                    continue;

                pkmn.EvolutionsFrom.Clear();
                pkmn.EvolutionsTo.Clear();
            }

            for (var i = 0; i < AllPokemons.Length; i++)
            {
                var pk = AllPokemons[i];
                var evoEntry = _evoNarc.Files[i];
                for (var evo = 0; evo < 7; evo++)
                {
                    var method = PpTxtHandler.ReadWord(evoEntry, evo * 6);
                    var species = PpTxtHandler.ReadWord(evoEntry, evo * 6 + 4);
                    if (method >= 1 &&
                        method <= Gen5Constants.EvolutionMethodCount &&
                        species >= 1)
                    {
                        var et = EvolutionType.FromIndex(5, method);
                        var extraInfo = PpTxtHandler.ReadWord(evoEntry, evo * 6 + 2);
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

        private void SaveEvolutions()
        {
            for (var i = 0; i < AllPokemons.Length; i++)
            {
                var evoEntry = _evoNarc.Files[i];
                var pk = AllPokemons[i];
                var evosWritten = 0;
                foreach (var evo in pk.EvolutionsFrom)
                {
                    PpTxtHandler.WriteWord(evoEntry, evosWritten * 6, evo.Type1.ToIndex(5));
                    PpTxtHandler.WriteWord(evoEntry, evosWritten * 6 + 2, evo.ExtraInfo);
                    PpTxtHandler.WriteWord(evoEntry, evosWritten * 6 + 4, evo.To.Id);
                    evosWritten++;
                    if (evosWritten == 7)
                        break;
                }
                while (evosWritten < 7)
                {
                    PpTxtHandler.WriteWord(evoEntry, evosWritten * 6, 0);
                    PpTxtHandler.WriteWord(evoEntry, evosWritten * 6 + 2, 0);
                    PpTxtHandler.WriteWord(evoEntry, evosWritten * 6 + 4, 0);
                    evosWritten++;
                }
            }
            BaseRom.WriteFile(_romEntry.GetString("PokemonEvolutions"), _evoNarc.Bytes);
        }


        private void LoadTrainerNames()
        {
            // blank one
            var tnames = GetStrings(false, _romEntry.GetInt("TrainerNamesTextOffset")).SliceFrom(1);

            // Tack the mugshot names on the end

            TrainerNames =
                tnames.Concat(
                        _mnames.Where(mname => !mname.IsEmpty() && mname[0] >= 'A' && mname[0] <= 'Z')
                    )
                    .Slice();
        }

        private void SaveTrainerNames()
        {
            // Grab the mugshot names off the back of the list of trainer names
            // we got back
            var trNamesSize = TrainerNames.Length;
            for (var i = _mnames.Length - 1; i >= 0; i--)
            {
                var origMName = _mnames[i];

                if (origMName.IsEmpty() || origMName[0] < 'A' || origMName[0] > 'Z')
                    continue;

                // Grab replacement
                var replacement = TrainerNames[--trNamesSize];
                _mnames[i] = replacement;
            }
            // Save back mugshot names
            SetStrings(false, _romEntry.GetInt("TrainerMugshotsTextOffset"), _mnames);

            // Now save the rest of trainer names
            var tnames = GetStrings(false, _romEntry.GetInt("TrainerNamesTextOffset"));

            for (var i = 1; i < tnames.Length; i++)
                tnames[i] = TrainerNames[i - 1];

            SetStrings(false, _romEntry.GetInt("TrainerNamesTextOffset"), tnames);
        }


        private void LoadDoublesTrainerClasses()
        {
            var doublesClasses = _romEntry.ArrayEntries["DoublesTrainerClasses"];

            var doublesTrainerClasses = new int[doublesClasses.Length];

            for (var i = 0; i < doublesTrainerClasses.Length; i++)
                doublesTrainerClasses[i] = doublesClasses[i];

            DoublesTrainerClasses = doublesTrainerClasses;
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

            fieldItems.AddRange(
                Load(
                    _scriptNarc.Files[_romEntry.GetInt("ItemBallsScriptOffset")], 
                    _romEntry.ArrayEntries["ItemBallsSkip"], 
                    Gen5Constants.NormalItemSetVarCommand
                )
            );

            fieldItems.AddRange(
                Load(
                    _scriptNarc.Files[_romEntry.GetInt("HiddenItemsScriptOffset")], 
                    _romEntry.ArrayEntries["HiddenItemsSkip"], 
                    Gen5Constants.HiddenItemSetVarCommand
                )
            );

            FieldItems = fieldItems.ToArray();

            RegularFieldItems = FieldItems
                .Where(item => Gen5Constants.AllowedItems.IsAllowed(item) && !Gen5Constants.AllowedItems.IsTm(item))
                .ToArray();

            CurrentFieldTMs = FieldItems
                .Where(Gen5Constants.AllowedItems.IsTm)
                .Select(TmFromIndex)
                .ToArray();

            IEnumerable<int> Load(IList<byte> script, IList<int> skipTable, int setVar)
            {
                var offset = 0;
                var skipTableOffset = 0;
                
                while (TryAdvanceFieldItemOffset(script, ref offset, out var offsetInFile))
                {
                    if (!TryAdvanceSkipTableOffset(skipTable, offset, ref skipTableOffset))
                        continue;

                    var command = PpTxtHandler.ReadWord(script, offsetInFile + 2);
                    var variable = PpTxtHandler.ReadWord(script, offsetInFile + 4);


                    if (command != setVar || variable != Gen5Constants.NormalItemVarSet)
                        continue;

                    var item = PpTxtHandler.ReadWord(script, offsetInFile + 6);
                    yield return item;
                }
            }
        }

        private bool TryAdvanceFieldItemOffset(IList<byte> script, ref int offset, out int offsetInFile)
        {

            var part1 = PpTxtHandler.ReadWord(script, offset);
            offsetInFile = 0;

            if (part1 == Gen5Constants.ScriptListTerminator)
                return false;

            offsetInFile = ReadRelativePointer(script, offset);
            offset += 4;

            return offsetInFile <= script.Count;
        }

        private static bool TryAdvanceSkipTableOffset(IList<int> skipTable, int offset, ref int skipTableOffset)
        {
            if (skipTableOffset >= skipTable.Count || skipTable[skipTableOffset] != offset / 4 - 1)
                return true;

            skipTableOffset++;
            return false;
        }

        private void SaveFieldItems()
        {
            // normal items
            var scriptFileNormal = _romEntry.GetInt("ItemBallsScriptOffset");
            var scriptFileHidden = _romEntry.GetInt("HiddenItemsScriptOffset");

            using (var iterItems = ((IEnumerable<int>) FieldItems).GetEnumerator())
            {
                Apply(iterItems, _scriptNarc.Files[scriptFileNormal], _romEntry.ArrayEntries["ItemBallsSkip"]);
                Apply(iterItems, _scriptNarc.Files[scriptFileHidden], _romEntry.ArrayEntries["HiddenItemsSkip"]);
            }

            void Apply(IEnumerator<int> iterItems, IList<byte> script, IList<int> skipTable)
            {
                var offset = 0;
                var skipTableOffset = 0;

                while (TryAdvanceFieldItemOffset(script, ref offset, out var offsetInFile))
                {
                    if (!TryAdvanceSkipTableOffset(skipTable, offset, ref skipTableOffset))
                        continue;

                    var command = PpTxtHandler.ReadWord(script, offsetInFile + 2);
                    var variable = PpTxtHandler.ReadWord(script, offsetInFile + 4);

                    if (command != Gen5Constants.NormalItemSetVarCommand || variable != Gen5Constants.NormalItemVarSet)
                        continue;

                    iterItems.MoveNext();
                    var item = iterItems.Current;

                    PpTxtHandler.WriteWord(script, offsetInFile + 6, item);
                }
            }
        }

        private int TmFromIndex(int index)
        {
            if (index >= Gen5Constants.TmBlockOneOffset &&
                index < Gen5Constants.TmBlockOneOffset + Gen5Constants.TmBlockOneCount)
                return index - (Gen5Constants.TmBlockOneOffset - 1);
            return index + Gen5Constants.TmBlockOneCount - (Gen5Constants.TmBlockTwoOffset - 1);
        }

        private int IndexFromTm(int tm)
        {
            if (tm >= 1 && tm <= Gen5Constants.TmBlockOneCount)
                return tm + (Gen5Constants.TmBlockOneOffset - 1);

            return tm + (Gen5Constants.TmBlockTwoOffset - 1 - Gen5Constants.TmBlockOneCount);
        }

        private void LoadIngameTrades()
        {
            var unused = _romEntry.ArrayEntries["TradesUnused"];
            var unusedOffset = 0;
            var tableSize = _tradeNarc.Files.Count;

            var trades = new List<IngameTrade>();

            for (var entry = 0; entry < tableSize; entry++)
            {
                if (unusedOffset < unused.Length && unused[unusedOffset] == entry)
                {
                    unusedOffset++;
                    continue;
                }

                var tfile = _tradeNarc.Files[entry];
                var trade = new IngameTrade
                {
                    Nickname = _tradeStrings[entry * 2],
                    GivenPokemon = AllPokemons[PpTxtHandler.ReadInt(tfile, 4)],
                    Ivs = new int[6],
                    OtId = PpTxtHandler.ReadWord(tfile, 0x34),
                    Item = PpTxtHandler.ReadInt(tfile, 0x4C),
                    OtName = _tradeStrings[entry * 2 + 1],
                    RequestedPokemon = AllPokemons[PpTxtHandler.ReadInt(tfile, 0x5C)]
                };

                for (var iv = 0; iv < 6; iv++)
                {
                    trade.Ivs[iv] = PpTxtHandler.ReadInt(tfile, 0x10 + iv * 4);
                }

                trades.Add(trade);
            }

            IngameTrades = trades.ToArray();
        }

        private void SaveIngameTrades()
        {
            // info
            var tradeOffset = 0;

            var tradeCount = _tradeNarc.Files.Count;
            var unused = _romEntry.ArrayEntries["TradesUnused"];
            var unusedOffset = 0;
            for (var i = 0; i < tradeCount; i++)
            {
                if (unusedOffset < unused.Length && unused[unusedOffset] == i)
                {
                    unusedOffset++;
                    continue;
                }

                var tfile = _tradeNarc.Files[i];
                var trade = IngameTrades[tradeOffset++];
                _tradeStrings[i * 2] = trade.Nickname;
                _tradeStrings[i * 2 + 1] = trade.OtName;
                PpTxtHandler.WriteInt(tfile, 4, trade.GivenPokemon.Id);
                PpTxtHandler.WriteInt(tfile, 8, 0);
                for (var iv = 0; iv < 6; iv++)
                {
                    PpTxtHandler.WriteInt(tfile, 0x10 + iv * 4, trade.Ivs[iv]);
                }
                PpTxtHandler.WriteInt(tfile, 0x2C, 0xFF);
                PpTxtHandler.WriteWord(tfile, 0x34, trade.OtId);
                PpTxtHandler.WriteInt(tfile, 0x4C, trade.Item);
                PpTxtHandler.WriteInt(tfile, 0x5C, trade.RequestedPokemon.Id);
            }

            BaseRom.WriteFile(_romEntry.GetString("InGameTrades"), _tradeNarc.Bytes);
            SetStrings(false, _romEntry.GetInt("IngameTradesTextOffset"), _tradeStrings);
        }

        private void LoadPokemonSprites()
        {
            foreach (var pokemon in AllPokemons)
            {
                // First prepare the palette, it's the easy bit
                var rawPalette = _pokespritesNarc.Files[pokemon.Id * 20 + 18];
                var palette = new int[16];
                for (var j = 1; j < 16; j++)
                    palette[j] = GfxFunctions.Conv16BitColorToArgb(PpTxtHandler.ReadWord(rawPalette, 40 + j * 2));

                // Get the picture and uncompress it.
                var compressedPic = _pokespritesNarc.Files[pokemon.Id * 20];
                var uncompressedPic = DsDecmp.Decompress(compressedPic);

                // Output to 64x144 tiled image to prepare for unscrambling
                pokemon.Sprite = GfxFunctions.DrawTiledImage(uncompressedPic, palette, 48, 64, 144, 4);
            }
        }

        private void LoadHiddenHollow()
        {
            if (!IsBw2)
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
                        Pokemon = PpTxtHandler.ReadWord(data, version * 78 + rarityslot * 26 + @group * 2)
                    };

                    _hiddenHollows.Add(entry, hiddenHollow);
                }
            }
        }

        private void SaveHiddenHollow()
        {
            if (!IsBw2)
                return;

            foreach (var pair in _hiddenHollows)
            {
                var entry = pair.Key;
                var hiddenHollow = pair.Value;
                var data = _hhNarc.Files[entry.Index];

                PpTxtHandler.WriteWord(data, entry.Version * 78 + entry.Rarityslot * 26 + entry.Group * 2, hiddenHollow.Pokemon);
                data[entry.Version * 78 + entry.Rarityslot * 26 + 16 + entry.Group] = hiddenHollow.GenderRatio;
                data[entry.Version * 78 + entry.Rarityslot * 26 + 20 + entry.Group] = hiddenHollow.Form;
            }

            BaseRom.WriteFile(_romEntry.GetString("HiddenHollows"), _hhNarc.Bytes);
        }

        public static bool IsLoadable(string filename) => EntryFor(GetRomCodeFromFile(filename)) != null;

        private class HiddenHollowEntry
        {
            public int Group { get; }

            public int Index { get; }
            public int Rarityslot { get; }
            public int Version { get; }

            public HiddenHollowEntry(int index, int version, int rarityslot, int group)
            {
                Index = index;
                Version = version;
                Rarityslot = rarityslot;
                Group = group;
            }

            private bool Equals(HiddenHollowEntry other) => Index == other.Index &&
                                                            Version == other.Version &&
                                                            Rarityslot == other.Rarityslot &&
                                                            Group == other.Group;

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                    return false;
                if (ReferenceEquals(this, obj))
                    return true;
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

        private class OffsetWithinEntry
        {
            public int Entry { get; set; }
            public int Offset { get; set; }
        }

        private class Gen5StaticPokemon
        {
            private readonly IList<int> _files;
            private readonly IList<int> _offsets;

            public Gen5StaticPokemon(IList<int> files, IList<int> offset)
            {
                _files = files;
                _offsets = offset;
            }

            public Pokemon GetPokemon(Gen5RomHandler parent, NarcArchive scriptNarc) => parent.AllPokemons[
                PpTxtHandler.ReadWord(scriptNarc.Files[_files[0]], _offsets[0])];

            public void SetPokemon(Gen5RomHandler parent, NarcArchive scriptNarc, Pokemon pkmn)
            {
                var value = pkmn.Id;
                for (var i = 0; i < _offsets.Count; i++)
                {
                    var file = scriptNarc.Files[_files[i]];
                    PpTxtHandler.WriteWord(file, _offsets[i], value);
                }
            }
        }

        private class RomEntry
        {
            public Dictionary<string, int[]> ArrayEntries { get; } =
                new Dictionary<string, int[]>();

            public Dictionary<string, int> Numbers { get; } = new Dictionary<string, int>();

            public Dictionary<string, OffsetWithinEntry[]> OffsetArrayEntries { get; } =
                new Dictionary<string, OffsetWithinEntry[]>();

            public List<Gen5StaticPokemon> Pokemon { get; } = new List<Gen5StaticPokemon>();
            public Dictionary<string, string> Strings { get; } = new Dictionary<string, string>();
            public Dictionary<string, string> TweakFiles { get; } = new Dictionary<string, string>();

            public string Code { get; set; }
            public bool CopyStaticPokemon { get; set; }

            public string Name { get; set; }
            public int RomType { get; set; }
            public bool StaticPokemonSupport { get; set; }

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