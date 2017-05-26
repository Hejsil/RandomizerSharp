using System;
using System.Collections.Generic;
using System.Linq;
using RandomizerSharp.Constants;
using RandomizerSharp.PokemonModel;
using RandomizerSharp.Properties;
using RandomizerSharp.RomHandlers;

namespace RandomizerSharp.Randomizers
{
    public class UtilityTweacker : BaseRandomizer
    {
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
            { 463, 75 }
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
            { 409, 75 }
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
            { 409, 10 }
        };

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
            { 174, Typing.Ghost }
        };

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
            { 441, 80 }
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
            { 546, 120 }
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
            { 533, 15 }
        };

        public UtilityTweacker(AbstractRomHandler romHandler)
            : base(romHandler)
        {
        }

        public UtilityTweacker(AbstractRomHandler romHandler, Random random)
            : base(romHandler, random)
        {
        }


        public void StandardizeExpCurves()
        {
            var pokes = ValidPokemons;
            foreach (var pkmn in pokes)
            {
                if (pkmn == null)
                    continue;

                pkmn.GrowthExpCurve = pkmn.Legendary ? ExpCurve.Slow : ExpCurve.MediumFast;
            }
        }

        public void OrderDamagingMovesByDamage()
        {
            foreach (var pkmn in ValidPokemons)
            {
                var moves = pkmn.MovesLearnt;
                //  Build up a list of damaging moves and their positions
                var damagingMoveIndices = new List<int>();
                var damagingMoves = new List<Move>();
                for (var i = 0; i < moves.Count; i++)
                {
                    var mv = ValidMoves[moves[i].Move];
                    if (mv.Power > 1)
                    {
                        //  considered a damaging move for this purpose
                        damagingMoveIndices.Add(i);
                        damagingMoves.Add(mv);
                    }
                }

                //  Ties should be sorted randomly, so shuffle the list first.
                damagingMoves.Shuffle(Random);
                //  Sort the damaging moves by power
                damagingMoves.Sort(
                    (m1, m2) =>
                    {
                        if (m1.Power < m2.Power)
                            return -1;
                        if (m1.Power > m2.Power)
                            return 1;
                        return 0;
                    });

                //  Reassign damaging moves in the ordered positions
                for (var i = 0; i < damagingMoves.Count; i++)
                    moves[damagingMoveIndices[i]].Move = damagingMoves[i].Id;
            }
        }

        public void MetronomeOnlyMode()
        {
            var metronomeMl = new MoveLearnt
            {
                Level = 1,
                Move = GlobalConstants.MetronomeMove
            };
            foreach (var pkmn in ValidPokemons)
            {
                pkmn.MovesLearnt.Clear();
                pkmn.MovesLearnt.Add(metronomeMl);
            }

            //  trainers
            //  run this to remove all custom non-Metronome moves
            foreach (var t in RomHandler.Trainers)
            foreach (var tpk in t.Pokemon)
                tpk.ResetMoves = true;

            //  tms
            var tmMoves = RomHandler.TmMoves;
            for (var i = 0; i < tmMoves.Length; i++)
                tmMoves[i] = GlobalConstants.MetronomeMove;

            //  movetutors
            var mtMoves = RomHandler.MoveTutorMoves;
            for (var i = 0; i < mtMoves.Length; i++)
                mtMoves[i] = GlobalConstants.MetronomeMove;

            //  move tweaks
            var metronome = ValidMoves[GlobalConstants.MetronomeMove];
            metronome.Pp = 40;
            var hms = RomHandler.HmMoves;
            foreach (var hm in hms)
            {
                var thisHm = ValidMoves[hm];
                thisHm.Pp = 0;
            }
        }

        public void FullMoveTutorCompatibility()
        {
            foreach (var pkmn in ValidPokemons)
            {
                var flags = pkmn.MoveTutorCompatibility;
                for (var i = 1; i < flags.Length; i++)
                    flags[i] = true;
            }
        }


        public void EnsureMoveTutorCompatSanity()
        {
            //  if a pokemon learns a move in its moveset
            //  and there is a tutor of that move, make sure
            //  that tutor can be learned.
            var mtMoves = RomHandler.MoveTutorMoves;
            foreach (var pkmn in ValidPokemons)
            {
                var moveset = pkmn.MovesLearnt;
                var pkmnCompat = pkmn.MoveTutorCompatibility;
                foreach (var ml in moveset)
                {
                    if (!mtMoves.Contains(ml.Move))
                        continue;

                    var mtIndex = Array.IndexOf(mtMoves, ml.Move);
                    pkmnCompat[mtIndex + 1] = true;
                }
            }
        }


        public void MinimumCatchRate(int rateNonLegendary, int rateLegendary)
        {
            var pokes = ValidPokemons;
            foreach (var pkmn in pokes)
            {
                if (pkmn == null)
                    continue;

                var minCatchRate = pkmn.Legendary ? rateLegendary : rateNonLegendary;

                pkmn.CatchRate = Math.Max(pkmn.CatchRate, minCatchRate);
            }
        }

        public void CondenseLevelEvolutions(int maxLevel, int maxIntermediateLevel)
        {
            var allPokemon = ValidPokemons;
            var changedEvos = new HashSet<Evolution>();
            //  search for level evolutions
            foreach (var pk in allPokemon)
            {
                if (pk == null)
                    continue;

                foreach (var checkEvo in pk.EvolutionsFrom)
                {
                    if (!checkEvo.Type1.UsesLevel())
                        continue;

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
                    {
                        if (!otherEvo.Type1.UsesLevel() || otherEvo.ExtraInfo <= maxIntermediateLevel)
                            continue;

                        otherEvo.ExtraInfo = maxIntermediateLevel;
                        changedEvos.Add(otherEvo);
                    }
                }
            }
        }

        protected void ApplyCamelCaseNames()
        {
            var pokes = ValidPokemons;
            foreach (var pkmn in pokes)
            {
                if (pkmn == null)
                    continue;

                pkmn.Name = RomFunctions.CamelCase(pkmn.Name);
            }
        }

        public void RemoveBrokenMoves()
        {
            foreach (var pokemon in ValidPokemons)
                pokemon.MovesLearnt.RemoveAll(move => Move.GameBreaking.Contains(move.Move));
        }

        public void LevelModifyTrainers(int levelModifier)
        {
            //  Fully random is easy enough - randomize then worry about rival
            //  carrying starter at the end
            foreach (var t in RomHandler.Trainers)
            {
                foreach (var tp in t.Pokemon)
                {
                    if (levelModifier != 0)
                        tp.Level = Math.Min(100, (int) Math.Round(tp.Level * (1 + levelModifier / 100.0)));
                }
            }
        }


        public void ForceFullyEvolvedTrainerPokes(int minLevel)
        {
            foreach (var t in RomHandler.Trainers)
            foreach (var tp in t.Pokemon)
            {
                if (tp.Level < minLevel)
                    continue;

                var newPokemon = FullyEvolve(tp.Pokemon);

                if (ReferenceEquals(newPokemon, tp.Pokemon))
                    continue;

                tp.Pokemon = newPokemon;
                tp.ResetMoves = true;
            }

            Pokemon FullyEvolve(Pokemon pokemon)
            {
                var seenMons = new HashSet<Pokemon> { pokemon };
                while (true)
                {
                    if (pokemon.EvolutionsFrom.Count == 0)
                        break;

                    //  check for cyclic evolutions from what we've already seen
                    var cyclic = false;
                    foreach (var ev in pokemon.EvolutionsFrom)
                    {
                        if (seenMons.Contains(ev.To))
                        {
                            //  cyclic evolution detected - bail now
                            cyclic = true;
                            break;
                        }
                    }

                    if (cyclic)
                        break;

                    //  pick a random evolution to continue from
                    pokemon = pokemon.EvolutionsFrom[Random.Next(pokemon.EvolutionsFrom.Count)].To;
                    seenMons.Add(pokemon);
                }

                return pokemon;
            }
        }

        public void UpdateMovesToGen5()
        {
            foreach (var move in ValidMoves)
            {
                var id = move.Id;

                if (Gen5UpdateMoveType.TryGetValue(id, out var type))
                    move.Type = type;

                if (Gen5UpdateMoveAccuracy.TryGetValue(id, out var accuracy))
                    if (Math.Abs(move.Hitratio - accuracy) >= 1)
                        move.Hitratio = accuracy;

                if (Gen5UpdateMovePower.TryGetValue(id, out var power))
                    move.Power = power;

                if (Gen5UpdateMovePp.TryGetValue(id, out var pp))
                    move.Pp = pp;
            }
        }

        public void UpdateMovesToGen6()
        {
            UpdateMovesToGen5();

            foreach (var move in ValidMoves)
            {
                var id = move.Id;

                if (Gen6UpdateMoveAccuracy.TryGetValue(id, out var accuracy))
                    if (Math.Abs(move.Hitratio - accuracy) >= 1)
                        move.Hitratio = accuracy;

                if (Gen6UpdateMovePower.TryGetValue(id, out var power))
                    move.Power = power;

                if (Gen6UpdateMovePp.TryGetValue(id, out var pp))
                    move.Pp = pp;
            }
        }

        public void RemoveTradeEvolutions(bool changeMoveEvos)
        {
            var extraEvolutions = new HashSet<Evolution>();
            foreach (var pkmn in ValidPokemons)
            {
                if (pkmn == null)
                    continue;

                extraEvolutions.Clear();
                foreach (var evo in pkmn.EvolutionsFrom)
                {
                    if (changeMoveEvos && evo.Type1 == EvolutionType.LevelWithMove)
                    {
                        // read move
                        var move = evo.ExtraInfo;
                        var levelLearntAt = 1;
                        foreach (var ml in evo.From.MovesLearnt)
                        {
                            if (ml.Move == move)
                            {
                                levelLearntAt = ml.Level;
                                break;
                            }
                        }
                        if (levelLearntAt == 1)
                            levelLearntAt = 45;
                        // change to pure level evo
                        evo.Type1 = EvolutionType.Level;
                        evo.ExtraInfo = levelLearntAt;
                    }
                    // Pure Trade
                    if (evo.Type1 == EvolutionType.Trade)
                    {
                        // Replace w/ level 37
                        evo.Type1 = EvolutionType.Level;
                        evo.ExtraInfo = 37;
                    }
                    // Trade w/ TM
                    if (evo.Type1 == EvolutionType.TradeItem)
                    {
                        // Get the current item & evolution
                        var item = evo.ExtraInfo;
                        if (evo.From.Id == Gen5Constants.SlowpokeIndex)
                        {
                            // Slowpoke is awkward - he already has a level evo
                            // So we can't do Level up w/ Held TM for him
                            // Put Water Stone instead
                            evo.Type1 = EvolutionType.Stone;
                            evo.ExtraInfo = Gen5Constants.WaterStoneIndex; // water
                        }
                        else
                        {
                            // Replace, for this entry, w/
                            // Level up w/ Held TM at Day
                            evo.Type1 = EvolutionType.LevelItemDay;
                            // now add an extra evo for
                            // Level up w/ Held TM at Night
                            var extraEntry = new Evolution(
                                evo.From,
                                evo.To,
                                true,
                                EvolutionType.LevelItemNight,
                                item);
                            extraEvolutions.Add(extraEntry);
                        }
                    }
                    if (evo.Type1 == EvolutionType.TradeSpecial)
                    {
                        // This is the karrablast <-> shelmet trade
                        // Replace it with Level up w/ Other Species in Party
                        // (22)
                        // Based on what species we're currently dealing with
                        evo.Type1 = EvolutionType.LevelWithOther;
                        evo.ExtraInfo = evo.From.Id == Gen5Constants.KarrablastIndex
                            ? Gen5Constants.ShelmetIndex
                            : Gen5Constants.KarrablastIndex;
                    }
                }

                foreach (var ev in extraEvolutions)
                {
                    pkmn.EvolutionsFrom.Add(ev);
                    ev.To.EvolutionsTo.Add(ev);
                }
            }
        }

        public void ApplyFastestText()
        {
            switch (RomHandler)
            {
                case Gen5RomHandler gen5:
                    byte[] patch;

                    switch (gen5.Game.GameKind)
                    {
                        case GameEnum.Black2:
                            patch = Resources.b2_instant_text;
                            break;
                        case GameEnum.White2:
                            patch = Resources.w2_instant_text;
                            break;
                        case GameEnum.Black:
                            patch = Resources.b1_instant_text;
                            break;
                        case GameEnum.White:
                            patch = Resources.w1_instant_text;
                            break;

                        default:
                            throw new ArgumentOutOfRangeException(nameof(gen5.Game), gen5.Game, null);
                    }

                    FileFunctions.ApplyPatch(gen5.Arm9, patch);
                    break;
            }
        }
    }
}