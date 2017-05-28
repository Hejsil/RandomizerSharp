using System;
using System.Collections.Generic;
using System.Linq;
using RandomizerSharp.Constants;
using RandomizerSharp.PokemonModel;
using RandomizerSharp.RomHandlers;

namespace RandomizerSharp.Randomizers
{
    public class TrainerRandomizer : BaseRandomizer
    {
        public TrainerRandomizer(AbstractRomHandler romHandler)
            : base(romHandler)
        {
        }

        public TrainerRandomizer(AbstractRomHandler romHandler, Random random)
            : base(romHandler, random)
        {
        }

        public void RandomizeTrainerPokes(
            bool usePowerLevels,
            bool noLegendaries,
            bool noEarlyWonderGuard,
            bool rivaleCarriesStarter)
        {
            //  Fully random is easy enough - randomize then worry about rival
            //  carrying starter at the end
            foreach (var t in RomHandler.Trainers)
            {
                //if (t.Tag != null && t.Tag.Equals("IRIVAL"))
                //    continue;

                foreach (var tp in t.Pokemon)
                {
                    var wgAllowed = !noEarlyWonderGuard || tp.Level >= 20;
                    tp.Pokemon = PickReplacement(tp.Pokemon, usePowerLevels, null, noLegendaries, !wgAllowed);
                    tp.ResetMoves(RomHandler.Moves[0]);
                }
            }

            if (!rivaleCarriesStarter)
                return;

            RivalCarriesStarterUpdate(RomHandler.Trainers, "RIVAL", 1);
            RivalCarriesStarterUpdate(RomHandler.Trainers, "FRIEND", 2);

            void RivalCarriesStarterUpdate(IEnumerable<Trainer> currentTrainers, string prefix, int pokemonOffset)
            {
                var enumerable = currentTrainers as Trainer[] ?? currentTrainers.ToArray();
                //  Find the highest rival battle #
                var highestRivalNum = enumerable
                    .Where(t => t.Tag != null && t.Tag.StartsWith(prefix))
                    .Max(t => int.Parse(t.Tag.Substring(prefix.Length, t.Tag.IndexOf('-') - prefix.Length)));

                if (highestRivalNum == 0)
                    return;

                //  Get the starters
                //  us 0 1 2 => them 0+n 1+n 2+n
                var starters = RomHandler.Starters;
                //  Yellow needs its own case, unfortunately.
                if (RomHandler.Game == Game.Yellow)
                {
                    //  The rival's starter is index 1
                    var rivalStarter = starters[1].Pokemon;
                    var timesEvolves = NumEvolutions(rivalStarter, 2);
                    //  Apply evolutions as appropriate
                    switch (timesEvolves)
                    {
                        case 0:
                            for (var j = 1; j <= 3; j++)
                                ChangeStarterWithTag(enumerable, prefix + (j + "-0"), rivalStarter);

                            for (var j = 4; j <= 7; j++)
                            for (var i = 0; i < 3; i++)
                                ChangeStarterWithTag(enumerable, prefix + (j + ("-" + i)), rivalStarter);

                            break;
                        case 1:
                            for (var j = 1; j <= 3; j++)
                                ChangeStarterWithTag(enumerable, prefix + (j + "-0"), rivalStarter);

                            rivalStarter = PickRandomEvolutionOf(rivalStarter, false);

                            for (var j = 4; j <= 7; j++)
                            for (var i = 0; i < 3; i++)
                                ChangeStarterWithTag(enumerable, prefix + (j + ("-" + i)), rivalStarter);

                            break;
                        case 2:
                            for (var j = 1; j <= 2; j++)
                                ChangeStarterWithTag(enumerable, prefix + (j + ("-" + 0)), rivalStarter);

                            rivalStarter = PickRandomEvolutionOf(rivalStarter, true);
                            ChangeStarterWithTag(enumerable, prefix + "3-0", rivalStarter);

                            for (var i = 0; i < 3; i++)
                                ChangeStarterWithTag(enumerable, prefix + ("4-" + i), rivalStarter);

                            rivalStarter = PickRandomEvolutionOf(rivalStarter, false);

                            for (var j = 5; j <= 7; j++)
                            for (var i = 0; i < 3; i++)
                                ChangeStarterWithTag(enumerable, prefix + (j + ("-" + i)), rivalStarter);

                            break;
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
                                if (GetLevelOfStarter(enumerable, prefix + (j + ("-" + i))) >= 30)
                                    break;

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
                                if (GetLevelOfStarter(enumerable, prefix + (j + ("-" + i))) >= 16)
                                    break;

                                ChangeStarterWithTag(enumerable, prefix + (j + ("-" + i)), thisStarter);
                            }

                            thisStarter = PickRandomEvolutionOf(thisStarter, true);

                            for (; j <= highestRivalNum; j++)
                            {
                                if (GetLevelOfStarter(enumerable, prefix + (j + ("-" + i))) >= 36)
                                    break;

                                ChangeStarterWithTag(enumerable, prefix + (j + ("-" + i)), thisStarter);
                            }

                            thisStarter = PickRandomEvolutionOf(thisStarter, false);

                            for (; j <= highestRivalNum; j++)
                                ChangeStarterWithTag(enumerable, prefix + (j + ("-" + i)), thisStarter);
                        }
                    }
                }

                int NumEvolutions(Pokemon pk, int maxInterested, int depth = 0)
                {
                    if (pk.EvolutionsFrom.Count == 0)
                        return 0;
                    if (depth == maxInterested - 1)
                        return 1;

                    return pk.EvolutionsFrom.Max(ev => NumEvolutions(ev.To, depth + 1, maxInterested) + 1);
                }
            }

            void ChangeStarterWithTag(IEnumerable<Trainer> currentTrainers, string tag, Pokemon starter)
            {
                foreach (var t in currentTrainers)
                {
                    if (t.Tag == null || !t.Tag.Equals(tag))
                        continue;

                    //  Bingo
                    //  Change the highest level pokemon, not the last.
                    //  BUT: last gets +2 lvl priority (effectively +1)
                    //  same as above, equal priority = earlier wins
                    var bestPoke = t.Pokemon[0];
                    var trainerPkmnCount = t.Pokemon.Length;
                    for (var i = 1; i < trainerPkmnCount; i++)
                    {
                        var levelBonus = i == trainerPkmnCount - 1 ? 2 : 0;

                        if (t.Pokemon[i].Level + levelBonus > bestPoke.Level)
                            bestPoke = t.Pokemon[i];
                    }

                    bestPoke.Pokemon = starter;
                    bestPoke.ResetMoves(RomHandler.Moves[0]);
                }
            }

            int GetLevelOfStarter(IEnumerable<Trainer> currentTrainers, string tag)
            {
                foreach (var t in currentTrainers)
                {
                    if (t.Tag == null || !t.Tag.Equals(tag))
                        continue;

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
                        if (t.Pokemon[i].Level + levelBonus > highestLevel)
                            highestLevel = t.Pokemon[i].Level;
                    }

                    return highestLevel;
                }

                return 0;
            }

            Pokemon PickRandomEvolutionOf(Pokemon basePokemon, bool mustEvolveItself)
            {
                //  Used for "rival carries starter"
                //  Pick a random evolution of base Pokemon, subject to
                //  "must evolve itself" if appropriate.
                var candidates = new List<Pokemon>();

                foreach (var ev in basePokemon.EvolutionsFrom)
                {
                    if (!mustEvolveItself ||
                        ev.To.EvolutionsFrom.Count > 0)
                        candidates.Add(ev.To);
                }

                if (candidates.Count == 0)
                    throw new NotImplementedException(
                        "Random evolution called on a Pokemon without any usable evolutions.");

                return candidates[Random.Next(candidates.Count)];
            }
        }

        public void TypeThemeTrainerPokes(
            bool usePowerLevels,
            bool weightByFrequency,
            bool noLegendaries,
            bool noEarlyWonderGuard,
            int levelModifier = 0)
        {
            var currentTrainers = RomHandler.Trainers;

            //  Construct groupings for types
            //  Anything starting with GYM or ELITE or CHAMPION is a group
            var assignedTrainers = new HashSet<Trainer>();
            var groups = new Dictionary<string, List<Trainer>>();
            foreach (var t in currentTrainers)
            {
                if (t.Tag != null &&
                    t.Tag.Equals("IRIVAL"))
                    continue;

                var group = t.Tag ?? "";

                if (group.Contains("-"))
                    group = group.Substring(0, group.IndexOf('-'));

                if (group.StartsWith("GYM") ||
                    group.StartsWith("ELITE") ||
                    group.StartsWith("CHAMPION") ||
                    group.StartsWith("THEMED"))
                {
                    //  Yep this is a group
                    if (!groups.ContainsKey(group))
                        groups[group] = new List<Trainer>();

                    groups[group].Add(t);
                    assignedTrainers.Add(t);
                }
                else if (group.StartsWith("GIO"))
                {
                    //  Giovanni has same grouping as his gym, gym 8
                    if (!groups.ContainsKey("GYM8"))
                        groups["GYM8"] = new List<Trainer>();

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
                trainersInGroup.Shuffle(Random);
                var typeForGroup = RandomType();
                if (group.StartsWith("GYM"))
                {
                    while (usedGymTypes.Contains(typeForGroup))
                        typeForGroup = RandomType();

                    usedGymTypes.Add(typeForGroup);
                }

                if (group.StartsWith("ELITE"))
                {
                    while (usedEliteTypes.Contains(typeForGroup))
                        typeForGroup = RandomType();

                    usedEliteTypes.Add(typeForGroup);
                }

                if (group.Equals("CHAMPION"))
                    usedUberTypes.Add(typeForGroup);

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
                        !wgAllowed);

                    tp.ResetMoves(RomHandler.Moves[0]);

                    if (levelModifier != 0)
                        tp.Level = Math.Min(100, (int) Math.Round(tp.Level * (1 + levelModifier / 100.0)));
                }
            }

            //  New: randomize the order trainers are randomized in.
            //  Leads to less predictable results for various modifiers.
            //  Need to keep the original ordering around for saving though.
            var scrambledTrainers = new List<Trainer>(currentTrainers);
            scrambledTrainers.Shuffle(Random);
            //  Give a type to each unassigned trainer
            foreach (var t in scrambledTrainers)
            {
                if (t.Tag != null &&
                    t.Tag.Equals("IRIVAL"))
                    continue;

                if (assignedTrainers.Contains(t))
                    continue;

                var typeForTrainer = RandomType();
                //  Ubers: can't have the same type as each other
                if (t.Tag != null &&
                    t.Tag.Equals("UBER"))
                {
                    while (usedUberTypes.Contains(typeForTrainer))
                        typeForTrainer = RandomType();

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
                        !shedAllowed);

                    tp.ResetMoves(RomHandler.Moves[0]);

                    if (levelModifier != 0)
                        tp.Level = Math.Min(100, (int) Math.Round(tp.Level * (1 + levelModifier / 100.0)));
                }
            }
        }


        private Pokemon PickReplacement(
            Pokemon current,
            bool usePowerLevels,
            Typing type,
            bool noLegendaries,
            bool noWonderGuard)
        {
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
                    foreach (var pk in ValidPokemons)
                    {
                        if (pk.PrimaryType != type && pk.SecondaryType != type)
                            continue;
                        if (noLegendaries && pk.Legendary)
                            continue;
                        if (noWonderGuard && HasWonderGuard(pk))
                            continue;
                        
                        var bst = pk.BstForPowerLevels();

                        if (bst >= minTarget && bst <= maxTarget)
                            canPick.Add(pk);
                    }

                    minTarget = minTarget - currentBst / 20;
                    maxTarget = maxTarget + currentBst / 20;
                    expandRounds++;
                }

                return canPick[Random.Next(canPick.Count)];
            }

            {
                Pokemon pk;

                while (true)
                {
                    pk = ValidPokemons[Random.Next(ValidPokemons.Count)];

                    if (pk.PrimaryType != type && pk.SecondaryType != type)
                        continue;
                    if (noLegendaries && pk.Legendary)
                        continue;
                    if (noWonderGuard && HasWonderGuard(pk))
                        continue;

                    break;
                }

                return pk;
            }
        }

        private static bool HasWonderGuard(Pokemon pk) => pk.Ability1.Id == GlobalConstants.WonderGuardIndex ||
                                                          pk.Ability2.Id == GlobalConstants.WonderGuardIndex ||
                                                          pk.Ability3.Id == GlobalConstants.WonderGuardIndex;
    }
}