using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RandomizerSharp.Constants;
using RandomizerSharp.NDS;
using RandomizerSharp.PokemonModel;
using RandomizerSharp.RomHandlers;

namespace RandomizerSharp.Randomizers
{
    public class MoveRandomizer : BaseRandomizer
    {
        public MoveRandomizer(AbstractRomHandler romHandler)
            : base(romHandler)
        {
        }

        public MoveRandomizer(AbstractRomHandler romHandler, Random random)
            : base(romHandler, random)
        {
        }

        public void RandomizeMovePowers()
        {
            foreach (var mv in ValidMoves)
            {
                if (mv.Id == Move.StruggleId || mv.Power < 10)
                    continue;

                //  "Generic" damaging move to randomize power
                if (Random.Next(3) != 2)
                    mv.Power = Random.Next(11) * 5 + 50;
                else
                    mv.Power = Random.Next(27) * 5 + 20;

                //  Tiny chance for massive power jumps
                for (var i = 0; i < 2; i++)
                {
                    if (Random.Next(100) == 0)
                        mv.Power += 50;
                }
            }
        }


        public void RandomizeMovePPs()
        {
            foreach (var mv in ValidMoves)
            {
                if (mv == null || mv.Id == 165)
                    continue;

                if (Random.Next(3) != 2)
                    mv.Pp = Random.Next(3) * 5 + 15;
                else
                    mv.Pp = Random.Next(8) * 5 + 5;
            }
        }

        public void RandomizeMoveAccuracies()
        {
            foreach (var mv in ValidMoves)
            {
                if (mv == null || mv.Id == 165 || !(mv.Hitratio >= 5))
                    continue;

                if (mv.Hitratio <= 50)
                {
                    //  lowest tier (acc <= 50)
                    //  new accuracy = rand(20...50) inclusive
                    //  with a 10% chance to increase by 50%
                    mv.Hitratio = Random.Next(7) * 5 + 20;
                    if (Random.Next(10) == 0)
                        mv.Hitratio = mv.Hitratio * 1 / (5 * 5);
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
                        if (Random.Next(10) < 2)
                            break;

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
                        if (Random.Next(10) < 4)
                            break;

                        mv.Hitratio -= 5;
                    }
                }
            }
        }

        public void RandomizeMoveCategory()
        {
            if (!RomHandler.Game.HasPhysicalSpecialSplit)
                return;

            foreach (var mv in ValidMoves)
            {
                if (mv.Id != 165 && mv.Category != MoveCategory.Status)
                    mv.Category = (MoveCategory) Random.Next(2);
            }
        }

        public void RandomizeMovesLearnt(
            bool typeThemed,
            bool noBroken,
            bool forceFourStartingMoves,
            double goodDamagingProbability)
        {
            //  Get current sets
            var hms = RomHandler.Machines
                .Where(machine => machine.Type == Machine.Kind.Hidden)
                .Select(machine => machine.Move);

            var allBanned = new HashSet<Move>(noBroken ? RomHandler.Moves.Where(move => Move.GameBreaking.Contains(move.Id)) : Enumerable.Empty<Move>());
            allBanned.UnionWith(hms);

            //  Build sets of moves
            var validMoves = new List<Move>();
            var validDamagingMoves = new List<Move>();
            var validTypeMoves = new Dictionary<Typing, List<Move>>();
            var validTypeDamagingMoves = new Dictionary<Typing, List<Move>>();
            foreach (var mv in ValidMoves)
            {
                if (mv == null || GlobalConstants.BannedRandomMoves[mv.Id] || allBanned.Contains(mv))
                    continue;

                validMoves.Add(mv);

                if (mv.Type != null)
                {
                    if (!validTypeMoves.ContainsKey(mv.Type))
                        validTypeMoves[mv.Type] = new List<Move>();

                    validTypeMoves[mv.Type].Add(mv);
                }

                if (GlobalConstants.BannedForDamagingMove[mv.Id])
                    continue;

                if (mv.Power < 2 * GlobalConstants.MinDamagingMovePower && (mv.Power < GlobalConstants.MinDamagingMovePower || !(mv.Hitratio >= 90)))
                    continue;

                validDamagingMoves.Add(mv);

                if (mv.Type == null)
                    continue;

                if (!validTypeDamagingMoves.ContainsKey(mv.Type))
                    validTypeDamagingMoves[mv.Type] = new List<Move>();

                validTypeDamagingMoves[mv.Type].Add(mv);
            }

            foreach (var pokemon in ValidPokemons)
            {
                var learnt = new HashSet<int>();
                var moves = pokemon.MovesLearnt;
                //  4 starting moves?
                if (forceFourStartingMoves)
                {
                    var lv1Count = moves.Count(ml => ml.Level == 1);

                    if (lv1Count < 4)
                        for (var i = 0; i < 4 - lv1Count; i++)
                        {
                            var fakeLv1 = new MoveLearnt
                            {
                                Level = 1,
                                Move = RomHandler.Moves[0]
                            };

                            moves.Insert(0, fakeLv1);
                        }
                }

                //  Find last lv1 move
                //  lv1index ends up as the index of the first non-lv1 move
                var lv1Index = 0;
                while (lv1Index < moves.Count &&
                       moves[lv1Index].Level == 1)
                    lv1Index++;

                //  last lv1 move is 1 before lv1index
                if (lv1Index != 0)
                    lv1Index--;

                //  Replace moves as needed
                for (var i = 0; i < moves.Count; i++)
                {
                    //  should this move be forced damaging?
                    var attemptDamaging = i == lv1Index || Random.NextDouble() < goodDamagingProbability;

                    //  type themed?
                    Typing typeOfMove = null;
                    if (typeThemed)
                    {
                        var picked = Random.NextDouble();
                        if (pokemon.PrimaryType == Typing.Normal ||
                            pokemon.SecondaryType == Typing.Normal)
                        {
                            if (pokemon.SecondaryType == null)
                            {
                                //  Pure NORMAL: 75% normal, 25% random
                                if (picked < 0.75)
                                    typeOfMove = Typing.Normal;

                                //  else random
                            }
                            else
                            {
                                //  Find the other type
                                //  Normal/OTHER: 30% normal, 55% other, 15% random
                                var otherType = pokemon.PrimaryType;
                                if (otherType == Typing.Normal)
                                    otherType = pokemon.SecondaryType;

                                if (picked < 0.3)
                                    typeOfMove = Typing.Normal;
                                else if (picked < 0.85)
                                    typeOfMove = otherType;

                                //  else random
                            }
                        }
                        else if (pokemon.SecondaryType != null)
                        {
                            //  Primary/Secondary: 50% primary, 30% secondary, 5%
                            //  normal, 15% random
                            if (picked < 0.5)
                                typeOfMove = pokemon.PrimaryType;
                            else if (picked < 0.8)
                                typeOfMove = pokemon.SecondaryType;
                            else if (picked < 0.85)
                                typeOfMove = Typing.Normal;

                            //  else random
                        }
                        else
                        {
                            //  Primary/None: 60% primary, 20% normal, 20% random
                            if (picked < 0.6)
                                typeOfMove = pokemon.PrimaryType;
                            else if (picked < 0.8)
                                typeOfMove = Typing.Normal;

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
                            else if (CheckForUnusedMove(validDamagingMoves, learnt))
                                pickList = validDamagingMoves;
                        }
                        else if (CheckForUnusedMove(validDamagingMoves, learnt))
                        {
                            pickList = validDamagingMoves;
                        }
                    }
                    else if (typeOfMove != null)
                    {
                        if (validTypeMoves.ContainsKey(typeOfMove) &&
                            CheckForUnusedMove(validTypeMoves[typeOfMove], learnt))
                            pickList = validTypeMoves[typeOfMove];
                    }

                    //  now pick a move until we get a valid one
                    var mv = pickList[Random.Next(pickList.Count)];
                    while (learnt.Contains(mv.Id))
                        mv = pickList[Random.Next(pickList.Count)];

                    //  write it
                    moves[i].Move = mv;
                    if (i == lv1Index)
                        moves[i].Level = 1;

                    learnt.Add(mv.Id);
                }
            }

            bool CheckForUnusedMove(List<Move> potentialList, HashSet<int> alreadyUsed)
            {
                foreach (var mv in potentialList)
                {
                    if (!alreadyUsed.Contains(mv.Id))
                        return true;
                }

                return false;
            }
        }

        public void RandomizeTmMoves(
            bool noBroken,
            bool preserveField,
            bool levelupTmMoveSanity,
            double goodDamagingProbability)
        {
            //  Get current sets
            var hms = RomHandler.Machines
                .Where(machine => machine.Type == Machine.Kind.Hidden)
                .Select(machine => machine.Move).ToArray();

            //  Get current sets
            var tms = RomHandler.Machines
                .Where(machine => machine.Type == Machine.Kind.Technical)
                .Select(machine => machine.Move).ToArray();

            var banned = new List<Move>(noBroken ? RomHandler.Moves.Where(move => Move.GameBreaking.Contains(move.Id)) : Enumerable.Empty<Move>());

            //  field moves?
            var fieldMoves = RomHandler.Moves.Where(move => RomHandler.Game.FieldMoves.Contains(move.Id));
            var preservedFieldMoveCount = 0;
            if (preserveField)
            {
                var banExistingField = new List<Move>(tms);
                banExistingField.RemoveAll(i => !fieldMoves.Contains(i));
                preservedFieldMoveCount = banExistingField.Count;
                banned.AddRange(banExistingField);
            }

            //  Determine which moves are pickable
            var usableMoves = ValidMoves.ToList();
            usableMoves.RemoveAt(0);

            //  remove null entry
            var unusableMoves = new HashSet<Move>();
            var unusableDamagingMoves = new HashSet<Move>();
            foreach (var mv in usableMoves)
            {
                if (GlobalConstants.BannedRandomMoves[mv.Id] ||
                    hms.Contains(mv) ||
                    banned.Contains(mv))
                    unusableMoves.Add(mv);
                else if (GlobalConstants.BannedForDamagingMove[mv.Id] ||
                         mv.Power < GlobalConstants.MinDamagingMovePower)
                    unusableDamagingMoves.Add(mv);
            }

            usableMoves.RemoveAll(unusableMoves);
            var usableDamagingMoves = new List<Move>(usableMoves);
            usableDamagingMoves.RemoveAll(unusableDamagingMoves.Contains);

            //  pick (tmCount - preservedFieldMoveCount) moves
            var pickedMoves = new List<Move>();
            for (var i = 0; i < RomHandler.Machines.Count - preservedFieldMoveCount; i++)
            {
                Move chosenMove;
                if (Random.NextDouble() < goodDamagingProbability &&
                    usableDamagingMoves.Count > 0)
                    chosenMove = usableDamagingMoves[Random.Next(usableDamagingMoves.Count)];
                else
                    chosenMove = usableMoves[Random.Next(usableMoves.Count)];

                pickedMoves.Add(chosenMove);
                usableMoves.Remove(chosenMove);
                usableDamagingMoves.Remove(chosenMove);
            }

            //  shuffle the picked moves because high goodDamagingProbability
            //  could bias them towards early numbers otherwise
            pickedMoves.Shuffle(Random);

            //  finally, distribute them as tms
            var pickedMoveIndex = 0;

            for (var i = 0; i < RomHandler.Machines.Count; i++)
            {
                if (preserveField && fieldMoves.Contains(tms[i]))
                    continue;

                RomHandler.Machines[i].Move = pickedMoves[pickedMoveIndex++];
            }

            if (!levelupTmMoveSanity)
                return;

            //  if a pokemon learns a move in its moveset
            //  and there is a TM of that move, make sure
            //  that TM can be learned.
            var movesFromMachines = new HashSet<Move>(RomHandler.Machines.Select(m => m.Move));
            foreach (var pokemon in ValidPokemons)
            {
                var moveset = pokemon.MovesLearnt;
                var pkmnCompat = pokemon.TMHMCompatibility;
                foreach (var ml in moveset)
                {
                    if (!movesFromMachines.Contains(ml.Move))
                        continue;

                    var learnt = pkmnCompat.First(machineL => machineL.Machine.Move == ml.Move);
                    learnt.Learns = true;
                }
            }
        }

        public void RandomizeTmhmCompatibility(TmsHmsCompatibility compatibility)
        {
            //  Get current sets
            var tmHMs = RomHandler.Machines
                .Select(machine => machine.Move).ToList();

            //  Get current compatibility
            //  new: increase HM chances if required early on
            var earlies = new HashSet<int>(RomHandler.Game.EarlyRequiredHmMoves);
            var requiredEarlyOn = new HashSet<Move>(tmHMs.Where(move => earlies.Contains(move.Id)));

            foreach (var pkmn in ValidPokemons)
            {
                var flags = pkmn.TMHMCompatibility;
                for (var i = 0; i < tmHMs.Count; i++)
                {
                    var move = tmHMs[i];
                    var probability = 0.5;
                    if (compatibility == TmsHmsCompatibility.RandomPreferType)
                        if (pkmn.PrimaryType.Equals(move.Type) ||
                            pkmn.SecondaryType != null && pkmn.SecondaryType.Equals(move.Type))
                            probability = 0.9;
                        else if (move.Type != null &&
                                 move.Type.Equals(Typing.Normal))
                            probability = 0.5;
                        else
                            probability = 0.25;

                    if (requiredEarlyOn.Contains(move))
                        probability = Math.Min(1, probability * 1.8);

                    flags[i].Learns = Random.NextDouble() < probability;
                }
            }

            if (compatibility != TmsHmsCompatibility.Full)
                return;

            //  Get current sets
            var tmCount = RomHandler.Machines.Count(move => move.Type == Machine.Kind.Technical);
            foreach (var pokemon in ValidPokemons)
            {
                var flags = pokemon.TMHMCompatibility;
                for (var i = tmCount + 1; i < flags.Length; i++)
                    flags[i].Learns = true;
            }
        }


        public void RandomizeMoveTutorMoves(bool noBroken, bool preserveField, double goodDamagingProbability)
        {

            //  Get current sets
            var hms = RomHandler.Machines
                .Where(machine => machine.Type == Machine.Kind.Hidden)
                .Select(machine => machine.Move).ToArray();
            //  Get current sets
            var tms = RomHandler.Machines
                .Where(machine => machine.Type == Machine.Kind.Technical)
                .Select(machine => machine.Move).ToArray();

            //  Pick some random Move Tutor moves, excluding TMs.
            var oldMTs = RomHandler.MoveTutorMoves;
            var mtCount = oldMTs.Length;

            var banned = new List<Move>(noBroken ? RomHandler.Moves.Where(move => Move.GameBreaking.Contains(move.Id)) : Enumerable.Empty<Move>());

            //  field moves?
            var fieldMoves = RomHandler.Moves.Where(move => RomHandler.Game.FieldMoves.Contains(move.Id));
            var preservedFieldMoveCount = 0;
            if (preserveField)
            {
                var banExistingField = new List<Move>(oldMTs);
                banExistingField.RemoveAll(i => !fieldMoves.Contains(i));
                preservedFieldMoveCount = banExistingField.Count;
                banned.AddRange(banExistingField);
            }

            //  Determine which moves are pickable
            var usableMoves = ValidMoves.ToList();
            usableMoves.RemoveAt(0);
            //  remove null entry
            var unusableMoves = new HashSet<Move>();
            var unusableDamagingMoves = new HashSet<Move>();
            foreach (var mv in usableMoves)
            {
                if (GlobalConstants.BannedRandomMoves[mv.Id] ||
                    tms.Contains(mv) ||
                    hms.Contains(mv) ||
                    banned.Contains(mv))
                    unusableMoves.Add(mv);
                else if (GlobalConstants.BannedForDamagingMove[mv.Id] ||
                         mv.Power < GlobalConstants.MinDamagingMovePower)
                    unusableDamagingMoves.Add(mv);
            }

            usableMoves.RemoveAll(unusableMoves.Contains);
            var usableDamagingMoves = new List<Move>(usableMoves);
            usableDamagingMoves.RemoveAll(unusableDamagingMoves.Contains);

            //  pick (tmCount - preservedFieldMoveCount) moves
            var pickedMoves = new List<Move>();
            for (var i = 0;
                i < mtCount - preservedFieldMoveCount;
                i++)
            {
                Move chosenMove;
                if (Random.NextDouble() < goodDamagingProbability &&
                    usableDamagingMoves.Count > 0)
                    chosenMove = usableDamagingMoves[Random.Next(usableDamagingMoves.Count)];
                else
                    chosenMove = usableMoves[Random.Next(usableMoves.Count)];

                pickedMoves.Add(chosenMove);
                usableMoves.Remove(chosenMove);
                usableDamagingMoves.Remove(chosenMove);
            }

            //  shuffle the picked moves because high goodDamagingProbability
            //  could bias them towards early numbers otherwise
            pickedMoves.Shuffle(Random);

            //  finally, distribute them as tutors
            var pickedMoveIndex = 0;
            for (var i = 0; i < mtCount; i++)
            {
                if (preserveField && fieldMoves.Contains(oldMTs[i]))
                    continue;

                oldMTs[i] = pickedMoves[pickedMoveIndex++];
            }
        }

        public void RandomizeMoveTutorCompatibility(bool preferSameType)
        {
            //  Get current compatibility
            var mts = RomHandler.MoveTutorMoves;
            foreach (var pkmn in ValidPokemons)
            {
                var flags = pkmn.MoveTutorCompatibility;
                for (var i = 0; i < mts.Length; i++)
                {
                    var mv = mts[i];
                    var probability = 0.5;
                    if (preferSameType)
                        if (pkmn.PrimaryType.Equals(mv.Type) ||
                            pkmn.SecondaryType != null && pkmn.SecondaryType.Equals(mv.Type))
                            probability = 0.9;
                        else if (mv.Type != null &&
                                 mv.Type.Equals(Typing.Normal))
                            probability = 0.5;
                        else
                            probability = 0.25;

                    flags[i] = Random.NextDouble() < probability;
                }
            }
        }
    }
}