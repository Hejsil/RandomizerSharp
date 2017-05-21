using System;
using System.Collections.Generic;
using System.Linq;
using RandomizerSharp.PokemonModel;
using RandomizerSharp.RomHandlers;

namespace RandomizerSharp.Randomizers
{
    public class TraitRandomizer : BaseRandomizer
    {
        public TraitRandomizer(AbstractRomHandler romHandler)
            : base(romHandler)
        {
        }

        public TraitRandomizer(AbstractRomHandler romHandler, Random random)
            : base(romHandler, random)
        {
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
                var allPokes = RomHandler.ValidPokemons;
                foreach (var pk in allPokes)
                {
                    if (pk != null)
                        RandomizeStatsWithinBst(pk);
                }
            }

            void RandomizeStatsWithinBst(Pokemon pokemon)
            {
                int baseStat;
                double totW;

                var hpW = Random.NextDouble();
                var atkW = Random.NextDouble();
                var defW = Random.NextDouble();
                var spaW = Random.NextDouble();
                var spdW = Random.NextDouble();
                var speW = Random.NextDouble();

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
                    pokemon.Speed > 255)
                    RandomizeStatsWithinBst(pokemon);
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
                    pokemon => pokemon.ShuffleStats(Random),
                    (evFrom, evTo, toMonIsFinalEvo) => evTo.CopyShuffledStatsUpEvolution(evFrom)
                );
            }
            else
            {
                var allPokes = RomHandler.ValidPokemons;
                foreach (var pk in allPokes)
                    pk?.ShuffleStats(Random);
            }
        }

        public void RandomizePokemonTypes(bool evolutionSanity)
        {
            var allPokes = RomHandler.ValidPokemons;
            if (evolutionSanity)
            {
                CopyUpEvolutionsHelper(
                    RandomizePokemonType,
                    RandomizeEvolutionTypes);
            }
            else
            {
                foreach (var pkmn in allPokes)
                {
                    if (pkmn != null)
                    {
                        pkmn.PrimaryType = RandomType();
                        pkmn.SecondaryType = null;

                        if (!(Random.NextDouble() < 0.5))
                            continue;

                        pkmn.SecondaryType = RandomType();
                        while (pkmn.SecondaryType == pkmn.PrimaryType)
                            pkmn.SecondaryType = RandomType();
                    }
                }
            }

            void RandomizeEvolutionTypes(Pokemon evFrom, Pokemon evTo, bool toMonIsFinalEvo)
            {
                evTo.PrimaryType = evFrom.PrimaryType;
                evTo.SecondaryType = evFrom.SecondaryType;

                if (evTo.SecondaryType == null)
                {
                    var chance = toMonIsFinalEvo ? 0.25 : 0.15;
                    if (Random.NextDouble() < chance)
                    {
                        evTo.SecondaryType = RandomType();
                        while (evTo.SecondaryType == evTo.PrimaryType)
                            evTo.SecondaryType = RandomType();
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
                    if (Random.NextDouble() < 0.35)
                    {
                        pokemon.SecondaryType = RandomType();
                        while (pokemon.SecondaryType == pokemon.PrimaryType)
                            pokemon.SecondaryType = RandomType();
                    }
                }
                else
                {
                    if (Random.NextDouble() < 0.5)
                    {
                        pokemon.SecondaryType = RandomType();
                        while (pokemon.SecondaryType == pokemon.PrimaryType)
                            pokemon.SecondaryType = RandomType();
                    }
                }
            }
        }

        public Pokemon Random2EvosPokemon()
        {
            var twoEvoPokes =
                RomHandler.ValidPokemons
                    .Where(
                        pk => pk.EvolutionsTo.Count == 0 &&
                              pk.EvolutionsFrom.Any(ev => ev.To.EvolutionsFrom.Count > 0))
                    .ToList();

            return twoEvoPokes[Random.Next(twoEvoPokes.Count)];
        }

        public void RandomizeEvolutions(bool similarStrength, bool sameType, bool limitToThreeStages, bool forceChange)
        {
            var pokemonPool = new List<Pokemon>(RomHandler.ValidPokemons);
            var stageLimit = limitToThreeStages ? 3 : 10;

            //  Cache old evolutions for data later
            var originalEvos = new Dictionary<Pokemon, List<Evolution>>();
            foreach (var pk in pokemonPool)
                originalEvos[pk] = new List<Evolution>(pk.EvolutionsFrom);

            var newEvoPairs = new HashSet<EvolutionPair>();
            var oldEvoPairs = new HashSet<EvolutionPair>();
            if (forceChange)
                foreach (var pk in pokemonPool)
                foreach (var ev in pk.EvolutionsFrom)
                    oldEvoPairs.Add(new EvolutionPair(ev.From, ev.To));

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
                pokemonPool.Shuffle(Random);
                foreach (var fromPk in pokemonPool)
                {
                    var oldEvos = originalEvos[fromPk];
                    foreach (var ev in oldEvos)
                    {
                        //  Pick a Pokemon as replacement
                        replacements.Clear();
                        //  Step 1: base filters
                        foreach (var pk in RomHandler.ValidPokemons)
                        {
                            //  Prevent evolving into oneself (mandatory)
                            if (ReferenceEquals(pk, fromPk))
                                continue;

                            //  Force same EXP expCurve (mandatory)
                            if (pk.GrowthExpCurve != fromPk.GrowthExpCurve)
                                continue;

                            var ep = new EvolutionPair(fromPk, pk);
                            //  Prevent split evos choosing the same Pokemon
                            //  (mandatory)
                            if (newEvoPairs.Contains(ep))
                                continue;

                            //  Prevent evolving into old thing if flagged
                            if (forceChange && oldEvoPairs.Contains(ep))
                                continue;

                            //  Prevent evolution that causes cycle (mandatory)
                            if (EvoCycleCheck(fromPk, pk))
                                continue;

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
                            if (exceededLimit)
                                continue;

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
                            {
                                if (pk.PrimaryType == fromPk.PrimaryType ||
                                    fromPk.SecondaryType != null && pk.PrimaryType == fromPk.SecondaryType ||
                                    pk.SecondaryType != null && pk.SecondaryType == fromPk.PrimaryType ||
                                    fromPk.SecondaryType != null &&
                                    pk.SecondaryType != null &&
                                    pk.SecondaryType == fromPk.SecondaryType)
                                    includeType.Add(pk);
                            }

                            if (includeType.Count != 0)
                                replacements.RemoveAll(pokemon => !includeType.Contains(pokemon));
                        }

                        //  Step 3: pick - by similar strength or otherwise
                        Pokemon picked;
                        if (replacements.Count == 1)
                            picked = replacements[0];
                        else if (similarStrength)
                            picked = PickEvoPowerLvlReplacement(replacements, ev.To);
                        else
                            picked = replacements[Random.Next(replacements.Count)];

                        //  Step 4: add it to the new evos pool
                        var newEvo = new Evolution(fromPk, picked, ev.CarryStats, ev.Type1, ev.ExtraInfo);
                        fromPk.EvolutionsFrom.Add(newEvo);
                        picked.EvolutionsTo.Add(newEvo);
                        newEvoPairs.Add(new EvolutionPair(fromPk, picked));
                    }

                    if (hadError)
                        break;
                }

                //  If no error, done and return
                if (!hadError)
                    return;
                loops++;
            }

            //  If we made it out of the loop, we weren't able to randomize evos.
            throw new NotImplementedException("Not able to randomize evolutions in a sane amount of retries.");

            Pokemon PickEvoPowerLvlReplacement(IReadOnlyCollection<Pokemon> pool, Pokemon current)
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
                    foreach (var pk in pool)
                    {
                        if (pk.BstForPowerLevels() >= minTarget &&
                            pk.BstForPowerLevels() <= maxTarget &&
                            !canPick.Contains(pk))
                            canPick.Add(pk);
                    }

                    minTarget = minTarget - currentBst / 20;
                    maxTarget = maxTarget + currentBst / 20;
                    expandRounds++;
                }

                return canPick[Random.Next(canPick.Count)];
            }

            bool EvoCycleCheck(Pokemon from, Pokemon to)
            {
                var tempEvo = new Evolution(from, to, false, EvolutionType.None, 0);
                from.EvolutionsFrom.Add(tempEvo);
                var visited = new HashSet<Pokemon>();
                var recStack = new HashSet<Pokemon>();
                var recur = IsCyclic(from, visited, recStack);
                from.EvolutionsFrom.Remove(tempEvo);
                return recur;

                bool IsCyclic(Pokemon pk, ISet<Pokemon> visit, ISet<Pokemon> stack)
                {
                    if (!visit.Contains(pk))
                    {
                        visit.Add(pk);
                        stack.Add(pk);
                        foreach (var ev in pk.EvolutionsFrom)
                        {
                            if (!visit.Contains(ev.To) &&
                                IsCyclic(ev.To, visit, stack))
                                return true;
                            if (stack.Contains(ev.To))
                                return true;
                        }
                    }

                    stack.Remove(pk);
                    return false;
                }
            }

            HashSet<Pokemon> RelatedPokemon(Pokemon original)
            {
                var results = new HashSet<Pokemon> { original };
                var toCheck = new Queue<Pokemon>();
                toCheck.Enqueue(original);
                while (toCheck.Count != 0)
                {
                    var check = toCheck.Dequeue();
                    foreach (var ev in check.EvolutionsFrom)
                    {
                        if (results.Contains(ev.To))
                            continue;

                        results.Add(ev.To);
                        toCheck.Enqueue(ev.To);
                    }

                    foreach (var ev in check.EvolutionsTo)
                    {
                        if (results.Contains(ev.From))
                            continue;

                        results.Add(ev.From);
                        toCheck.Enqueue(ev.From);
                    }
                }

                return results;
            }

            int NumPreEvolutions(Pokemon pk, int maxInterested, int depth = 0)
            {
                if (pk.EvolutionsTo.Count == 0)
                    return 0;
                if (depth == maxInterested - 1)
                    return 1;

                return pk.EvolutionsTo.Max(ev => NumPreEvolutions(ev.From, depth + 1, maxInterested) + 1);
            }
        }


        public void RandomizeTrainerNames(CustomNamesSet customNames)
        {
            if (!RomHandler.CanChangeTrainerText)
                return;

            //  index 0 = singles, 1 = doubles
            List<string>[] allTrainerNames = { new List<string>(), new List<string>() };
            Dictionary<int, List<string>>[] trainerNamesByLength =
            {
                new Dictionary<int, List<string>>(),
                new Dictionary<int, List<string>>()
            };

            AddAllNames(trainerNamesByLength[0], allTrainerNames[0], customNames.TrainerNames);
            AddAllNames(trainerNamesByLength[1], allTrainerNames[1], customNames.DoublesTrainerNames);

            //  Get the current trainer names data
            var currentTrainerNames = RomHandler.TrainerNames;
            if (currentTrainerNames.Length == 0)
                return;

            var mode = RomHandler.TrainerNameMode;
            var maxLength = RomHandler.MaxTrainerNameLength;
            var totalMaxLength = RomHandler.MaxSumOfTrainerNameLengths;
            var success = false;
            var tries = 0;
            //  Init the translation map and new list
            var translation = new Dictionary<string, string>();

            var tcNameLengths = RomHandler.TcNameLengthsByTrainer;
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
                        totalLength = totalLength + RomHandler.InternalStringLength(translation[trainerName]);
                    }
                    else
                    {
                        var idx = trainerName.Contains("&") ? 1 : 0;
                        var pickFrom = allTrainerNames[idx];
                        var intStrLen = RomHandler.InternalStringLength(trainerName);
                        if (mode == TrainerNameMode.SameLength)
                            pickFrom = trainerNamesByLength[idx][intStrLen];

                        var changeTo = trainerName;
                        var ctl = intStrLen;
                        if (pickFrom != null &&
                            pickFrom.Count > 0 &&
                            intStrLen > 1)
                        {
                            var innerTries = 0;
                            changeTo = pickFrom[Random.Next(pickFrom.Count)];
                            ctl = RomHandler.InternalStringLength(changeTo);
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

                                changeTo = pickFrom[Random.Next(pickFrom.Count)];
                                ctl = RomHandler.InternalStringLength(changeTo);
                            }
                        }

                        translation[trainerName] = changeTo;
                        currentTrainerNames[i] = changeTo;
                        totalLength = totalLength + ctl;
                    }

                    if (totalLength <= totalMaxLength)
                        continue;

                    success = false;
                    tries++;
                    break;
                }
            }

            if (!success)
                throw new NotImplementedException(
                    "Could not randomize trainer names in a reasonable amount of attempts." +
                    "\nPlease add some shorter names to your custom trainer names.");
        }

        public void RandomizeTrainerClassNames(CustomNamesSet customNames)
        {
            if (!RomHandler.CanChangeTrainerText)
                return;

            //  index 0 = singles, 1 = doubles
            List<string>[] allTrainerClasses = { new List<string>(), new List<string>() };
            Dictionary<int, List<string>>[] trainerClassesByLength =
            {
                new Dictionary<int, List<string>>(),
                new Dictionary<int, List<string>>()
            };

            AddAllNames(trainerClassesByLength[0], allTrainerClasses[0], customNames.TrainerClasses);
            AddAllNames(trainerClassesByLength[1], allTrainerClasses[1], customNames.DoublesTrainerClasses);

            //  Get the current trainer names data
            var currentClassNames = RomHandler.TrainerClassNames.ToArray();
            var mustBeSameLength = RomHandler.FixedTrainerClassNamesLength;
            var maxLength = RomHandler.MaxTrainerClassNameLength;
            //  Init the translation map and new list
            var translation = new Dictionary<string, string>();
            var numTrainerClasses = currentClassNames.Length;
            var doublesClasses = RomHandler.DoublesTrainerClasses;
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
                    var intStrLen = RomHandler.InternalStringLength(trainerClassName);
                    if (mustBeSameLength)
                        pickFrom = trainerClassesByLength[idx][intStrLen];

                    var changeTo = trainerClassName;
                    if (pickFrom != null &&
                        pickFrom.Count > 0)
                    {
                        changeTo = pickFrom[Random.Next(pickFrom.Count)];
                        while (changeTo.Length > maxLength)
                            changeTo = pickFrom[Random.Next(pickFrom.Count)];
                    }

                    translation[trainerClassName] = changeTo;
                    currentClassNames[i] = changeTo;
                }
            }
        }

        private void AddAllNames(
            IDictionary<int, List<string>> classByLength,
            ICollection<string> classesByNothing,
            IEnumerable<string> trainerClasses)
        {
            foreach (var trainerClassName in trainerClasses)
            {
                classesByNothing.Add(trainerClassName);
                var len = RomHandler.InternalStringLength(trainerClassName);
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
                if (this == obj)
                    return true;

                if (obj == null)
                    return false;

                if (GetType() != obj.GetType())
                    return false;

                var other = (EvolutionPair) obj;
                if (_from == null)
                {
                    if (other._from != null)
                        return false;
                }
                else if (!_from.Equals(other._from))
                {
                    return false;
                }

                if (_to == null)
                {
                    if (other._to != null)
                        return false;
                }
                else if (!_to.Equals(other._to))
                {
                    return false;
                }

                return true;
            }
        }
    }
}