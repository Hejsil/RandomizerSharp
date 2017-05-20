using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RandomizerSharp.Constants;
using RandomizerSharp.PokemonModel;
using RandomizerSharp.RomHandlers;

namespace RandomizerSharp.Randomizers
{
    public class Randomizer
    {
        private readonly StreamWriter _logStream;
        private readonly AbstractRomHandler _romHandler;
        private readonly Random _random;

        public Randomizer(AbstractRomHandler romHandler, StreamWriter logStream)
        {
            _romHandler = romHandler;
            _logStream = logStream;
            _random = new Random();
        }

        public Randomizer(AbstractRomHandler romHandler, StreamWriter logStream, int seed)
        {
            _romHandler = romHandler;
            _logStream = logStream;
            _random = new Random(seed);
        }

        public Pokemon RandomPokemon()
        {
            var pokemons = _romHandler.ValidPokemons;
            return pokemons[_random.Next(pokemons.Count)];
        }

        public Pokemon RandomNonLegendaryPokemon()
        {
            var nonLegendaries = _romHandler.NonLegendaryPokemons;
            return nonLegendaries[_random.Next(nonLegendaries.Count)];
        }

        public Pokemon RandomLegendaryPokemon()
        {
            var legendaries = _romHandler.LegendaryPokemons;
            return legendaries[_random.Next(legendaries.Count)];
        }

        public void RandomizeMovePowers()
        {
            foreach (var mv in _romHandler.ValidMoves)
            {
                if (mv.InternalId == Move.StruggleId || mv.Power < 10) continue;

                //  "Generic" damaging move to randomize power
                if (_random.Next(3) != 2) mv.Power = _random.Next(11) * 5 + 50;
                else mv.Power = _random.Next(27) * 5 + 20;

                //  Tiny chance for massive power jumps
                for (var i = 0; i < 2; i++) if (_random.Next(100) == 0) mv.Power += 50;

                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (mv.HitCount == 1) continue;

                //  Divide randomized power by average hit count, round to
                //  nearest 5
                mv.Power = (int) (Math.Round(mv.Power / (mv.HitCount / 5)) * 5);
                if (mv.Power < 5) mv.Power = 5;
            }
        }

        public void RandomizePokemonStats(bool evolutionSanity)
        {
            if (evolutionSanity)
            {
                CopyUpEvolutionsHelper(
                    RandomizeStatsWithinBst,
                    CopyRandomizedStatsUpEvolution
                );
            }
            else
            {
                var allPokes = _romHandler.ValidPokemons;
                foreach (var pk in allPokes) { if (pk != null) RandomizeStatsWithinBst(pk); }
            }

            void RandomizeStatsWithinBst(Pokemon pokemon)
            {
                int baseStat;
                double totW;

                var hpW = _random.NextDouble();
                var atkW = _random.NextDouble();
                var defW = _random.NextDouble();
                var spaW = _random.NextDouble();
                var spdW = _random.NextDouble();
                var speW = _random.NextDouble();

                if (pokemon.Id == Pokemon.ShedinjaNumber)
                {
                    baseStat = pokemon.Bst() - 51;
                    totW = atkW + defW + spaW + spdW + speW;
                    pokemon.Hp = 1;
                }
                else
                {
                    baseStat = pokemon.Bst() - 70;
                    totW = hpW + atkW + defW + spaW + spdW + speW;
                    pokemon.Hp = (int) Math.Max(1, Math.Round(hpW / totW * baseStat)) + 20;
                }

                pokemon.Attack = (int) Math.Max(1, Math.Round(atkW / totW * baseStat)) + 10;
                pokemon.Defense = (int) Math.Max(1, Math.Round(defW / totW * baseStat)) + 10;
                pokemon.Spatk = (int) Math.Max(1, Math.Round(spaW / totW * baseStat)) + 10;
                pokemon.Spdef = (int) Math.Max(1, Math.Round(spdW / totW * baseStat)) + 10;
                pokemon.Speed = (int) Math.Max(1, Math.Round(speW / totW * baseStat)) + 10;
                pokemon.Special = (int) Math.Ceiling((pokemon.Spatk + pokemon.Spdef) / 2.0f);

                if (pokemon.Hp > 255 ||
                    pokemon.Attack > 255 ||
                    pokemon.Defense > 255 ||
                    pokemon.Spatk > 255 ||
                    pokemon.Spdef > 255 ||
                    pokemon.Speed > 255) RandomizeStatsWithinBst(pokemon);
            }

            void CopyRandomizedStatsUpEvolution(Pokemon evTo, Pokemon evolvesFrom, bool toMonIsFinalEvo)
            {
                double ourBst = evTo.Bst();
                double theirBst = evolvesFrom.Bst();
                var bstRatio = ourBst / theirBst;
                evTo.Hp = (int) Math.Min(255, Math.Max(1, Math.Round(evolvesFrom.Hp * bstRatio)));
                evTo.Attack = (int) Math.Min(255, Math.Max(1, Math.Round(evolvesFrom.Attack * bstRatio)));
                evTo.Defense = (int) Math.Min(255, Math.Max(1, Math.Round(evolvesFrom.Defense * bstRatio)));
                evTo.Speed = (int) Math.Min(255, Math.Max(1, Math.Round(evolvesFrom.Speed * bstRatio)));
                evTo.Spatk = (int) Math.Min(255, Math.Max(1, Math.Round(evolvesFrom.Spatk * bstRatio)));
                evTo.Spdef = (int) Math.Min(255, Math.Max(1, Math.Round(evolvesFrom.Spdef * bstRatio)));
                evTo.Special = (int) Math.Ceiling((evTo.Spatk + evTo.Spdef) / 2.0f);
            }
        }

        public void ShufflePokemonStats(bool evolutionSanity)
        {
            if (evolutionSanity)
            {
                CopyUpEvolutionsHelper(
                    pokemon => pokemon.ShuffleStats(_random),
                    (evFrom, evTo, toMonIsFinalEvo) => evTo.CopyShuffledStatsUpEvolution(evFrom)
                );
            }
            else
            {
                var allPokes = _romHandler.ValidPokemons;
                foreach (var pk in allPokes) pk?.ShuffleStats(_random);
            }
        }

        public void LevelModifyTrainers(int levelModifier)
        {
            //  Fully random is easy enough - randomize then worry about rival
            //  carrying starter at the end
            foreach (var t in _romHandler.Trainers)
            {
                foreach (var tp in t.Pokemon)
                {
                    if (levelModifier != 0)
                        tp.Level = Math.Min(100, (int) Math.Round(tp.Level * (1 + levelModifier / 100.0)));
                }
            }
        }

        public void RandomizeTrainerPokes(bool usePowerLevels, bool noLegendaries, bool noEarlyWonderGuard, bool rivaleCarriesStarter)
        {
            //  Fully random is easy enough - randomize then worry about rival
            //  carrying starter at the end
            foreach (var t in _romHandler.Trainers)
            {
                //if (t.Tag != null && t.Tag.Equals("IRIVAL"))
                //    continue;

                foreach (var tp in t.Pokemon)
                {
                    var wgAllowed = !noEarlyWonderGuard || tp.Level >= 20;
                    tp.Pokemon = PickReplacement(tp.Pokemon, usePowerLevels, null, noLegendaries, wgAllowed);
                    tp.ResetMoves = true;
                }
            }

            if (rivaleCarriesStarter)
                RivalCarriesStarter();
        }

        public enum Encounters
        {
            CatchEmAll,
            TypeThemed,
            UsePowerLevel,
        }

        public void RandomEncounters(Encounters encounters, bool noLegendaries)
        {
            var currentEncounters = _romHandler.Encounters.ToList();
            currentEncounters.Shuffle(_random);

            var banned = _romHandler.BannedForWildEncounters;

            if (encounters == Encounters.CatchEmAll)
            {
                // Clone, so we don't modify original
                var allPokes = noLegendaries ? _romHandler.NonLegendaryPokemons.ToList() : _romHandler.ValidPokemons.ToList();
                allPokes.RemoveAll(banned);

                foreach (var area in currentEncounters)
                {
                    var pickablePokemon = allPokes;

                    if (area.BannedPokemon.Count > 0)
                    {
                        // Clone, so we don't modify original
                        pickablePokemon = allPokes.ToList();
                        pickablePokemon.RemoveAll(area.BannedPokemon);
                    }

                    foreach (var enc in area.Encounters)
                    {
                        if (pickablePokemon.Count == 0)
                        {
                            // Clone, so we don't modify original
                            var tempPickable = noLegendaries ? _romHandler.NonLegendaryPokemons.ToList() : _romHandler.ValidPokemons.ToList();
                            tempPickable.RemoveAll(banned);
                            tempPickable.RemoveAll(area.BannedPokemon);

                            if (tempPickable.Count == 0)
                                throw new NotImplementedException("ERROR: Couldn't replace a wild Pokemon!");
                            
                            enc.Pokemon = tempPickable[_random.Next(tempPickable.Count)];
                        }
                        else
                        {
                            var picked = _random.Next(pickablePokemon.Count);
                            enc.Pokemon = pickablePokemon[picked];

                            pickablePokemon.RemoveAt(picked);

                            if (allPokes != pickablePokemon)
                                allPokes.Remove(enc.Pokemon);

                            if (allPokes.Count != 0)
                                continue;

                            if (noLegendaries)
                                allPokes.AddRange(_romHandler.NonLegendaryPokemons);
                            else
                                allPokes.AddRange(_romHandler.ValidPokemons);

                            allPokes.RemoveAll(banned);

                            if (pickablePokemon == allPokes)
                                continue;

                            pickablePokemon.AddRange(allPokes);

                            pickablePokemon.RemoveAll(area.BannedPokemon);
                        }
                    }
                }
            }
            else if (encounters == Encounters.TypeThemed)
            {
                var cachedPokeLists = new SortedDictionary<Typing, IList<Pokemon>>();
                foreach (var area in currentEncounters)
                {
                    IList<Pokemon> possiblePokemon = null;
                    var iterLoops = 0;
                    while (possiblePokemon == null && iterLoops < 10000)
                    {
                        var areaTheme = RandomType();
                        if (!cachedPokeLists.ContainsKey(areaTheme))
                        {
                            IList<Pokemon> pType = PokemonOfType(areaTheme, noLegendaries);

                            pType.RemoveAll(banned);
                            cachedPokeLists[areaTheme] = pType;
                        }

                        possiblePokemon = cachedPokeLists[areaTheme];
                        if (area.BannedPokemon.Count > 0)
                        {
                            possiblePokemon = possiblePokemon.ToList();
                            possiblePokemon.RemoveAll(area.BannedPokemon);
                        }
                        if (possiblePokemon.Count == 0)
                        {
                            possiblePokemon = null;
                        }
                        iterLoops++;
                    }
                    if (possiblePokemon == null)
                    {
                        throw new NotImplementedException("Could not randomize an area in a reasonable amount of attempts.");
                    }
                    foreach (var enc in area.Encounters)
                    {
                        enc.Pokemon = possiblePokemon[_random.Next(possiblePokemon.Count)];
                    }
                }
            }
            else if (encounters == Encounters.UsePowerLevel)
            {
                var allowedPokes = noLegendaries ? _romHandler.NonLegendaryPokemons.ToList() : _romHandler.ValidPokemons.ToList();
                allowedPokes.RemoveAll(banned);
                
                foreach (var area in currentEncounters)
                {
                    var localAllowed = allowedPokes;
                    if (area.BannedPokemon.Count > 0)
                    {
                        localAllowed = allowedPokes.ToList();
                        localAllowed.RemoveAll(area.BannedPokemon);
                    }
                    foreach (var enc in area.Encounters)
                    {
                        enc.Pokemon = PickWildPowerLvlReplacement(localAllowed, enc.Pokemon, false, null);
                    }
                }
            }
            else
            {
                foreach (var area in currentEncounters)
                {
                    foreach (var enc in area.Encounters)
                    {
                        enc.Pokemon = noLegendaries ? RandomNonLegendaryPokemon() : RandomPokemon();
                        while (banned.Contains(enc.Pokemon) || area.BannedPokemon.Contains(enc.Pokemon))
                        {
                            enc.Pokemon = noLegendaries ? RandomNonLegendaryPokemon() : RandomPokemon();
                        }
                    }
                }
            }
        }



        private Pokemon PickReplacement(
            Pokemon current,
            bool usePowerLevels,
            Typing type,
            bool noLegendaries,
            bool wonderGuardAllowed)
        {
            var pickFrom = _romHandler.ValidPokemons;

            if (type != null)
                pickFrom = pickFrom.Where(pk => pk.PrimaryType.Equals(type) || pk.SecondaryType.Equals(type)).ToArray();
            if (noLegendaries) pickFrom = pickFrom.Where(pk => !pk.Legendary).ToArray();

            if (usePowerLevels)
            {
                //  start with within 10% and add 5% either direction till we find
                //  something
                var currentBst = current.BstForPowerLevels();
                var minTarget = currentBst - currentBst / 10;
                var maxTarget = currentBst + currentBst / 10;
                var canPick = new List<Pokemon>();
                var expandRounds = 0;

                while (canPick.Count == 0 || canPick.Count < 3 && expandRounds < 2)
                {
                    foreach (var pk in pickFrom)
                    {
                        if (pk == null) continue;

                        var hasWonderGuard = pk.Ability1 == GlobalConstants.WonderGuardIndex ||
                                             pk.Ability2 == GlobalConstants.WonderGuardIndex ||
                                             pk.Ability3 == GlobalConstants.WonderGuardIndex;

                        if (pk.BstForPowerLevels() >= minTarget &&
                            pk.BstForPowerLevels() <= maxTarget &&
                            (wonderGuardAllowed || hasWonderGuard)) canPick.Add(pk);
                    }

                    minTarget = minTarget - currentBst / 20;
                    maxTarget = maxTarget + currentBst / 20;
                    expandRounds++;
                }

                return canPick[_random.Next(canPick.Count)];
            }

            if (wonderGuardAllowed) { return pickFrom[_random.Next(pickFrom.Count)]; }

            {
                var pk = pickFrom[_random.Next(pickFrom.Count)];

                while (pk.Ability1 == GlobalConstants.WonderGuardIndex ||
                       pk.Ability2 == GlobalConstants.WonderGuardIndex ||
                       pk.Ability3 == GlobalConstants.WonderGuardIndex) { pk = pickFrom[_random.Next(pickFrom.Count)]; }

                return pk;
            }
        }

        private void CopyUpEvolutionsHelper(Action<Pokemon> bpAction, Action<Pokemon, Pokemon, bool> epAction)
        {
            var allPokes = _romHandler.ValidPokemons;
            foreach (var pk in allPokes) if (pk != null) pk.TemporaryFlag = false;

            //  Get evolution data.
            var dontCopyPokes = RomFunctions.GetBasicOrNoCopyPokemon(_romHandler);
            var middleEvos = RomFunctions.GetMiddleEvolutions(_romHandler);
            foreach (var pk in dontCopyPokes)
            {
                bpAction(pk);
                pk.TemporaryFlag = true;
            }

            //  go "up" evolutions looking for pre-evos to do first
            foreach (var pk in allPokes)
                if (pk != null && !pk.TemporaryFlag)
                {
                    //  Non-randomized pokes at this point must have
                    //  a linear chain of single evolutions down to
                    //  a randomized poke.
                    var currentStack = new Stack<Evolution>();
                    var ev = pk.EvolutionsTo[0];
                    while (!ev.From.TemporaryFlag)
                    {
                        currentStack.Push(ev);
                        ev = ev.From.EvolutionsTo[0];
                    }

                    //  Now "ev" is set to an evolution from a Pokemon that has had
                    //  the base action done on it to one that hasn't.
                    //  Do the evolution action for everything left on the stack.
                    epAction(ev.From, ev.To, !middleEvos.Contains(ev.To));
                    ev.To.TemporaryFlag = true;
                    while (currentStack.Count != 0)
                    {
                        ev = currentStack.Pop();
                        epAction(ev.From, ev.To, !middleEvos.Contains(ev.To));
                        ev.To.TemporaryFlag = true;
                    }
                }
        }

        public Typing RandomType()
        {
            var t = Typing.RandomType(_random);
            while (!_romHandler.TypeInGame(t)) t = Typing.RandomType(_random);

            return t;
        }

        public void RandomizePokemonTypes(bool evolutionSanity)
        {
            var allPokes = _romHandler.ValidPokemons;
            if (evolutionSanity)
            {
                CopyUpEvolutionsHelper(
                    RandomizePokemonType,
                    RandomizeEvolutionTypes);
            }
            else
            {
                foreach (var pkmn in allPokes)
                    if (pkmn != null)
                    {
                        pkmn.PrimaryType = RandomType();
                        pkmn.SecondaryType = null;

                        if (!(_random.NextDouble() < 0.5)) continue;

                        pkmn.SecondaryType = RandomType();
                        while (pkmn.SecondaryType == pkmn.PrimaryType) pkmn.SecondaryType = RandomType();
                    }
            }

            void RandomizeEvolutionTypes(Pokemon evFrom, Pokemon evTo, bool toMonIsFinalEvo)
            {
                evTo.PrimaryType = evFrom.PrimaryType;
                evTo.SecondaryType = evFrom.SecondaryType;

                if (evTo.SecondaryType == null)
                {
                    var chance = toMonIsFinalEvo ? 0.25 : 0.15;
                    if (_random.NextDouble() < chance)
                    {
                        evTo.SecondaryType = RandomType();
                        while (evTo.SecondaryType == evTo.PrimaryType) evTo.SecondaryType = RandomType();
                    }
                }
            }

            void RandomizePokemonType(Pokemon pokemon)
            {
                // Step 1: Basic or Excluded From Copying Pokemon
                // A Basic/EFC pokemon has a 35% chance of a second type if
                // it has an evolution that copies type/stats, a 50% chance
                // otherwise
                pokemon.PrimaryType = RandomType();
                pokemon.SecondaryType = null;
                if (pokemon.EvolutionsFrom.Count == 1 &&
                    pokemon.EvolutionsFrom[0].CarryStats)
                {
                    if (_random.NextDouble() < 0.35)
                    {
                        pokemon.SecondaryType = RandomType();
                        while (pokemon.SecondaryType == pokemon.PrimaryType) pokemon.SecondaryType = RandomType();
                    }
                }
                else
                {
                    if (_random.NextDouble() < 0.5)
                    {
                        pokemon.SecondaryType = RandomType();
                        while (pokemon.SecondaryType == pokemon.PrimaryType) pokemon.SecondaryType = RandomType();
                    }
                }
            }
        }

        private List<Pokemon> PokemonOfType(Typing type, bool noLegendaries)
        {
            return _romHandler.ValidPokemons
                .Where(
                    pk => pk != null &&
                          (!noLegendaries || !pk.Legendary) &&
                          (pk.PrimaryType == type || pk.SecondaryType == type))
                .ToList();
        }

        private Pokemon PickWildPowerLvlReplacement(
            List<Pokemon> pokemonPool,
            Pokemon current,
            bool banSamePokemon,
            List<Pokemon> usedUp)
        {
            //  start with within 10% and add 5% either direction till we find
            //  something
            var currentBst = current.BstForPowerLevels();
            var minTarget = currentBst - currentBst / 10;
            var maxTarget = currentBst + currentBst / 10;
            var canPick = new List<Pokemon>();
            var expandRounds = 0;
            while (canPick.Count == 0 ||
                   canPick.Count < 3 && expandRounds < 3)
            {
                foreach (var pk in pokemonPool)
                    if (pk.BstForPowerLevels() >= minTarget &&
                        pk.BstForPowerLevels() <= maxTarget &&
                        (!banSamePokemon || !ReferenceEquals(pk, current)) &&
                        (usedUp == null || !usedUp.Contains(pk)) &&
                        !canPick.Contains(pk)) canPick.Add(pk);

                minTarget = minTarget - currentBst / 20;
                maxTarget = maxTarget + currentBst / 20;
                expandRounds++;
            }

            return canPick[_random.Next(canPick.Count)];
        }

        public void RandomizeMoveTypes()
        {
            foreach (var mv in _romHandler.ValidMoves)
                if (mv.InternalId != Move.StruggleId && mv.Type != null) mv.Type = RandomType();
        }


        public void TypeThemeTrainerPokes(
            bool usePowerLevels,
            bool weightByFrequency,
            bool noLegendaries,
            bool noEarlyWonderGuard,
            int levelModifier)
        {
            var currentTrainers = _romHandler.Trainers;

            //  Construct groupings for types
            //  Anything starting with GYM or ELITE or CHAMPION is a group
            var assignedTrainers = new HashSet<Trainer>();
            var groups = new Dictionary<string, List<Trainer>>();
            foreach (var t in currentTrainers)
            {
                if (t.Tag != null &&
                    t.Tag.Equals("IRIVAL")) continue;

                var group = t.Tag ?? "";

                if (group.Contains("-")) group = group.Substring(0, group.IndexOf('-'));

                if (group.StartsWith("GYM") ||
                    group.StartsWith("ELITE") ||
                    group.StartsWith("CHAMPION") ||
                    group.StartsWith("THEMED"))
                {
                    //  Yep this is a group
                    if (!groups.ContainsKey(group)) groups[group] = new List<Trainer>();

                    groups[group].Add(t);
                    assignedTrainers.Add(t);
                }
                else if (group.StartsWith("GIO"))
                {
                    //  Giovanni has same grouping as his gym, gym 8
                    if (!groups.ContainsKey("GYM8")) groups["GYM8"] = new List<Trainer>();

                    groups["GYM8"].Add(t);
                    assignedTrainers.Add(t);
                }
            }

            //  Give a type to each group
            //  Gym & elite types have to be unique
            //  So do uber types, including the type we pick for champion
            var usedGymTypes = new HashSet<Typing>();
            var usedEliteTypes = new HashSet<Typing>();
            var usedUberTypes = new HashSet<Typing>();
            foreach (var group in groups.Keys)
            {
                var trainersInGroup = groups[group];
                //  Shuffle ordering within group to promote randomness
                trainersInGroup.Shuffle(_random);
                var typeForGroup = RandomType();
                if (group.StartsWith("GYM"))
                {
                    while (usedGymTypes.Contains(typeForGroup)) typeForGroup = RandomType();

                    usedGymTypes.Add(typeForGroup);
                }

                if (group.StartsWith("ELITE"))
                {
                    while (usedEliteTypes.Contains(typeForGroup)) typeForGroup = RandomType();

                    usedEliteTypes.Add(typeForGroup);
                }

                if (group.Equals("CHAMPION")) usedUberTypes.Add(typeForGroup);

                //  Themed groups just have a theme, no special criteria
                foreach (var t in trainersInGroup)
                foreach (var tp in t.Pokemon)
                {
                    var wgAllowed = !noEarlyWonderGuard || tp.Level >= 20;
                    tp.Pokemon = PickReplacement(
                        tp.Pokemon,
                        usePowerLevels,
                        typeForGroup,
                        noLegendaries,
                        wgAllowed);
                    tp.ResetMoves = true;
                    if (levelModifier != 0)
                        tp.Level = Math.Min(100, (int) Math.Round(tp.Level * (1 + levelModifier / 100.0)));
                }
            }

            //  New: randomize the order trainers are randomized in.
            //  Leads to less predictable results for various modifiers.
            //  Need to keep the original ordering around for saving though.
            var scrambledTrainers = new List<Trainer>(currentTrainers);
            scrambledTrainers.Shuffle(_random);
            //  Give a type to each unassigned trainer
            foreach (var t in scrambledTrainers)
            {
                if (t.Tag != null &&
                    t.Tag.Equals("IRIVAL")) continue;

                if (!assignedTrainers.Contains(t))
                {
                    var typeForTrainer = RandomType();
                    //  Ubers: can't have the same type as each other
                    if (t.Tag != null &&
                        t.Tag.Equals("UBER"))
                    {
                        while (usedUberTypes.Contains(typeForTrainer)) typeForTrainer = RandomType();

                        usedUberTypes.Add(typeForTrainer);
                    }

                    foreach (var tp in t.Pokemon)
                    {
                        var shedAllowed = !noEarlyWonderGuard || tp.Level >= 20;
                        tp.Pokemon = PickReplacement(
                            tp.Pokemon,
                            usePowerLevels,
                            typeForTrainer,
                            noLegendaries,
                            shedAllowed);
                        tp.ResetMoves = true;
                        if (levelModifier != 0)
                            tp.Level = Math.Min(100, (int) Math.Round(tp.Level * (1 + levelModifier / 100.0)));
                    }
                }
            }
        }


        public void Game1To1Encounters(bool useTimeOfDay, bool usePowerLevels, bool noLegendaries)
        {
            //  Build the full 1-to-1 map
            var translateMap = new Dictionary<Pokemon, Pokemon>();
            var remainingLeft = _romHandler.ValidPokemons.ToList();
            var remainingRight = noLegendaries
                ? new List<Pokemon>(_romHandler.NonLegendaryPokemons)
                : new List<Pokemon>(_romHandler.ValidPokemons);

            var banned = _romHandler.BannedForWildEncounters;

            //  Banned pokemon should be mapped to themselves
            foreach (var bannedPk in banned)
            {
                translateMap[bannedPk] = bannedPk;
                remainingLeft.Remove(bannedPk);
                remainingRight.Remove(bannedPk);
            }

            while (remainingLeft.Count != 0)
            {
                if (usePowerLevels)
                {
                    var pickedLeft = _random.Next(remainingLeft.Count);
                    var pickedLeftP = remainingLeft[pickedLeft];
                    remainingLeft.RemoveAt(pickedLeft);

                    var pickedRightP = remainingRight.Count == 1
                        ? remainingRight[0]
                        : PickWildPowerLvlReplacement(remainingRight, pickedLeftP, true, null);

                    remainingRight.Remove(pickedRightP);
                    translateMap[pickedLeftP] = pickedRightP;
                }
                else
                {
                    var pickedLeft = _random.Next(remainingLeft.Count);
                    var pickedRight = _random.Next(remainingRight.Count);
                    var pickedLeftP = remainingLeft[pickedLeft];
                    var pickedRightP = remainingRight[pickedRight];
                    remainingLeft.RemoveAt(pickedLeft);
                    while (pickedLeftP.Id == pickedRightP.Id &&
                           remainingRight.Count != 1)
                    {
                        //  Reroll for a different pokemon if at all possible
                        pickedRight = _random.Next(remainingRight.Count);
                        pickedRightP = remainingRight[pickedRight];
                    }

                    remainingRight.RemoveAt(pickedRight);
                    translateMap[pickedLeftP] = pickedRightP;
                }

                if (remainingRight.Count == 0)
                {
                    if (noLegendaries) remainingRight.AddRange(_romHandler.NonLegendaryPokemons);
                    else remainingRight.AddRange(_romHandler.ValidPokemons);

                    remainingRight.RemoveAll(banned.Contains);
                }
            }

            //  Map remaining to themselves just in case
            var allPokes = _romHandler.ValidPokemons;
            foreach (var poke in allPokes) if (!translateMap.ContainsKey(poke)) translateMap[poke] = poke;

            foreach (var area in _romHandler.Encounters)
            foreach (var enc in area.Encounters)
            {
                //  Apply the map
                enc.Pokemon = translateMap[enc.Pokemon];
                if (area.BannedPokemon.Contains(enc.Pokemon))
                {
                    //  Ignore the map and put a random non-banned poke
                    var tempPickable = noLegendaries
                        ? new List<Pokemon>(_romHandler.NonLegendaryPokemons)
                        : new List<Pokemon>(_romHandler.ValidPokemons);
                    tempPickable.RemoveAll(banned.Contains);
                    tempPickable.RemoveAll(area.BannedPokemon.Contains);
                    if (tempPickable.Count == 0)
                        throw new NotImplementedException("ERROR: Couldn\'t replace a wild Pokemon!");

                    if (usePowerLevels)
                    {
                        enc.Pokemon = PickWildPowerLvlReplacement(tempPickable, enc.Pokemon, false, null);
                    }
                    else
                    {
                        var picked = _random.Next(tempPickable.Count);
                        enc.Pokemon = tempPickable[picked];
                    }
                }
            }
        }

        public void Area1To1Encounters(
            bool useTimeOfDay,
            bool catchEmAll,
            bool typeThemed,
            bool usePowerLevels,
            bool noLegendaries)
        {
            var banned = _romHandler.BannedForWildEncounters;
            //  New: randomize the order encounter sets are randomized in.
            //  Leads to less predictable results for various modifiers.
            //  Need to keep the original ordering around for saving though.
            var scrambledEncounters = new List<EncounterSet>(_romHandler.Encounters);
            scrambledEncounters.Shuffle(_random);

            //  Assume EITHER catch em all OR type themed for now
            if (catchEmAll)
            {
                var allPokes = noLegendaries
                    ? new List<Pokemon>(_romHandler.NonLegendaryPokemons)
                    : new List<Pokemon>(_romHandler.ValidPokemons);
                allPokes.RemoveAll(banned.Contains);
                foreach (var area in scrambledEncounters)
                {
                    //  Poke-set
                    var inArea = PokemonInArea(area);
                    //  Build area map using catch em all
                    var areaMap = new Dictionary<Pokemon, Pokemon>();
                    var pickablePokemon = allPokes;
                    if (area.BannedPokemon.Count > 0)
                    {
                        pickablePokemon = new List<Pokemon>(allPokes);
                        pickablePokemon.RemoveAll(area.BannedPokemon.Contains);
                    }

                    foreach (var areaPk in inArea)
                        if (pickablePokemon.Count == 0)
                        {
                            //  No more pickable pokes left, take a random one
                            var tempPickable = noLegendaries
                                ? new List<Pokemon>(_romHandler.NonLegendaryPokemons)
                                : new List<Pokemon>(_romHandler.ValidPokemons);

                            tempPickable.RemoveAll(banned.Contains);
                            tempPickable.RemoveAll(area.BannedPokemon.Contains);
                            if (tempPickable.Count == 0)
                                throw new NotImplementedException("ERROR: Couldn\'t replace a wild Pokemon!");

                            var picked = _random.Next(tempPickable.Count);
                            var pickedMn = tempPickable[picked];
                            areaMap[areaPk] = pickedMn;
                        }
                        else
                        {
                            var picked = _random.Next(allPokes.Count);
                            var pickedMn = allPokes[picked];
                            areaMap[areaPk] = pickedMn;
                            pickablePokemon.Remove(pickedMn);
                            if (allPokes != pickablePokemon) allPokes.Remove(pickedMn);

                            if (allPokes.Count == 0)
                            {
                                if (noLegendaries) allPokes.AddRange(_romHandler.NonLegendaryPokemons);
                                else allPokes.AddRange(_romHandler.ValidPokemons);

                                allPokes.AddRange(banned);
                                if (pickablePokemon != allPokes)
                                {
                                    pickablePokemon.AddRange(allPokes);
                                    pickablePokemon.RemoveAll(area.BannedPokemon.Contains);
                                }
                            }
                        }

                    foreach (var enc in area.Encounters)
                        //  Apply the map
                        enc.Pokemon = areaMap[enc.Pokemon];
                }
            }
            else if (typeThemed)
            {
                var cachedPokeLists = new Dictionary<Typing, List<Pokemon>>();
                foreach (var area in scrambledEncounters)
                {
                    //  Poke-set
                    var inArea = PokemonInArea(area);
                    List<Pokemon> possiblePokemon = null;
                    var iterLoops = 0;
                    while (possiblePokemon == null &&
                           iterLoops < 10000)
                    {
                        var areaTheme = RandomType();
                        if (!cachedPokeLists.ContainsKey(areaTheme))
                        {
                            var pType = PokemonOfType(areaTheme, noLegendaries);
                            pType.RemoveAll(banned.Contains);
                            cachedPokeLists[areaTheme] = pType;
                        }

                        possiblePokemon = new List<Pokemon>(cachedPokeLists[areaTheme]);
                        if (area.BannedPokemon.Count > 0) possiblePokemon.RemoveAll(area.BannedPokemon.Contains);

                        if (possiblePokemon.Count < inArea.Count) possiblePokemon = null;

                        iterLoops++;
                    }

                    if (possiblePokemon == null)
                        throw new NotImplementedException(
                            "Could not randomize an area in a reasonable amount of attempts.");

                    //  Build area map using type theme.
                    var areaMap = new Dictionary<Pokemon, Pokemon>();
                    foreach (var areaPk in inArea)
                    {
                        var picked = _random.Next(possiblePokemon.Count);
                        var pickedMn = possiblePokemon[picked];
                        areaMap[areaPk] = pickedMn;
                        possiblePokemon.RemoveAt(picked);
                    }

                    foreach (var enc in area.Encounters)
                        //  Apply the map
                        enc.Pokemon = areaMap[enc.Pokemon];
                }
            }
            else if (usePowerLevels)
            {
                var allowedPokes = noLegendaries
                    ? new List<Pokemon>(_romHandler.NonLegendaryPokemons)
                    : new List<Pokemon>(_romHandler.ValidPokemons);

                allowedPokes.RemoveAll(banned.Contains);
                foreach (var area in scrambledEncounters)
                {
                    //  Poke-set
                    var inArea = PokemonInArea(area);
                    //  Build area map using randoms
                    var areaMap = new Dictionary<Pokemon, Pokemon>();
                    var usedPks = new List<Pokemon>();
                    var localAllowed = allowedPokes;
                    if (area.BannedPokemon.Count > 0)
                    {
                        localAllowed = new List<Pokemon>(allowedPokes);
                        localAllowed.RemoveAll(area.BannedPokemon.Contains);
                    }

                    foreach (var areaPk in inArea)
                    {
                        var picked = PickWildPowerLvlReplacement(localAllowed, areaPk, false, usedPks);
                        areaMap[areaPk] = picked;
                        usedPks.Add(picked);
                    }

                    foreach (var enc in area.Encounters)
                        //  Apply the map
                        enc.Pokemon = areaMap[enc.Pokemon];
                }
            }
            else
            {
                //  Entirely random
                foreach (var area in scrambledEncounters)
                {
                    //  Poke-set
                    var inArea = PokemonInArea(area);
                    //  Build area map using randoms
                    var areaMap = new Dictionary<Pokemon, Pokemon>();
                    foreach (var areaPk in inArea)
                    {
                        var picked = noLegendaries ? RandomNonLegendaryPokemon() : RandomPokemon();
                        while (areaMap.ContainsValue(picked) ||
                               banned.Contains(picked) ||
                               area.BannedPokemon.Contains(picked))
                            picked = noLegendaries ? RandomNonLegendaryPokemon() : RandomPokemon();

                        areaMap[areaPk] = picked;
                    }

                    foreach (var enc in area.Encounters)
                        //  Apply the map
                        enc.Pokemon = areaMap[enc.Pokemon];
                }
            }
        }

        private HashSet<Pokemon> PokemonInArea(EncounterSet area)
        {
            var inArea = new HashSet<Pokemon>();
            foreach (var enc in area.Encounters) inArea.Add(enc.Pokemon);

            return inArea;
        }

        public void RandomizeAbilities(
            bool evolutionSanity,
            bool allowWonderGuard,
            bool banTrappingAbilities,
            bool banNegativeAbilities)
        {
            //  Abilities don't exist in some games...
            if (_romHandler.AbilitiesPerPokemon == 0) return;

            var hasDwAbilities = _romHandler.AbilitiesPerPokemon == 3;
            var bannedAbilities = new List<int>();
            if (!allowWonderGuard) bannedAbilities.Add(GlobalConstants.WonderGuardIndex);

            if (banTrappingAbilities) bannedAbilities.AddRange(GlobalConstants.BattleTrappingAbilities);

            if (banNegativeAbilities) bannedAbilities.AddRange(GlobalConstants.NegativeAbilities);

            var maxAbility = _romHandler.HighestAbilityIndex;
            if (evolutionSanity)
            {
                //  copy abilities straight up evolution lines
                //  still keep WG as an exception, though
                CopyUpEvolutionsHelper(
                    pokemon =>
                    {
                        if (pokemon.Ability1 != GlobalConstants.WonderGuardIndex &&
                            pokemon.Ability2 != GlobalConstants.WonderGuardIndex &&
                            pokemon.Ability3 != GlobalConstants.WonderGuardIndex)
                        {
                            // Pick first ability
                            pokemon.Ability1 = PickRandomAbility(maxAbility, bannedAbilities);

                            // Second ability?
                            pokemon.Ability2 = _random.NextDouble() < 0.5
                                ? PickRandomAbility(maxAbility, bannedAbilities, pokemon.Ability1)
                                : 0;

                            // Third ability?
                            if (hasDwAbilities)
                                pokemon.Ability3 = PickRandomAbility(
                                    maxAbility,
                                    bannedAbilities,
                                    pokemon.Ability1,
                                    pokemon.Ability2);
                        }
                    },
                    (evFrom, evTo, toMonIsFinalEvo) =>
                    {
                        if (evTo.Ability1 != GlobalConstants.WonderGuardIndex &&
                            evTo.Ability2 != GlobalConstants.WonderGuardIndex &&
                            evTo.Ability3 != GlobalConstants.WonderGuardIndex)
                        {
                            evTo.Ability1 = evFrom.Ability1;
                            evTo.Ability2 = evFrom.Ability2;
                            evTo.Ability3 = evFrom.Ability3;
                        }
                    });
            }
            else
            {
                var allPokes = _romHandler.ValidPokemons;
                foreach (var pk in allPokes)
                {
                    if (pk == null) continue;

                    //  Don't remove WG if already in place.
                    if (pk.Ability1 != GlobalConstants.WonderGuardIndex &&
                        pk.Ability2 != GlobalConstants.WonderGuardIndex &&
                        pk.Ability3 != GlobalConstants.WonderGuardIndex)
                    {
                        //  Pick first ability
                        pk.Ability1 = PickRandomAbility(maxAbility, bannedAbilities);
                        //  Second ability?
                        pk.Ability2 = _random.NextDouble() < 0.5
                            ? PickRandomAbility(maxAbility, bannedAbilities, pk.Ability1)
                            : 0;

                        //  Third ability?
                        if (hasDwAbilities)
                            pk.Ability3 = PickRandomAbility(maxAbility, bannedAbilities, pk.Ability1, pk.Ability2);
                    }
                }
            }
        }

        private int PickRandomAbility(int maxAbility, List<int> bannedAbilities, params int[] alreadySetAbilities)
        {
            int newAbility;
            while (true)
            {
                newAbility = _random.Next(maxAbility) + 1;
                if (bannedAbilities.Contains(newAbility)) continue;

                var repeat = false;
                foreach (var t in alreadySetAbilities)
                    if (t == newAbility)
                    {
                        repeat = true;
                        break;
                    }

                if (repeat) continue;
                break;
            }

            return newAbility;
        }

        public Pokemon Random2EvosPokemon()
        {
            var twoEvoPokes =
                _romHandler.ValidPokemons
                    .Where(
                        pk => pk.EvolutionsTo.Count == 0 &&
                              pk.EvolutionsFrom.Any(ev => ev.To.EvolutionsFrom.Count > 0))
                    .ToList();

            return twoEvoPokes[_random.Next(twoEvoPokes.Count)];
        }


        public void RivalCarriesStarter()
        {
            RivalCarriesStarterUpdate(_romHandler.Trainers, "RIVAL", 1);
            RivalCarriesStarterUpdate(_romHandler.Trainers, "FRIEND", 2);
        }

        private void RivalCarriesStarterUpdate(IEnumerable<Trainer> currentTrainers, string prefix, int pokemonOffset)
        {
            var enumerable = currentTrainers as Trainer[] ?? currentTrainers.ToArray();
            //  Find the highest rival battle #
            var highestRivalNum = enumerable
                .Where(t => t.Tag != null && t.Tag.StartsWith(prefix))
                .Max(t => int.Parse(t.Tag.Substring(prefix.Length, t.Tag.IndexOf('-') - prefix.Length)));

            if (highestRivalNum == 0) return;

            //  Get the starters
            //  us 0 1 2 => them 0+n 1+n 2+n
            var starters = _romHandler.Starters;
            //  Yellow needs its own case, unfortunately.
            if (_romHandler.IsYellow)
            {
                //  The rival's starter is index 1
                var rivalStarter = starters[1].Pokemon;
                var timesEvolves = NumEvolutions(rivalStarter, 2);
                //  Apply evolutions as appropriate
                if (timesEvolves == 0)
                {
                    for (var j = 1; j <= 3; j++) ChangeStarterWithTag(enumerable, prefix + (j + "-0"), rivalStarter);

                    for (var j = 4; j <= 7; j++)
                    for (var i = 0; i < 3; i++)
                        ChangeStarterWithTag(enumerable, prefix + (j + ("-" + i)), rivalStarter);
                }
                else if (timesEvolves == 1)
                {
                    for (var j = 1; j <= 3; j++) ChangeStarterWithTag(enumerable, prefix + (j + "-0"), rivalStarter);

                    rivalStarter = PickRandomEvolutionOf(rivalStarter, false);
                    for (var j = 4; j <= 7; j++)
                    for (var i = 0; i < 3; i++)
                        ChangeStarterWithTag(enumerable, prefix + (j + ("-" + i)), rivalStarter);
                }
                else if (timesEvolves == 2)
                {
                    for (var j = 1; j <= 2; j++)
                        ChangeStarterWithTag(enumerable, prefix + (j + ("-" + 0)), rivalStarter);

                    rivalStarter = PickRandomEvolutionOf(rivalStarter, true);
                    ChangeStarterWithTag(enumerable, prefix + "3-0", rivalStarter);
                    for (var i = 0; i < 3; i++) ChangeStarterWithTag(enumerable, prefix + ("4-" + i), rivalStarter);

                    rivalStarter = PickRandomEvolutionOf(rivalStarter, false);
                    for (var j = 5; j <= 7; j++)
                    for (var i = 0; i < 3; i++)
                        ChangeStarterWithTag(enumerable, prefix + (j + ("-" + i)), rivalStarter);
                }
            }
            else
            {
                //  Replace each starter as appropriate
                //  Use level to determine when to evolve, not number anymore
                for (var i = 0; i < 3; i++)
                {
                    //  Rival's starters are pokemonOffset over from each of ours
                    var starterToUse = (i + pokemonOffset) % 3;
                    var thisStarter = starters[starterToUse].Pokemon;
                    var timesEvolves = NumEvolutions(thisStarter, 2);
                    //  If a fully evolved pokemon, use throughout
                    //  Otherwise split by evolutions as appropriate
                    if (timesEvolves == 0)
                    {
                        for (var j = 1; j <= highestRivalNum; j++)
                            ChangeStarterWithTag(enumerable, prefix + (j + ("-" + i)), thisStarter);
                    }
                    else if (timesEvolves == 1)
                    {
                        var j = 1;
                        for (; j <= highestRivalNum / 2; j++)
                        {
                            if (GetLevelOfStarter(enumerable, prefix + (j + ("-" + i))) >= 30) break;

                            ChangeStarterWithTag(enumerable, prefix + (j + ("-" + i)), thisStarter);
                        }

                        thisStarter = PickRandomEvolutionOf(thisStarter, false);

                        for (; j <= highestRivalNum; j++)
                            ChangeStarterWithTag(enumerable, prefix + (j + ("-" + i)), thisStarter);
                    }
                    else if (timesEvolves == 2)
                    {
                        var j = 1;
                        for (; j <= highestRivalNum; j++)
                        {
                            if (GetLevelOfStarter(enumerable, prefix + (j + ("-" + i))) >= 16) break;

                            ChangeStarterWithTag(enumerable, prefix + (j + ("-" + i)), thisStarter);
                        }

                        thisStarter = PickRandomEvolutionOf(thisStarter, true);

                        for (; j <= highestRivalNum; j++)
                        {
                            if (GetLevelOfStarter(enumerable, prefix + (j + ("-" + i))) >= 36) break;

                            ChangeStarterWithTag(enumerable, prefix + (j + ("-" + i)), thisStarter);
                        }

                        thisStarter = PickRandomEvolutionOf(thisStarter, false);

                        for (; j <= highestRivalNum; j++)
                            ChangeStarterWithTag(enumerable, prefix + (j + ("-" + i)), thisStarter);
                    }
                }
            }
        }


        public void ForceFullyEvolvedTrainerPokes(int minLevel)
        {
            foreach (var t in _romHandler.Trainers)
            foreach (var tp in t.Pokemon)
                if (tp.Level >= minLevel)
                {
                    var newPokemon = FullyEvolve(tp.Pokemon);

                    if (ReferenceEquals(newPokemon, tp.Pokemon)) continue;

                    tp.Pokemon = newPokemon;
                    tp.ResetMoves = true;
                }
        }

        public void RandomizeMovePPs()
        {
            foreach (var mv in _romHandler.ValidMoves)
                if (mv != null &&
                    mv.InternalId != 165)
                    if (_random.Next(3) != 2) mv.Pp = _random.Next(3) * 5 + 15;
                    else mv.Pp = _random.Next(8) * 5 + 5;
        }

        public void RandomizeMoveAccuracies()
        {
            foreach (var mv in _romHandler.ValidMoves)
                if (mv != null &&
                    mv.InternalId != 165 &&
                    mv.Hitratio >= 5)
                    if (mv.Hitratio <= 50)
                    {
                        //  lowest tier (acc <= 50)
                        //  new accuracy = rand(20...50) inclusive
                        //  with a 10% chance to increase by 50%
                        mv.Hitratio = _random.Next(7) * 5 + 20;
                        if (_random.Next(10) == 0) mv.Hitratio = mv.Hitratio * 1 / (5 * 5);
                    }
                    else if (mv.Hitratio < 90)
                    {
                        //  middle tier (50 < acc < 90)
                        //  count down from 100% to 20% in 5% increments with 20%
                        //  chance to "stop" and use the current accuracy at each
                        //  increment
                        //  gives decent-but-not-100% accuracy most of the time
                        mv.Hitratio = 100;
                        while (mv.Hitratio > 20)
                        {
                            if (_random.Next(10) < 2) break;

                            mv.Hitratio -= 5;
                        }
                    }
                    else
                    {
                        //  highest tier (90 <= acc <= 100)
                        //  count down from 100% to 20% in 5% increments with 40%
                        //  chance to "stop" and use the current accuracy at each
                        //  increment
                        //  gives high accuracy most of the time
                        mv.Hitratio = 100;
                        while (mv.Hitratio > 20)
                        {
                            if (_random.Next(10) < 4) break;

                            mv.Hitratio -= 5;
                        }
                    }
        }

        public void RandomizeMoveCategory()
        {
            if (!_romHandler.HasPhysicalSpecialSplit) return;

            foreach (var mv in _romHandler.ValidMoves)
                if (mv.InternalId != 165 && mv.Category != MoveCategory.Status)
                    mv.Category = (MoveCategory) _random.Next(2);
        }

        private static readonly IReadOnlyDictionary<int, Typing> Gen5UpdateMoveType = new Dictionary<int, Typing>
        {
            //  Karate Chop => FIGHTING (gen1)
            { 2, Typing.Flying },

            //  Gust => FLYING (gen1)
            { 16, Typing.Flying },
            //  Sand Attack => GROUND (gen1)
            { 28, Typing.Ground },
            //  Move 44, Bite, becomes dark (but doesn't exist anyway)
            //  Curse => GHOST (gen2-4)
            { 174, Typing.Ghost },
        };

        private static readonly IReadOnlyDictionary<int, int> Gen5UpdateMoveAccuracy = new Dictionary<int, int>
        {
            //  Razor Wind => 100% accuracy (gen1/2)
            { 13, 100 },
            //  Whirlwind => 100 accuracy
            { 18, 100 },
            //  Bind => 85% accuracy (gen1-4)
            { 20, 85 },
            //  Tackle => 50 power, 100% accuracy , gen1-4
            { 33, 100 },
            //  Wrap => 90% accuracy (gen1-4)
            { 35, 90 },
            //  Disable => 100% accuracy (gen1-4)
            { 50, 100 },
            //  Blizzard => 70% accuracy (gen1)
            { 59, 70 },
            //  Low Kick => 100% accuracy (gen1)
            { 67, 100 },
            //  Fire Spin => 35 power, 85% acc (gen1-4)
            { 83, 85 },
            //  Rock Throw => 90% accuracy (gen1)
            { 88, 90 },
            //  Toxic => 90% accuracy (gen1-4)
            { 92, 90 },
            //  Hypnosis => 60% accuracy
            { 95, 60 },
            //  Clamp => 85% acc (gen1-4)
            { 128, 85 },
            //  Glare => 90% acc (gen1-4)
            { 137, 90 },
            //  Poison Gas => 80% acc (gen1-4)
            { 139, 80 },
            //  Flash => 100% acc (gen1/2/3)
            { 148, 100 },
            //  Crabhammer => 90% acc (gen1-4)
            { 152, 90 },
            //  Cotton Spore => 100% acc (gen2-4)
            { 178, 100 },
            //  Scary Face => 100% acc (gen2-4)
            { 184, 100 },
            //  Bone Rush => 90% acc (gen2-4)
            { 198, 90 },
            //  Future Sight => 10 pp, 100 power, 100% acc (gen2-4)
            { 248, 100 },
            //  Whirlpool => 35 pow, 85% acc (gen2-4)
            { 250, 85 },
            //  Sand Tomb => 35 pow, 85% acc (gen3-4)
            { 328, 85 },
            //  Rock Blast => 90% acc (gen3-4)
            { 350, 90 },
            //  Doom Desire => 140 pow, 100% acc, gen3-4
            { 353, 100 },
            //  Magma Storm => 75% acc
            { 463, 75 },
        };

        private static readonly IReadOnlyDictionary<int, int> Gen5UpdateMovePower = new Dictionary<int, int>
        {
            //  Wing Attack => 60 power (gen1)
            { 17, 60 },
            //  Fly => 90 power (gen1/2/3)
            { 19, 90 },
            //  Jump Kick => 10 pp, 100 power (gen1-4)
            { 26, 100 },
            //  Tackle => 50 power, 100% accuracy , gen1-4
            { 33, 50 },
            //  Thrash => 120 power, 10pp (gen1-4)
            { 37, 120 },
            //  Double-Edge => 120 power (gen1)
            { 38, 120 },
            //  Move 67, Low Kick, has weight-based power in gen3+
            //  Petal Dance => 120power, 10pp (gen1-4)
            { 80, 120 },
            //  Fire Spin => 35 power, 85% acc (gen1-4)
            { 83, 35 },
            //  Dig => 80 power (gen1/2/3)
            { 91, 80 },
            //  SelfDestruct => 200power (gen1)
            { 120, 200 },
            //  HJKick => 130 power, 10pp (gen1-4)
            { 136, 130 },
            //  Explosion => 250 power (gen1)
            { 153, 250 },
            //  Zap Cannon => 120 power (gen2-3)
            { 192, 120 },
            //  Outrage => 120 power (gen2-3)
            { 200, 120 },
            //  Giga Drain => 10pp (gen2-3), 75 power (gen2-4)
            { 202, 75 },
            //  Fury Cutter => 20 power (gen2-4)
            { 210, 20 },
            //  Future Sight => 10 pp, 100 power, 100% acc (gen2-4)
            { 248, 100 },
            //  Rock Smash => 40 power (gen2-3)
            { 249, 40 },
            //  Whirlpool => 35 pow, 85% acc (gen2-4)
            { 250, 35 },
            //  Uproar => 90 power (gen3-4)
            { 253, 90 },
            //  Uproar => 90 power (gen3-4)
            { 291, 80 },
            //  Sand Tomb => 35 pow, 85% acc (gen3-4)
            { 328, 35 },
            //  Bullet Seed => 25 power (gen3-4)
            { 331, 25 },
            //  Icicle Spear => 25 power (gen3-4)
            { 333, 25 },
            //  Covet => 60 power (gen3-4)
            { 343, 60 },
            { 348, 90 },
            //  Doom Desire => 140 pow, 100% acc, gen3-4
            { 353, 140 },
            //  Feint => 30 pow
            { 364, 30 },
            //  Last Resort => 140 pow
            { 387, 140 },
            //  Drain Punch => 10 pp, 75 pow
            { 409, 75 },
        };

        private static readonly IReadOnlyDictionary<int, int> Gen5UpdateMovePp = new Dictionary<int, int>
        {
            //  Vine Whip => 15 pp (gen1/2/3)
            { 22, 15 },
            //  Jump Kick => 10 pp, 100 power (gen1-4)
            { 26, 10 },
            //  Thrash => 120 power, 10pp (gen1-4)
            { 37, 10 },
            //  Absorb => 25pp (gen1/2/3)
            { 71, 25 },
            //  Mega Drain => 15pp (gen1/2/3)
            { 72, 15 },
            //  Petal Dance => 120power, 10pp (gen1-4)
            { 80, 10 },
            //  Recover => 10pp (gen1/2/3)
            { 105, 10 },
            //  Clamp => 85% acc (gen1-4)
            { 128, 15 },
            //  HJKick => 130 power, 10pp (gen1-4)
            { 136, 10 },
            //  Outrage => 120 power (gen2-3)
            { 200, 10 },
            //  Giga Drain => 10pp (gen2-3), 75 power (gen2-4)
            { 202, 10 },
            //  Future Sight => 10 pp, 100 power, 100% acc (gen2-4)
            { 248, 10 },
            //  Uproar => 90 power (gen3-4)
            { 254, 20 },
            //  Drain Punch => 10 pp, 75 pow
            { 409, 10 },
        };

        public void UpdateMovesToGen5()
        {
            foreach (var move in _romHandler.ValidMoves)
            {
                var id = move.InternalId;

                if (Gen5UpdateMoveType.TryGetValue(id, out var type)) move.Type = type;

                if (Gen5UpdateMoveAccuracy.TryGetValue(id, out var accuracy))
                    if (Math.Abs(move.Hitratio - accuracy) >= 1) move.Hitratio = accuracy;

                if (Gen5UpdateMovePower.TryGetValue(id, out var power)) move.Power = power;

                if (Gen5UpdateMovePp.TryGetValue(id, out var pp)) move.Pp = pp;
            }
        }

        private static readonly IReadOnlyDictionary<int, int> Gen6UpdateMoveAccuracy = new Dictionary<int, int>
        {
            //  Pin Missile 25 Power, 95% Accuracy
            { 42, 95 },
            //  Glare 100% Accuracy
            { 137, 100 },
            //  Poison Gas 90% Accuracy
            { 139, 90 },
            //  Psywave 100% Accuracy
            { 149, 100 },
            //  Will-o-Wisp 85% Accuracy
            { 261, 85 },
            //  Meteor Mash 90 Power, 90% Accuracy
            { 309, 90 },
            //  Rock Tomb 15 PP, 60 Power, 95% Accuracy
            { 317, 95 },
            //  Psycho Shift 100% Accuracy
            { 375, 100 },
            //  Gunk Shot 80% Accuracy
            { 441, 80 },
        };

        private static readonly IReadOnlyDictionary<int, int> Gen6UpdateMovePower = new Dictionary<int, int>
        {
            //  Vine Whip 25 PP, 45 Power
            { 22, 45 },
            //  Pin Missile 25 Power, 95% Accuracy
            { 42, 25 },
            //  Flamethrower 90 Power
            { 53, 90 },
            //  Hydro Pump 110 Power
            { 56, 110 },
            //  Surf 90 Power
            { 57, 90 },
            //  Ice Beam 90 Power
            { 58, 90 },
            //  Blizzard 110 Power
            { 59, 110 },
            //  Thunderbolt 90 Power
            { 85, 90 },
            //  Thunder 110 Power
            { 87, 110 },
            //  Lick 30 Power
            { 122, 30 },
            //  Smog 30 Power
            { 123, 30 },
            //  Fire Blast 110 Power
            { 126, 110 },
            //  Skull Bash 10 PP, 130 Power
            { 130, 130 },
            //  Bubble 40 Power
            { 145, 40 },
            //  Crabhammer 100 Power
            { 152, 100 },
            //  Thief 25 PP, 60 Power
            { 168, 60 },
            //  Snore 50 Power
            { 173, 50 },
            //  Fury Cutter 40 Power
            { 210, 40 },
            //  Future Sight 120 Power
            { 248, 120 },
            //  Heat Wave 95 Power
            { 257, 95 },
            //  Smellingsalt 70 Power
            { 265, 70 },
            //  Knock off 65 Power
            { 282, 65 },
            //  Meteor Mash 90 Power, 90% Accuracy
            { 309, 90 },
            //  Air Cutter 60 Power
            { 314, 60 },
            //  Overheat 130 Power
            { 315, 130 },
            //  Rock Tomb 15 PP, 60 Power, 95% Accuracy
            { 317, 60 },
            //  Muddy Water 90 Power
            { 330, 90 },
            //  Wake-Up Slap 70 Power
            { 358, 70 },
            //  Assurance 60 Power
            { 372, 60 },
            //  Aura Sphere 80 Power
            { 396, 80 },
            //  Dragon Pulse 85 Power
            { 406, 85 },
            //  Power Gem 80 Power
            { 408, 80 },
            //  Energy Ball 90 Power
            { 412, 90 },
            //  Draco Meteor 130 Power
            { 434, 130 },
            //  Leaf Storm 130 Power
            { 437, 130 },
            //  Chatter 65 Power
            { 448, 65 },
            //  Magma Storm 100 Power
            { 463, 100 },
            //  Storm Throw 60 Power
            { 480, 60 },
            //  Synchronoise 120 Power
            { 485, 120 },
            //  Low Sweep 65 Power
            { 490, 65 },
            //  Hex 65 Power
            { 506, 65 },
            //  Incinerate 60 Power
            { 510, 60 },
            //  Pledges 80 Power
            { 518, 80 },
            { 519, 80 },
            { 520, 80 },
            //  Struggle Bug 50 Power
            { 522, 50 },
            //  Frost Breath 45 Power
            //  crits are 2x in these games
            { 524, 45 },
            //  Hurricane 110 Power
            { 542, 110 },
            //  Techno Blast 120 Power
            { 546, 120 },
        };

        private static readonly IReadOnlyDictionary<int, int> Gen6UpdateMovePp = new Dictionary<int, int>
        {
            //  gen 1
            //  Swords Dance 20 PP
            { 14, 20 },
            //  Vine Whip 25 PP, 45 Power
            { 22, 25 },
            //  Growth 20 PP
            { 74, 20 },
            //  Minimize 10 PP
            { 107, 10 },
            //  Barrier 20 PP
            { 112, 20 },
            //  Skull Bash 10 PP, 130 Power
            { 130, 10 },
            //  Acid Armor 20 PP
            { 151, 20 },
            //  Thief 25 PP, 60 Power
            { 168, 25 },
            //  Rock Tomb 15 PP, 60 Power, 95% Accuracy
            { 317, 15 },
            //  Extrasensory 20 PP
            { 326, 20 },
            //  Covet 25 PP
            { 343, 25 },
            //  Tailwind 15 PP
            { 366, 15 },
            //  Air Slash 15 PP
            { 403, 15 },
            //  Sacred Sword 15 PP
            { 533, 15 },
        };

        public void UpdateMovesToGen6()
        {
            UpdateMovesToGen5();

            foreach (var move in _romHandler.ValidMoves)
            {
                var id = move.InternalId;

                if (Gen6UpdateMoveAccuracy.TryGetValue(id, out var accuracy))
                    if (Math.Abs(move.Hitratio - accuracy) >= 1) move.Hitratio = accuracy;

                if (Gen6UpdateMovePower.TryGetValue(id, out var power)) move.Power = power;

                if (Gen6UpdateMovePp.TryGetValue(id, out var pp)) move.Pp = pp;
            }
        }

        private Pokemon FullyEvolve(Pokemon pokemon)
        {
            var seenMons = new HashSet<Pokemon> { pokemon };
            while (true)
            {
                if (pokemon.EvolutionsFrom.Count == 0) break;

                //  check for cyclic evolutions from what we've already seen
                var cyclic = false;
                foreach (var ev in pokemon.EvolutionsFrom)
                    if (seenMons.Contains(ev.To))
                    {
                        //  cyclic evolution detected - bail now
                        cyclic = true;
                        break;
                    }

                if (cyclic) break;

                //  pick a random evolution to continue from
                pokemon = pokemon.EvolutionsFrom[_random.Next(pokemon.EvolutionsFrom.Count)].To;
                seenMons.Add(pokemon);
            }

            return pokemon;
        }

        private Pokemon PickRandomEvolutionOf(Pokemon basePokemon, bool mustEvolveItself)
        {
            //  Used for "rival carries starter"
            //  Pick a random evolution of base Pokemon, subject to
            //  "must evolve itself" if appropriate.
            var candidates = new List<Pokemon>();
            foreach (var ev in basePokemon.EvolutionsFrom)
                if (!mustEvolveItself ||
                    ev.To.EvolutionsFrom.Count > 0) candidates.Add(ev.To);

            if (candidates.Count == 0)
                throw new NotImplementedException(
                    "Random evolution called on a Pokemon without any usable evolutions.");

            return candidates[_random.Next(candidates.Count)];
        }

        private int GetLevelOfStarter(IEnumerable<Trainer> currentTrainers, string tag)
        {
            foreach (var t in currentTrainers)
                if (t.Tag != null &&
                    t.Tag.Equals(tag))
                {
                    //  Bingo, get highest level
                    //  last pokemon is given priority +2 but equal priority
                    //  = first pokemon wins, so its effectively +1
                    //  If it's tagged the same we can assume it's the same team
                    //  just the opposite gender or something like that...
                    //  So no need to check other trainers with same tag.
                    var highestLevel = t.Pokemon[0].Level;
                    var trainerPkmnCount = t.Pokemon.Length;
                    for (var i = 1; i < trainerPkmnCount; i++)
                    {
                        var levelBonus = i == trainerPkmnCount - 1 ? 2 : 0;
                        if (t.Pokemon[i].Level + levelBonus > highestLevel) highestLevel = t.Pokemon[i].Level;
                    }

                    return highestLevel;
                }

            return 0;
        }

        private static void ChangeStarterWithTag(IEnumerable<Trainer> currentTrainers, string tag, Pokemon starter)
        {
            foreach (var t in currentTrainers)
            {
                if (t.Tag == null || !t.Tag.Equals(tag)) continue;

                //  Bingo
                //  Change the highest level pokemon, not the last.
                //  BUT: last gets +2 lvl priority (effectively +1)
                //  same as above, equal priority = earlier wins
                var bestPoke = t.Pokemon[0];
                var trainerPkmnCount = t.Pokemon.Length;
                for (var i = 1; i < trainerPkmnCount; i++)
                {
                    var levelBonus = i == trainerPkmnCount - 1 ? 2 : 0;

                    if (t.Pokemon[i].Level + levelBonus > bestPoke.Level) bestPoke = t.Pokemon[i];
                }

                bestPoke.Pokemon = starter;
                bestPoke.ResetMoves = true;
            }
        }

        private static int NumEvolutions(Pokemon pk, int maxInterested)
        {
            return NumEvolutions(pk, 0, maxInterested);
        }

        private static int NumEvolutions(Pokemon pk, int depth, int maxInterested)
        {
            if (pk.EvolutionsFrom.Count == 0) return 0;
            if (depth == maxInterested - 1) return 1;
            var maxEvos = 0;
            foreach (var ev in pk.EvolutionsFrom)
                maxEvos = Math.Max(maxEvos, NumEvolutions(ev.To, depth + 1, maxInterested) + 1);

            return maxEvos;
        }

        //  Return the max depth of pre-evolutions a Pokemon has
        private static int NumPreEvolutions(Pokemon pk, int maxInterested)
        {
            return NumPreEvolutions(pk, 0, maxInterested);
        }

        private static int NumPreEvolutions(Pokemon pk, int depth, int maxInterested)
        {
            if (pk.EvolutionsTo.Count == 0) return 0;
            if (depth == maxInterested - 1) return 1;
            var maxPreEvos = 0;

            foreach (var ev in pk.EvolutionsTo)
                maxPreEvos = Math.Max(maxPreEvos, NumPreEvolutions(ev.From, depth + 1, maxInterested) + 1);

            return maxPreEvos;
        }

        public void RandomizeEvolutions(bool similarStrength, bool sameType, bool limitToThreeStages, bool forceChange)
        {
            var pokemonPool = new List<Pokemon>(_romHandler.ValidPokemons);
            var stageLimit = limitToThreeStages ? 3 : 10;

            //  Cache old evolutions for data later
            var originalEvos = new Dictionary<Pokemon, List<Evolution>>();
            foreach (var pk in pokemonPool) originalEvos[pk] = new List<Evolution>(pk.EvolutionsFrom);

            var newEvoPairs = new HashSet<EvolutionPair>();
            var oldEvoPairs = new HashSet<EvolutionPair>();
            if (forceChange)
                foreach (var pk in pokemonPool)
                foreach (var ev in pk.EvolutionsFrom) oldEvoPairs.Add(new EvolutionPair(ev.From, ev.To));

            var replacements = new List<Pokemon>();
            var loops = 0;
            while (loops < 1)
            {
                //  Setup for this loop.
                var hadError = false;
                foreach (var pk in pokemonPool)
                {
                    pk.EvolutionsFrom.Clear();
                    pk.EvolutionsTo.Clear();
                }

                newEvoPairs.Clear();
                //  Shuffle pokemon list so the results aren't overly predictable.
                pokemonPool.Shuffle(_random);
                foreach (var fromPk in pokemonPool)
                {
                    var oldEvos = originalEvos[fromPk];
                    foreach (var ev in oldEvos)
                    {
                        //  Pick a Pokemon as replacement
                        replacements.Clear();
                        //  Step 1: base filters
                        foreach (var pk in _romHandler.ValidPokemons)
                        {
                            //  Prevent evolving into oneself (mandatory)
                            if (ReferenceEquals(pk, fromPk)) continue;

                            //  Force same EXP curve (mandatory)
                            if (pk.GrowthCurve != fromPk.GrowthCurve) continue;

                            var ep = new EvolutionPair(fromPk, pk);
                            //  Prevent split evos choosing the same Pokemon
                            //  (mandatory)
                            if (newEvoPairs.Contains(ep)) continue;

                            //  Prevent evolving into old thing if flagged
                            if (forceChange && oldEvoPairs.Contains(ep)) continue;

                            //  Prevent evolution that causes cycle (mandatory)
                            if (EvoCycleCheck(fromPk, pk)) continue;

                            //  Prevent evolution that exceeds stage limit
                            var tempEvo = new Evolution(fromPk, pk, false, EvolutionType.None, 0);
                            fromPk.EvolutionsFrom.Add(tempEvo);
                            pk.EvolutionsTo.Add(tempEvo);
                            var exceededLimit = false;
                            var related = RelatedPokemon(fromPk);
                            foreach (var pk2 in related)
                            {
                                var numPreEvos = NumPreEvolutions(pk2, stageLimit);
                                if (numPreEvos >= stageLimit)
                                {
                                    exceededLimit = true;
                                    break;
                                }
                                if (numPreEvos == stageLimit - 1 &&
                                    pk2.EvolutionsFrom.Count == 0 &&
                                    originalEvos[pk2].Count > 0)
                                {
                                    exceededLimit = true;
                                    break;
                                }
                            }

                            fromPk.EvolutionsFrom.Remove(tempEvo);
                            pk.EvolutionsTo.Remove(tempEvo);
                            if (exceededLimit) continue;

                            //  Passes everything, add as a candidate.
                            replacements.Add(pk);
                        }

                        //  If we don't have any candidates after Step 1, severe
                        //  failure
                        //  exit out of this loop and try again from scratch
                        if (replacements.Count == 0)
                        {
                            hadError = true;
                            break;
                        }

                        //  Step 2: filter by type, if needed
                        if (replacements.Count > 1 && sameType)
                        {
                            var includeType = new HashSet<Pokemon>();
                            foreach (var pk in replacements)
                                if (pk.PrimaryType == fromPk.PrimaryType ||
                                    fromPk.SecondaryType != null && pk.PrimaryType == fromPk.SecondaryType ||
                                    pk.SecondaryType != null && pk.SecondaryType == fromPk.PrimaryType ||
                                    fromPk.SecondaryType != null &&
                                    pk.SecondaryType != null &&
                                    pk.SecondaryType == fromPk.SecondaryType) includeType.Add(pk);

                            if (includeType.Count != 0)
                                replacements.RemoveAll(pokemon => !includeType.Contains(pokemon));
                        }

                        //  Step 3: pick - by similar strength or otherwise
                        Pokemon picked;
                        if (replacements.Count == 1) picked = replacements[0];
                        else if (similarStrength) picked = PickEvoPowerLvlReplacement(replacements, ev.To);
                        else picked = replacements[_random.Next(replacements.Count)];

                        //  Step 4: add it to the new evos pool
                        var newEvo = new Evolution(fromPk, picked, ev.CarryStats, ev.Type, ev.ExtraInfo);
                        fromPk.EvolutionsFrom.Add(newEvo);
                        picked.EvolutionsTo.Add(newEvo);
                        newEvoPairs.Add(new EvolutionPair(fromPk, picked));
                    }

                    if (hadError) break;
                }

                //  If no error, done and return
                if (!hadError) return;
                loops++;
            }

            //  If we made it out of the loop, we weren't able to randomize evos.
            throw new NotImplementedException("Not able to randomize evolutions in a sane amount of retries.");
        }

        private bool EvoCycleCheck(Pokemon from, Pokemon to)
        {
            var tempEvo = new Evolution(from, to, false, EvolutionType.None, 0);
            @from.EvolutionsFrom.Add(tempEvo);
            var visited = new HashSet<Pokemon>();
            var recStack = new HashSet<Pokemon>();
            var recur = IsCyclic(from, visited, recStack);
            @from.EvolutionsFrom.Remove(tempEvo);
            return recur;
        }

        private bool IsCyclic(Pokemon pk, HashSet<Pokemon> visited, HashSet<Pokemon> recStack)
        {
            if (!visited.Contains(pk))
            {
                visited.Add(pk);
                recStack.Add(pk);
                foreach (var ev in pk.EvolutionsFrom)
                    if (!visited.Contains(ev.To) &&
                        IsCyclic(ev.To, visited, recStack)) return true;
                    else if (recStack.Contains(ev.To)) return true;
            }

            recStack.Remove(pk);
            return false;
        }

        private HashSet<Pokemon> RelatedPokemon(Pokemon original)
        {
            var results = new HashSet<Pokemon> { original };
            var toCheck = new Queue<Pokemon>();
            toCheck.Enqueue(original);
            while (toCheck.Count != 0)
            {
                var check = toCheck.Dequeue();
                foreach (var ev in check.EvolutionsFrom)
                    if (!results.Contains(ev.To))
                    {
                        results.Add(ev.To);
                        toCheck.Enqueue(ev.To);
                    }

                foreach (var ev in check.EvolutionsTo)
                    if (!results.Contains(ev.From))
                    {
                        results.Add(ev.From);
                        toCheck.Enqueue(ev.From);
                    }
            }

            return results;
        }

        private Pokemon PickEvoPowerLvlReplacement(List<Pokemon> pokemonPool, Pokemon current)
        {
            //  start with within 10% and add 5% either direction till we find
            //  something
            var currentBst = current.BstForPowerLevels();
            var minTarget = currentBst - currentBst / 10;
            var maxTarget = currentBst + currentBst / 10;
            var canPick = new List<Pokemon>();
            var expandRounds = 0;
            while (canPick.Count == 0 ||
                   canPick.Count < 3 && expandRounds < 3)
            {
                foreach (var pk in pokemonPool)
                    if (pk.BstForPowerLevels() >= minTarget &&
                        pk.BstForPowerLevels() <= maxTarget &&
                        !canPick.Contains(pk)) canPick.Add(pk);

                minTarget = minTarget - currentBst / 20;
                maxTarget = maxTarget + currentBst / 20;
                expandRounds++;
            }

            return canPick[_random.Next(canPick.Count)];
        }

        private class EvolutionPair
        {
            private readonly Pokemon _from;

            private readonly Pokemon _to;

            public EvolutionPair(Pokemon from, Pokemon to)
            {
                _from = from;
                _to = to;
            }


            public override int GetHashCode()
            {
                var prime = 31;
                var result = 1;
                result = prime * result + (_from == null ? 0 : _from.GetHashCode());
                result = prime * result + (_to == null ? 0 : _to.GetHashCode());
                return result;
            }


            public override bool Equals(object obj)
            {
                if (this == obj) return true;

                if (obj == null) return false;

                if (GetType() != obj.GetType()) return false;

                var other = (EvolutionPair) obj;
                if (_from == null) { if (other._from != null) return false; }
                else if (!_from.Equals(other._from)) { return false; }

                if (_to == null) { if (other._to != null) return false; }
                else if (!_to.Equals(other._to)) { return false; }

                return true;
            }
        }

        public void RemoveBrokenMoves()
        {
            var allBanned = new HashSet<int>(_romHandler.GameBreakingMoves);
            foreach (var pokemon in _romHandler.ValidPokemons)
                pokemon.MovesLearnt.RemoveAll(move => allBanned.Contains(move.Move));
        }

        public void RandomizeMovesLearnt(
            bool typeThemed,
            bool noBroken,
            bool forceFourStartingMoves,
            double goodDamagingProbability)
        {
            //  Get current sets
            var hms = _romHandler.HmMoves;

            var allBanned = new HashSet<int>(noBroken ? _romHandler.GameBreakingMoves : Enumerable.Empty<int>());
            allBanned.UnionWith(hms);
            allBanned.UnionWith(_romHandler.MovesBannedFromLevelup);

            //  Build sets of moves
            var validMoves = new List<Move>();
            var validDamagingMoves = new List<Move>();
            var validTypeMoves = new Dictionary<Typing, List<Move>>();
            var validTypeDamagingMoves = new Dictionary<Typing, List<Move>>();
            foreach (var mv in _romHandler.ValidMoves)
                if (mv != null &&
                    !GlobalConstants.BannedRandomMoves[mv.Number] &&
                    !allBanned.Contains(mv.Number))
                {
                    validMoves.Add(mv);
                    if (mv.Type != null)
                    {
                        if (!validTypeMoves.ContainsKey(mv.Type)) validTypeMoves[mv.Type] = new List<Move>();

                        validTypeMoves[mv.Type].Add(mv);
                    }

                    if (!GlobalConstants.BannedForDamagingMove[mv.Number])
                        if (mv.Power >= 2 * GlobalConstants.MinDamagingMovePower ||
                            mv.Power >= GlobalConstants.MinDamagingMovePower && mv.Hitratio >= 90)
                        {
                            validDamagingMoves.Add(mv);
                            if (mv.Type != null)
                            {
                                if (!validTypeDamagingMoves.ContainsKey(mv.Type))
                                    validTypeDamagingMoves[mv.Type] = new List<Move>();

                                validTypeDamagingMoves[mv.Type].Add(mv);
                            }
                        }
                }

            foreach (var pokemon in _romHandler.ValidPokemons)
            {
                var learnt = new HashSet<int>();
                var moves = pokemon.MovesLearnt;
                //  4 starting moves?
                if (forceFourStartingMoves)
                {
                    var lv1Count = 0;
                    foreach (var ml in moves) if (ml.Level == 1) lv1Count++;

                    if (lv1Count < 4)
                        for (var i = 0; i < 4 - lv1Count; i++)
                        {
                            var fakeLv1 = new MoveLearnt
                            {
                                Level = 1,
                                Move = 0
                            };
                            moves.Insert(0, fakeLv1);
                        }
                }

                //  Find last lv1 move
                //  lv1index ends up as the index of the first non-lv1 move
                var lv1Index = 0;
                while (lv1Index < moves.Count &&
                       moves[lv1Index].Level == 1) lv1Index++;

                //  last lv1 move is 1 before lv1index
                if (lv1Index != 0) lv1Index--;

                //  Replace moves as needed
                for (var i = 0; i < moves.Count; i++)
                {
                    //  should this move be forced damaging?
                    var attemptDamaging = i == lv1Index || _random.NextDouble() < goodDamagingProbability;

                    //  type themed?
                    Typing typeOfMove = null;
                    if (typeThemed)
                    {
                        var picked = _random.NextDouble();
                        if (pokemon.PrimaryType == Typing.Normal ||
                            pokemon.SecondaryType == Typing.Normal)
                        {
                            if (pokemon.SecondaryType == null)
                            {
                                //  Pure NORMAL: 75% normal, 25% random
                                if (picked < 0.75) typeOfMove = Typing.Normal;

                                //  else random
                            }
                            else
                            {
                                //  Find the other type
                                //  Normal/OTHER: 30% normal, 55% other, 15% random
                                var otherType = pokemon.PrimaryType;
                                if (otherType == Typing.Normal) otherType = pokemon.SecondaryType;

                                if (picked < 0.3) typeOfMove = Typing.Normal;
                                else if (picked < 0.85) typeOfMove = otherType;

                                //  else random
                            }
                        }
                        else if (pokemon.SecondaryType != null)
                        {
                            //  Primary/Secondary: 50% primary, 30% secondary, 5%
                            //  normal, 15% random
                            if (picked < 0.5) typeOfMove = pokemon.PrimaryType;
                            else if (picked < 0.8) typeOfMove = pokemon.SecondaryType;
                            else if (picked < 0.85) typeOfMove = Typing.Normal;

                            //  else random
                        }
                        else
                        {
                            //  Primary/None: 60% primary, 20% normal, 20% random
                            if (picked < 0.6) typeOfMove = pokemon.PrimaryType;
                            else if (picked < 0.8) typeOfMove = Typing.Normal;

                            //  else random
                        }
                    }

                    //  select a list to pick a move from that has at least one free
                    var pickList = validMoves;
                    if (attemptDamaging)
                    {
                        if (typeOfMove != null)
                        {
                            if (validTypeDamagingMoves.ContainsKey(typeOfMove) &&
                                CheckForUnusedMove(validTypeDamagingMoves[typeOfMove], learnt))
                                pickList = validTypeDamagingMoves[typeOfMove];
                            else if (CheckForUnusedMove(validDamagingMoves, learnt)) pickList = validDamagingMoves;
                        }
                        else if (CheckForUnusedMove(validDamagingMoves, learnt)) { pickList = validDamagingMoves; }
                    }
                    else if (typeOfMove != null)
                    {
                        if (validTypeMoves.ContainsKey(typeOfMove) &&
                            CheckForUnusedMove(validTypeMoves[typeOfMove], learnt))
                            pickList = validTypeMoves[typeOfMove];
                    }

                    //  now pick a move until we get a valid one
                    var mv = pickList[_random.Next(pickList.Count)];
                    while (learnt.Contains(mv.Number)) mv = pickList[_random.Next(pickList.Count)];

                    //  write it
                    moves[i].Move = mv.Number;
                    if (i == lv1Index) moves[i].Level = 1;

                    learnt.Add(mv.Number);
                }
            }

            bool CheckForUnusedMove(List<Move> potentialList, HashSet<int> alreadyUsed)
            {
                foreach (var mv in potentialList) if (!alreadyUsed.Contains(mv.Number)) return true;

                return false;
            }
        }

        protected void ApplyCamelCaseNames()
        {
            var pokes = _romHandler.ValidPokemons;
            foreach (var pkmn in pokes)
            {
                if (pkmn == null) continue;

                pkmn.Name = RomFunctions.CamelCase(pkmn.Name);
            }
        }


        public void MinimumCatchRate(int rateNonLegendary, int rateLegendary)
        {
            var pokes = _romHandler.ValidPokemons;
            foreach (var pkmn in pokes)
            {
                if (pkmn == null) continue;

                var minCatchRate = pkmn.Legendary ? rateLegendary : rateNonLegendary;

                pkmn.CatchRate = Math.Max(pkmn.CatchRate, minCatchRate);
            }
        }


        public void StandardizeExpCurves()
        {
            var pokes = _romHandler.ValidPokemons;
            foreach (var pkmn in pokes)
            {
                if (pkmn == null) continue;

                pkmn.GrowthCurve = pkmn.Legendary ? Exp.Curve.Slow : Exp.Curve.MediumFast;
            }
        }

        public void CondenseLevelEvolutions(int maxLevel, int maxIntermediateLevel)
        {
            var allPokemon = _romHandler.ValidPokemons;
            var changedEvos = new HashSet<Evolution>();
            //  search for level evolutions
            foreach (var pk in allPokemon)
                if (pk != null)
                    foreach (var checkEvo in pk.EvolutionsFrom)
                        if (checkEvo.Type.UsesLevel())
                        {
                            //  bring down the level of this evo if it exceeds max
                            //  level
                            if (checkEvo.ExtraInfo > maxLevel)
                            {
                                checkEvo.ExtraInfo = maxLevel;
                                changedEvos.Add(checkEvo);
                            }

                            //  Now, seperately, if an intermediate level evo is too
                            //  high, bring it down
                            foreach (var otherEvo in pk.EvolutionsTo)
                                if (otherEvo.Type.UsesLevel() &&
                                    otherEvo.ExtraInfo > maxIntermediateLevel)
                                {
                                    otherEvo.ExtraInfo = maxIntermediateLevel;
                                    changedEvos.Add(otherEvo);
                                }
                        }

            //  Log changes now that we're done (to avoid repeats)
            Log("--Condensed Level Evolutions--");
            foreach (var evol in changedEvos)
                Log($"{evol.From.Name} now evolves into {evol.To.Name} at minimum level {evol.ExtraInfo}");

            LogBlankLine();
        }


        public void OrderDamagingMovesByDamage()
        {
            foreach (var pkmn in _romHandler.ValidPokemons)
            {
                var moves = pkmn.MovesLearnt;
                //  Build up a list of damaging moves and their positions
                var damagingMoveIndices = new List<int>();
                var damagingMoves = new List<Move>();
                for (var i = 0; i < moves.Count; i++)
                {
                    var mv = _romHandler.ValidMoves[moves[i].Move];
                    if (mv.Power > 1)
                    {
                        //  considered a damaging move for this purpose
                        damagingMoveIndices.Add(i);
                        damagingMoves.Add(mv);
                    }
                }

                //  Ties should be sorted randomly, so shuffle the list first.
                damagingMoves.Shuffle(_random);
                //  Sort the damaging moves by power
                damagingMoves.Sort(
                    (m1, m2) =>
                    {
                        if (m1.Power * m1.HitCount < m2.Power * m2.HitCount) return -1;
                        if (m1.Power * m1.HitCount > m2.Power * m2.HitCount) return 1;
                        return 0;
                    });

                //  Reassign damaging moves in the ordered positions
                for (var i = 0; i < damagingMoves.Count; i++)
                    moves[damagingMoveIndices[i]].Move = damagingMoves[i].Number;
            }
        }

        public void MetronomeOnlyMode()
        {
            var metronomeMl = new MoveLearnt
            {
                Level = 1,
                Move = GlobalConstants.MetronomeMove
            };
            foreach (var pkmn in _romHandler.ValidPokemons)
            {
                pkmn.MovesLearnt.Clear();
                pkmn.MovesLearnt.Add(metronomeMl);
            }

            //  trainers
            //  run this to remove all custom non-Metronome moves
            foreach (var t in _romHandler.Trainers) foreach (var tpk in t.Pokemon) tpk.ResetMoves = true;

            //  tms
            var tmMoves = _romHandler.TmMoves;
            for (var i = 0; i < tmMoves.Length; i++) tmMoves[i] = GlobalConstants.MetronomeMove;

            //  movetutors
            if (_romHandler.HasMoveTutors)
            {
                var mtMoves = _romHandler.MoveTutorMoves;
                for (var i = 0; i < mtMoves.Length; i++) mtMoves[i] = GlobalConstants.MetronomeMove;
            }

            //  move tweaks
            var metronome = _romHandler.ValidMoves[GlobalConstants.MetronomeMove];
            metronome.Pp = 40;
            var hms = _romHandler.HmMoves;
            foreach (var hm in hms)
            {
                var thisHm = _romHandler.ValidMoves[hm];
                thisHm.Pp = 0;
            }
        }

        public void RandomizeStaticPokemon(bool legendForLegend)
        {
            //  Load
            var currentStaticPokemon = _romHandler.StaticPokemon;
            var banned = _romHandler.BannedForStaticPokemon;
            if (legendForLegend)
            {
                var legendariesLeft = new List<Pokemon>(_romHandler.LegendaryPokemons);
                var nonlegsLeft = new List<Pokemon>(_romHandler.NonLegendaryPokemons);
                legendariesLeft.RemoveAll(banned.Contains);
                nonlegsLeft.RemoveAll(banned.Contains);

                for (var i = 0; i < currentStaticPokemon.Length; ++i)
                {
                    if (currentStaticPokemon[i].Legendary)
                    {
                        var num = _random.Next(legendariesLeft.Count);
                        currentStaticPokemon[i] = legendariesLeft[num];
                        legendariesLeft.RemoveAt(num);

                        if (legendariesLeft.Count == 0)
                        {
                            legendariesLeft.AddRange(_romHandler.LegendaryPokemons);
                            legendariesLeft.RemoveAll(banned.Contains);
                        }
                    }
                    else
                    {
                        var num = _random.Next(nonlegsLeft.Count);
                        currentStaticPokemon[i] = nonlegsLeft[num];
                        nonlegsLeft.RemoveAt(num);
                        if (nonlegsLeft.Count == 0)
                        {
                            nonlegsLeft.AddRange(_romHandler.NonLegendaryPokemons);
                            nonlegsLeft.RemoveAll(banned.Contains);
                        }
                    }
                }
            }
            else
            {
                var pokemonLeft = new List<Pokemon>(_romHandler.ValidPokemons);
                pokemonLeft.RemoveAll(banned.Contains);
                for (var i = 0; i < currentStaticPokemon.Length; i++)
                {
                    var num = _random.Next(pokemonLeft.Count);

                    currentStaticPokemon[i] = pokemonLeft[num];
                    pokemonLeft.RemoveAt(num);

                    if (pokemonLeft.Count == 0)
                    {
                        pokemonLeft.AddRange(_romHandler.ValidPokemons);
                        pokemonLeft.RemoveAll(banned.Contains);
                    }
                }
            }
        }

        public void RandomizeTmMoves(bool noBroken, bool preserveField, bool levelupTmMoveSanity, double goodDamagingProbability)
        {
            //  Pick some random TM moves.
            var hms = _romHandler.HmMoves;
            var oldTMs = _romHandler.TmMoves;

            var banned = new List<int>(noBroken ? _romHandler.GameBreakingMoves : Enumerable.Empty<int>());
            //  field moves?
            var fieldMoves = _romHandler.FieldMoves;
            var preservedFieldMoveCount = 0;
            if (preserveField)
            {
                var banExistingField = new List<int>(oldTMs);
                banExistingField.RemoveAll(i => !fieldMoves.Contains(i));
                preservedFieldMoveCount = banExistingField.Count;
                banned.AddRange(banExistingField);
            }

            //  Determine which moves are pickable
            var usableMoves = new List<Move>(_romHandler.ValidMoves);
            usableMoves.RemoveAt(0);
            //  remove null entry
            var unusableMoves = new HashSet<Move>();
            var unusableDamagingMoves = new HashSet<Move>();
            foreach (var mv in usableMoves)
                if (GlobalConstants.BannedRandomMoves[mv.Number] ||
                    hms.Contains(mv.Number) ||
                    banned.Contains(mv.Number)) unusableMoves.Add(mv);
                else if (GlobalConstants.BannedForDamagingMove[mv.Number] ||
                         mv.Power < GlobalConstants.MinDamagingMovePower) unusableDamagingMoves.Add(mv);

            usableMoves.RemoveAll(unusableMoves.Contains);
            var usableDamagingMoves = new List<Move>(usableMoves);
            usableDamagingMoves.RemoveAll(unusableDamagingMoves.Contains);
            //  pick (tmCount - preservedFieldMoveCount) moves
            var pickedMoves = new List<int>();
            for (var i = 0; i < oldTMs.Length - preservedFieldMoveCount; i++)
            {
                Move chosenMove;
                if (_random.NextDouble() < goodDamagingProbability &&
                    usableDamagingMoves.Count > 0)
                    chosenMove = usableDamagingMoves[_random.Next(usableDamagingMoves.Count)];
                else chosenMove = usableMoves[_random.Next(usableMoves.Count)];

                pickedMoves.Add(chosenMove.Number);
                usableMoves.Remove(chosenMove);
                usableDamagingMoves.Remove(chosenMove);
            }

            //  shuffle the picked moves because high goodDamagingProbability
            //  could bias them towards early numbers otherwise
            pickedMoves.Shuffle(_random);
            //  finally, distribute them as tms
            var pickedMoveIndex = 0;

            for (var i = 0; i < oldTMs.Length; i++)
            {
                if (preserveField && fieldMoves.Contains(oldTMs[i])) continue;

                oldTMs[i] = pickedMoves[pickedMoveIndex++];
            }

            if (levelupTmMoveSanity)
            {
                //  if a pokemon learns a move in its moveset
                //  and there is a TM of that move, make sure
                //  that TM can be learned.
                var tmMoves = _romHandler.TmMoves;

                foreach (var pokemon in _romHandler.ValidPokemons)
                {
                    var moveset = pokemon.MovesLearnt;
                    var pkmnCompat = pokemon.TMHMCompatibility;
                    foreach (var ml in moveset)
                        if (tmMoves.Contains(ml.Move))
                        {
                            var tmIndex = Array.IndexOf(tmMoves, ml.Move);
                            pkmnCompat[tmIndex + 1] = true;
                        }
                }
            }
        }

        public enum TmsHmsCompatibility
        {
            RandomPreferType,
            CompletelyRandom,
            Full
        }

        public void RandomizeTmhmCompatibility(TmsHmsCompatibility compatibility)
        {
            //  Get current compatibility
            //  new: increase HM chances if required early on
            var requiredEarlyOn = _romHandler.EarlyRequiredHmMoves;
            var tmHMs = new List<int>(_romHandler.TmMoves);
            tmHMs.AddRange(_romHandler.HmMoves);

            foreach (var pkmn in _romHandler.ValidPokemons)
            {
                var flags = pkmn.TMHMCompatibility;
                for (var i = 1; i <= tmHMs.Count; i++)
                {
                    var move = tmHMs[i - 1];
                    var mv = _romHandler.ValidMoves[move];
                    var probability = 0.5;
                    if (compatibility == TmsHmsCompatibility.RandomPreferType)
                        if (pkmn.PrimaryType.Equals(mv.Type) ||
                            pkmn.SecondaryType != null && pkmn.SecondaryType.Equals(mv.Type)) probability = 0.9;
                        else if (mv.Type != null &&
                                 mv.Type.Equals(Typing.Normal)) probability = 0.5;
                        else probability = 0.25;

                    if (requiredEarlyOn.Contains(move)) probability = Math.Min(1, probability * 1.8);

                    flags[i] = _random.NextDouble() < probability;
                }
            }

            if (compatibility != TmsHmsCompatibility.Full)
                return;
            
            var tmCount = _romHandler.TmMoves.Length;
            foreach (var pokemon in _romHandler.ValidPokemons)
            {
                var flags = pokemon.TMHMCompatibility;
                for (var i = tmCount + 1; i < flags.Length; i++) flags[i] = true;
            }
        }


        public void RandomizeMoveTutorMoves(bool noBroken, bool preserveField, double goodDamagingProbability)
        {
            if (!_romHandler.HasMoveTutors) return;

            //  Pick some random Move Tutor moves, excluding TMs.
            var tms = _romHandler.TmMoves;
            var oldMTs = _romHandler.MoveTutorMoves;
            var mtCount = oldMTs.Length;
            var hms = _romHandler.HmMoves;

            var banned = new List<int>(noBroken ? _romHandler.GameBreakingMoves : Enumerable.Empty<int>());

            //  field moves?
            var fieldMoves = _romHandler.FieldMoves;
            var preservedFieldMoveCount = 0;
            if (preserveField)
            {
                var banExistingField = new List<int>(oldMTs);
                banExistingField.RemoveAll(i => !fieldMoves.Contains(i));
                preservedFieldMoveCount = banExistingField.Count;
                banned.AddRange(banExistingField);
            }

            //  Determine which moves are pickable
            var usableMoves = new List<Move>(_romHandler.ValidMoves);
            usableMoves.RemoveAt(0);
            //  remove null entry
            var unusableMoves = new HashSet<Move>();
            var unusableDamagingMoves = new HashSet<Move>();
            foreach (var mv in usableMoves)
                if (GlobalConstants.BannedRandomMoves[mv.Number] ||
                    tms.Contains(mv.Number) ||
                    hms.Contains(mv.Number) ||
                    banned.Contains(mv.Number)) unusableMoves.Add(mv);
                else if (GlobalConstants.BannedForDamagingMove[mv.Number] ||
                         mv.Power < GlobalConstants.MinDamagingMovePower) unusableDamagingMoves.Add(mv);

            usableMoves.RemoveAll(unusableMoves.Contains);
            var usableDamagingMoves = new List<Move>(usableMoves);
            usableDamagingMoves.RemoveAll(unusableDamagingMoves.Contains);
            //  pick (tmCount - preservedFieldMoveCount) moves
            var pickedMoves = new List<int>();
            for (var i = 0;
                i < mtCount - preservedFieldMoveCount;
                i++)
            {
                Move chosenMove;
                if (_random.NextDouble() < goodDamagingProbability &&
                    usableDamagingMoves.Count > 0)
                    chosenMove = usableDamagingMoves[_random.Next(usableDamagingMoves.Count)];
                else chosenMove = usableMoves[_random.Next(usableMoves.Count)];

                pickedMoves.Add(chosenMove.Number);
                usableMoves.Remove(chosenMove);
                usableDamagingMoves.Remove(chosenMove);
            }

            //  shuffle the picked moves because high goodDamagingProbability
            //  could bias them towards early numbers otherwise
            pickedMoves.Shuffle(_random);
            //  finally, distribute them as tutors
            var pickedMoveIndex = 0;
            for (var i = 0; i < mtCount; i++)
            {
                if (preserveField && fieldMoves.Contains(oldMTs[i])) continue;

                oldMTs[i] = pickedMoves[pickedMoveIndex++];
            }
        }

        public void FullMoveTutorCompatibility()
        {
            if (!_romHandler.HasMoveTutors) return;

            var compat = _romHandler.MoveTutorCompatibility;
            foreach (var compatEntry in compat)
            {
                var flags = compatEntry.Value;
                for (var i = 1; i < flags.Length; i++) flags[i] = true;
            }
        }

        public void RandomizeMoveTutorCompatibility(bool preferSameType)
        {
            if (!_romHandler.HasMoveTutors) return;

            //  Get current compatibility
            var compat = _romHandler.MoveTutorCompatibility;
            var mts = _romHandler.MoveTutorMoves;
            foreach (var compatEntry in compat)
            {
                var pkmn = compatEntry.Key;
                var flags = compatEntry.Value;
                for (var i = 1; i <= mts.Length; i++)
                {
                    var move = mts[i - 1];
                    var mv = _romHandler.ValidMoves[move];
                    var probability = 0.5;
                    if (preferSameType)
                        if (pkmn.PrimaryType.Equals(mv.Type) ||
                            pkmn.SecondaryType != null && pkmn.SecondaryType.Equals(mv.Type)) probability = 0.9;
                        else if (mv.Type != null &&
                                 mv.Type.Equals(Typing.Normal)) probability = 0.5;
                        else probability = 0.25;

                    flags[i] = _random.NextDouble() < probability;
                }
            }
        }


        public void EnsureMoveTutorCompatSanity()
        {
            if (!_romHandler.HasMoveTutors) return;

            //  if a pokemon learns a move in its moveset
            //  and there is a tutor of that move, make sure
            //  that tutor can be learned.
            var compat = _romHandler.MoveTutorCompatibility;
            var mtMoves = _romHandler.MoveTutorMoves;
            foreach (var pkmn in _romHandler.ValidPokemons)
            {
                var moveset = pkmn.MovesLearnt;
                var pkmnCompat = compat[pkmn];
                foreach (var ml in moveset)
                    if (mtMoves.Contains(ml.Move))
                    {
                        var mtIndex = Array.IndexOf(mtMoves, ml.Move);
                        pkmnCompat[mtIndex + 1] = true;
                    }
            }
        }

        public void RandomizeTrainerNames(CustomNamesSet customNames)
        {
            if (!_romHandler.CanChangeTrainerText) return;

            //  index 0 = singles, 1 = doubles
            List<string>[] allTrainerNames = { new List<string>(), new List<string>() };
            Dictionary<int, List<string>>[] trainerNamesByLength =
            {
                new Dictionary<int, List<string>>(),
                new Dictionary<int, List<string>>()
            };

            //  Read name lists
            foreach (var trainername in customNames.TrainerNames)
            {
                var len = _romHandler.InternalStringLength(trainername);
                if (len <= 10)
                {
                    allTrainerNames[0].Add(trainername);
                    if (trainerNamesByLength[0].ContainsKey(len)) { trainerNamesByLength[0][len].Add(trainername); }
                    else
                    {
                        var namesOfThisLength = new List<string>
                        {
                            trainername
                        };
                        trainerNamesByLength[0][len] = namesOfThisLength;
                    }
                }
            }

            foreach (var trainername in customNames.DoublesTrainerNames)
            {
                var len = _romHandler.InternalStringLength(trainername);
                if (len <= 10)
                {
                    allTrainerNames[1].Add(trainername);
                    if (trainerNamesByLength[1].ContainsKey(len)) { trainerNamesByLength[1][len].Add(trainername); }
                    else
                    {
                        var namesOfThisLength = new List<string> { trainername };
                        trainerNamesByLength[1][len] = namesOfThisLength;
                    }
                }
            }

            //  Get the current trainer names data
            var currentTrainerNames = _romHandler.TrainerNames;
            if (currentTrainerNames.Length == 0) return;

            var mode = _romHandler.TrainerNameMode;
            var maxLength = _romHandler.MaxTrainerNameLength;
            var totalMaxLength = _romHandler.MaxSumOfTrainerNameLengths;
            var success = false;
            var tries = 0;
            //  Init the translation map and new list
            var translation = new Dictionary<string, string>();

            var tcNameLengths = _romHandler.TcNameLengthsByTrainer;
            //  loop until we successfully pick names that fit
            //  should always succeed first attempt except for gen2.
            while (!success &&
                   tries < 10000)
            {
                success = true;
                translation.Clear();
                var totalLength = 0;
                //  Start choosing
                for (var i = 0; i < currentTrainerNames.Length; ++i)
                {
                    var trainerName = currentTrainerNames[i];

                    if (translation.ContainsKey(trainerName) &&
                        trainerName.Equals("GRUNT", StringComparison.InvariantCultureIgnoreCase) == false &&
                        trainerName.Equals("EXECUTIVE", StringComparison.InvariantCultureIgnoreCase) == false)
                    {
                        //  use an already picked translation
                        currentTrainerNames[i] = translation[trainerName];
                        totalLength = totalLength + _romHandler.InternalStringLength(translation[trainerName]);
                    }
                    else
                    {
                        var idx = trainerName.Contains("&") ? 1 : 0;
                        var pickFrom = allTrainerNames[idx];
                        var intStrLen = _romHandler.InternalStringLength(trainerName);
                        if (mode == TrainerNameMode.SameLength) pickFrom = trainerNamesByLength[idx][intStrLen];

                        var changeTo = trainerName;
                        var ctl = intStrLen;
                        if (pickFrom != null &&
                            pickFrom.Count > 0 &&
                            intStrLen > 1)
                        {
                            var innerTries = 0;
                            changeTo = pickFrom[_random.Next(pickFrom.Count)];
                            ctl = _romHandler.InternalStringLength(changeTo);
                            while (mode == TrainerNameMode.MaxLength && ctl > maxLength ||
                                   mode == TrainerNameMode.MaxLengthWithClass &&
                                   ctl + tcNameLengths[i] > maxLength)
                            {
                                innerTries++;
                                if (innerTries == 100)
                                {
                                    changeTo = trainerName;
                                    ctl = intStrLen;
                                    break;
                                }

                                changeTo = pickFrom[_random.Next(pickFrom.Count)];
                                ctl = _romHandler.InternalStringLength(changeTo);
                            }
                        }

                        translation[trainerName] = changeTo;
                        currentTrainerNames[i] = changeTo;
                        totalLength = totalLength + ctl;
                    }

                    if (totalLength > totalMaxLength)
                    {
                        success = false;
                        tries++;
                        break;
                    }
                }
            }

            if (!success)
                throw new NotImplementedException(
                    "Could not randomize trainer names in a reasonable amount of attempts." +
                    "\nPlease add some shorter names to your custom trainer names.");
        }

        public void RandomizeTrainerClassNames(CustomNamesSet customNames)
        {
            if (!_romHandler.CanChangeTrainerText) return;

            //  index 0 = singles, 1 = doubles
            List<string>[] allTrainerClasses = { new List<string>(), new List<string>() };
            Dictionary<int, List<string>>[] trainerClassesByLength =
            {
                new Dictionary<int, List<string>>(),
                new Dictionary<int, List<string>>()
            };

            AddAllClasses(trainerClassesByLength[0], allTrainerClasses[0], customNames.TrainerClasses);
            AddAllClasses(trainerClassesByLength[0], allTrainerClasses[0], customNames.DoublesTrainerClasses);

            void AddAllClasses(IDictionary<int, List<string>> classByLength, ICollection<string> classesByNothing, IEnumerable<string> trainerClasses)
            {

                foreach (var trainerClassName in trainerClasses)
                {
                    classesByNothing.Add(trainerClassName);
                    var len = _romHandler.InternalStringLength(trainerClassName);
                    if (classByLength.ContainsKey(len))
                    {
                        classByLength[len].Add(trainerClassName);
                    }
                    else
                    {
                        var namesOfThisLength = new List<string> { trainerClassName };
                        classByLength[len] = namesOfThisLength;
                    }
                }
            }

            //  Get the current trainer names data
            var currentClassNames = _romHandler.TrainerClassNames.ToArray();
            var mustBeSameLength = _romHandler.FixedTrainerClassNamesLength;
            var maxLength = _romHandler.MaxTrainerClassNameLength;
            //  Init the translation map and new list
            var translation = new Dictionary<string, string>();
            var numTrainerClasses = currentClassNames.Length;
            var doublesClasses = _romHandler.DoublesTrainerClasses;
            //  Start choosing
            for (var i = 0; i < numTrainerClasses; i++)
            {
                var trainerClassName = currentClassNames[i];
                if (translation.ContainsKey(trainerClassName))
                {
                    //  use an already picked translation
                    currentClassNames[i] = translation[trainerClassName];
                }
                else
                {
                    var idx = doublesClasses.Contains(i) ? 1 : 0;

                    var pickFrom = allTrainerClasses[idx];
                    var intStrLen = _romHandler.InternalStringLength(trainerClassName);
                    if (mustBeSameLength) pickFrom = trainerClassesByLength[idx][intStrLen];

                    var changeTo = trainerClassName;
                    if (pickFrom != null &&
                        pickFrom.Count > 0)
                    {
                        changeTo = pickFrom[_random.Next(pickFrom.Count)];
                        while (changeTo.Length > maxLength) changeTo = pickFrom[_random.Next(pickFrom.Count)];
                    }

                    translation[trainerClassName] = changeTo;
                    currentClassNames[i] = changeTo;
                }
            }
        }

        public void RandomizeWildHeldItems(bool banBadItems)
        {
            var pokemon = _romHandler.ValidPokemons;
            var possibleItems = banBadItems ? _romHandler.NonBadItems : _romHandler.AllowedItems;

            foreach (var pk in pokemon)
            {
                if (pk.GuaranteedHeldItem == -1 &&
                    pk.CommonHeldItem == -1 &&
                    pk.RareHeldItem == -1 &&
                    pk.DarkGrassHeldItem == -1) return;

                var canHaveDarkGrass = pk.DarkGrassHeldItem != -1;
                if (pk.GuaranteedHeldItem != -1)
                {
                    //  Guaranteed held items are supported.
                    if (pk.GuaranteedHeldItem > 0)
                    {
                        //  Currently have a guaranteed item
                        var decision = _random.NextDouble();
                        if (decision < 0.9)
                        {
                            //  Stay as guaranteed
                            canHaveDarkGrass = false;
                            pk.GuaranteedHeldItem = possibleItems.RandomItem(_random);
                        }
                        else
                        {
                            //  Change to 25% or 55% chance
                            pk.GuaranteedHeldItem = 0;
                            pk.CommonHeldItem = possibleItems.RandomItem(_random);
                            pk.RareHeldItem = possibleItems.RandomItem(_random);
                            while (pk.RareHeldItem == pk.CommonHeldItem)
                                pk.RareHeldItem = possibleItems.RandomItem(_random);
                        }
                    }
                    else
                    {
                        //  No guaranteed item atm
                        var decision = _random.NextDouble();
                        if (decision < 0.5)
                        {
                            //  No held item at all
                            pk.CommonHeldItem = 0;
                            pk.RareHeldItem = 0;
                        }
                        else if (decision < 0.65)
                        {
                            //  Just a rare item
                            pk.CommonHeldItem = 0;
                            pk.RareHeldItem = possibleItems.RandomItem(_random);
                        }
                        else if (decision < 0.8)
                        {
                            //  Just a common item
                            pk.CommonHeldItem = possibleItems.RandomItem(_random);
                            pk.RareHeldItem = 0;
                        }
                        else if (decision < 0.95)
                        {
                            //  Both a common and rare item
                            pk.CommonHeldItem = possibleItems.RandomItem(_random);
                            pk.RareHeldItem = possibleItems.RandomItem(_random);
                            while (pk.RareHeldItem == pk.CommonHeldItem)
                                pk.RareHeldItem = possibleItems.RandomItem(_random);
                        }
                        else
                        {
                            //  Guaranteed item
                            canHaveDarkGrass = false;
                            pk.GuaranteedHeldItem = possibleItems.RandomItem(_random);
                            pk.CommonHeldItem = 0;
                            pk.RareHeldItem = 0;
                        }
                    }
                }
                else
                {
                    //  Code for no guaranteed items
                    var decision = _random.NextDouble();
                    if (decision < 0.5)
                    {
                        //  No held item at all
                        pk.CommonHeldItem = 0;
                        pk.RareHeldItem = 0;
                    }
                    else if (decision < 0.65)
                    {
                        //  Just a rare item
                        pk.CommonHeldItem = 0;
                        pk.RareHeldItem = possibleItems.RandomItem(_random);
                    }
                    else if (decision < 0.8)
                    {
                        //  Just a common item
                        pk.CommonHeldItem = possibleItems.RandomItem(_random);
                        pk.RareHeldItem = 0;
                    }
                    else
                    {
                        //  Both a common and rare item
                        pk.CommonHeldItem = possibleItems.RandomItem(_random);
                        pk.RareHeldItem = possibleItems.RandomItem(_random);
                        while (pk.RareHeldItem == pk.CommonHeldItem) pk.RareHeldItem = possibleItems.RandomItem(_random);
                    }
                }

                if (canHaveDarkGrass)
                {
                    var dgDecision = _random.NextDouble();
                    pk.DarkGrassHeldItem = dgDecision < 0.5 ? possibleItems.RandomItem(_random) : 0;
                }
                else if (pk.DarkGrassHeldItem != -1) { pk.DarkGrassHeldItem = 0; }
            }
        }

        public void RandomizeStarterHeldItems(bool banBadItems)
        {
            var oldHeldItems = _romHandler.Starters;
            var possibleItems = banBadItems ? _romHandler.NonBadItems : _romHandler.AllowedItems;

            for (var i = 0; i < oldHeldItems.Count; i++) oldHeldItems[i].HeldItem = possibleItems.RandomItem(_random);
        }

        public void ShuffleFieldItems()
        {
            var currentItems = _romHandler.RegularFieldItems;
            var currentTMs = _romHandler.CurrentFieldTMs;
            currentItems.Shuffle(_random);
            currentTMs.Shuffle(_random);
        }

        public void RandomizeFieldItems(bool banBadItems)
        {
            var possibleItems = banBadItems ? _romHandler.NonBadItems : _romHandler.AllowedItems;

            var currentItems = _romHandler.RegularFieldItems;
            var currentTMs = _romHandler.CurrentFieldTMs;
            var requiredTMs = _romHandler.RequiredFieldTMs;
            var fieldItemCount = currentItems.Length;
            var fieldTmCount = currentTMs.Length;
            var reqTmCount = requiredTMs.Count;
            var totalTmCount = _romHandler.TmMoves.Length;
            var newItems = new List<int>();
            var newTMs = new List<int>();
            for (var i = 0; i < fieldItemCount; i++) newItems.Add(possibleItems.RandomNonTm(_random));

            newTMs.AddRange(requiredTMs);
            for (var i = reqTmCount; i < fieldTmCount; i++)
                while (true)
                {
                    var tm = _random.Next(totalTmCount) + 1;
                    if (!newTMs.Contains(tm))
                    {
                        newTMs.Add(tm);
                        break;
                    }
                }

            newItems.Shuffle(_random);
            newTMs.Shuffle(_random);

            for (var i = 0; i < newItems.Count; ++i) currentItems[i] = newItems[i];
            for (var i = 0; i < newTMs.Count; ++i) currentTMs[i] = newTMs[i];
        }


        public void RemoveTradeEvolutions(bool changeMoveEvos)
        {
            Log("--Removing Trade Evolutions--");
            var extraEvolutions = new HashSet<Evolution>();
            foreach (var pkmn in _romHandler.ValidPokemons)
                if (pkmn != null)
                {
                    extraEvolutions.Clear();
                    foreach (var evo in pkmn.EvolutionsFrom)
                    {
                        if (changeMoveEvos && evo.Type == EvolutionType.LevelWithMove)
                        {
                            // read move
                            var move = evo.ExtraInfo;
                            var levelLearntAt = 1;
                            foreach (var ml in evo.From.MovesLearnt)
                                if (ml.Move == move)
                                {
                                    levelLearntAt = ml.Level;
                                    break;
                                }
                            if (levelLearntAt == 1) levelLearntAt = 45;
                            // change to pure level evo
                            evo.Type = EvolutionType.Level;
                            evo.ExtraInfo = levelLearntAt;
                            LogEvoChangeLevel(evo.From.Name, evo.To.Name, levelLearntAt);
                        }
                        // Pure Trade
                        if (evo.Type == EvolutionType.Trade)
                        {
                            // Replace w/ level 37
                            evo.Type = EvolutionType.Level;
                            evo.ExtraInfo = 37;
                            LogEvoChangeLevel(evo.From.Name, evo.To.Name, 37);
                        }
                        // Trade w/ Item
                        if (evo.Type == EvolutionType.TradeItem)
                        {
                            // Get the current item & evolution
                            var item = evo.ExtraInfo;
                            if (evo.From.Id == Gen5Constants.SlowpokeIndex)
                            {
                                // Slowpoke is awkward - he already has a level evo
                                // So we can't do Level up w/ Held Item for him
                                // Put Water Stone instead
                                evo.Type = EvolutionType.Stone;
                                evo.ExtraInfo = Gen5Constants.WaterStoneIndex; // water
                                // stone
                                LogEvoChangeStone(
                                    evo.From.Name,
                                    evo.To.Name,
                                    _romHandler.ItemNames[Gen5Constants.WaterStoneIndex]);
                            }
                            else
                            {
                                LogEvoChangeLevelWithItem(evo.From.Name, evo.To.Name, _romHandler.ItemNames[item]);
                                // Replace, for this entry, w/
                                // Level up w/ Held Item at Day
                                evo.Type = EvolutionType.LevelItemDay;
                                // now add an extra evo for
                                // Level up w/ Held Item at Night
                                var extraEntry = new Evolution(
                                    evo.From,
                                    evo.To,
                                    true,
                                    EvolutionType.LevelItemNight,
                                    item);
                                extraEvolutions.Add(extraEntry);
                            }
                        }
                        if (evo.Type == EvolutionType.TradeSpecial)
                        {
                            // This is the karrablast <-> shelmet trade
                            // Replace it with Level up w/ Other Species in Party
                            // (22)
                            // Based on what species we're currently dealing with
                            evo.Type = EvolutionType.LevelWithOther;
                            evo.ExtraInfo = evo.From.Id == Gen5Constants.KarrablastIndex
                                ? Gen5Constants.ShelmetIndex
                                : Gen5Constants.KarrablastIndex;
                            LogEvoChangeLevelWithPkmn(
                                evo.From.Name,
                                evo.To.Name,
                                _romHandler.ValidPokemons[evo.From.Id == Gen5Constants.KarrablastIndex
                                        ? Gen5Constants.ShelmetIndex
                                        : Gen5Constants.KarrablastIndex]
                                    .Name);
                        }
                    }

                    foreach (var ev in extraEvolutions)
                    {
                        pkmn.EvolutionsFrom.Add(ev);
                        ev.To.EvolutionsTo.Add(ev);
                    }
                }
            LogBlankLine();
        }

        public void RandomizeStarters(bool withTwoEvos)
        {
            // Randomise
            _logStream.WriteLine("--Random 2-Evolution Starters--");
            var starterCount = 3;

            if (_romHandler.IsYellow)
                starterCount = 2;

            foreach (var starter in _romHandler.Starters)
                starter.Pokemon = null;

            for (var i = 0; i < starterCount; i++)
            {
                Pokemon pkmn;
                var selectFrom = _romHandler.ValidPokemons;

                if (withTwoEvos)
                {
                    selectFrom = selectFrom
                        .Where(pk => pk.EvolutionsTo.Count == 0 && pk.EvolutionsFrom.Count > 0)
                        .Where(pk => pk.EvolutionsFrom.Any(ev => ev.To.EvolutionsFrom.Count > 0))
                        .ToArray();
                }
                
                do
                {
                    pkmn = selectFrom[_random.Next(selectFrom.Count)];
                } while (_romHandler.Starters.Any(p => pkmn.Equals(p.Pokemon) .Equals(pkmn)));

                _romHandler.Starters[i].Pokemon = pkmn;

                _logStream.WriteLine("Set starter {0} to {1}", i + 1, pkmn.Name);
            }

            _logStream.WriteLine();
        }
        
        public void RandomizeIngameTrades(bool randomizeRequest, bool randomStats, bool randomItem)
        {

            // Process nicknames
            var nicknames = new List<string>();
            

            // get old trades
            var trades = _romHandler.IngameTrades;
            var usedRequests = new List<Pokemon>();
            var usedGivens = new List<Pokemon>();
            var possibleItems = _romHandler.AllowedItems;

            foreach (var trade in trades)
            {
                // pick new given pokemon
                var oldgiven = trade.GivenPokemon;
                Pokemon given;

                
                do {
                    given = RandomPokemon();
                } while (usedGivens.Contains(given));

                usedGivens.Add(given);
                trade.GivenPokemon = given;

                // requested pokemon?
                if (oldgiven == trade.RequestedPokemon)
                {
                    // preserve trades for the same pokemon
                    trade.RequestedPokemon = given;
                }
                else if (randomizeRequest)
                {
                    Pokemon request;
                    
                    do {
                        request = RandomPokemon();
                    } while (usedRequests.Contains(request) || Equals(request, given));

                    usedRequests.Add(request);
                    trade.RequestedPokemon = request;
                }

                if (randomStats)
                {
                    var maxIv = _romHandler.HasDVs ? 16 : 32;

                    for (var i = 0; i < trade.Ivs.Length; i++)
                    {
                        trade.Ivs[i] = _random.Next(maxIv);
                    }
                }

                if (randomItem)
                {
                    trade.Item = possibleItems.RandomItem(_random);
                }
            }
        }


    protected void Log(string log)
        {
            _logStream?.WriteLine(log);
        }

        protected void LogBlankLine()
        {
            _logStream?.WriteLine();
        }

        protected void LogEvoChangeLevel(string pkFrom, string pkTo, int level)
        {
            _logStream?.WriteLine("Made {0} evolve into {1} at level {2}", pkFrom, pkTo, level);
        }

        protected void LogEvoChangeLevelWithItem(string pkFrom, string pkTo, string itemName)
        {
            _logStream?.WriteLine("Made {0} evolve into {1} by leveling up holding {2}", pkFrom, pkTo, itemName);
        }

        protected void LogEvoChangeStone(string pkFrom, string pkTo, string itemName)
        {
            _logStream?.WriteLine("Made {0} evolve into {1} using a {2}", pkFrom, pkTo, itemName);
        }

        protected void LogEvoChangeLevelWithPkmn(string pkFrom, string pkTo, string otherRequired)
        {
            _logStream?.WriteLine(
                "Made {0} evolve into {1} by leveling up with {2} in the party",
                pkFrom,
                pkTo,
                otherRequired);
        }
    }
}
