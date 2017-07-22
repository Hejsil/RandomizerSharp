using System;
using System.Collections.Generic;
using System.Linq;
using RandomizerSharp.Constants;
using RandomizerSharp.PokemonModel;
using RandomizerSharp.RomHandlers;

namespace RandomizerSharp.Randomizers
{
    public class WildRandomizer : BaseRandomizer
    {
        public WildRandomizer(AbstractRomHandler romHandler)
            : base(romHandler)
        {
        }

        public WildRandomizer(AbstractRomHandler romHandler, Random random)
            : base(romHandler, random)
        {
        }

        public void RandomEncounters(EncountersRandomization encountersRandomization, bool noLegendaries)
        {
            var pickFrom = noLegendaries ? ValidPokemons.Where(p => !p.Legendary) : ValidPokemons;
            var enumerable = pickFrom as Pokemon[] ?? pickFrom.ToArray();

            var currentEncounters = RomHandler.Encounters.ToList();
            currentEncounters.Shuffle(Random);
            
            switch (encountersRandomization)
            {
                case EncountersRandomization.CatchEmAll:
                    // Clone, so we don't modify original
                    var allPokes = enumerable.ToList();

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
                                var tempPickable = enumerable.ToList();
                                tempPickable.RemoveAll(area.BannedPokemon);

                                if (tempPickable.Count == 0)
                                    throw new NotImplementedException("ERROR: Couldn't replace a wild Pokemon!");

                                enc.Pokemon1 = tempPickable[Random.Next(tempPickable.Count)];
                            }
                            else
                            {
                                var picked = Random.Next(pickablePokemon.Count);
                                enc.Pokemon1 = pickablePokemon[picked];

                                pickablePokemon.RemoveAt(picked);

                                if (allPokes != pickablePokemon)
                                    allPokes.Remove(enc.Pokemon1);

                                if (allPokes.Count != 0)
                                    continue;

                                allPokes.AddRange(enumerable);

                                if (pickablePokemon == allPokes)
                                    continue;

                                pickablePokemon.AddRange(allPokes);

                                pickablePokemon.RemoveAll(area.BannedPokemon);
                            }
                        }
                    }
                    break;
                case EncountersRandomization.TypeThemed:
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
                            throw new NotImplementedException(
                                "Could not randomize an area in a reasonable amount of attempts.");
                        }
                        foreach (var enc in area.Encounters)
                        {
                            enc.Pokemon1 = possiblePokemon[Random.Next(possiblePokemon.Count)];
                        }
                    }
                    break;
                case EncountersRandomization.UsePowerLevel:
                    var allowedPokes = enumerable.ToList();

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
                            enc.Pokemon1 = PickWildPowerLvlReplacement(localAllowed, enc.Pokemon1, false, null);
                        }
                    }
                    break;
                default:
                    foreach (var area in currentEncounters)
                    {
                        foreach (var enc in area.Encounters)
                        {
                            enc.Pokemon1 = noLegendaries ? RandomPokemon(p => !p.Legendary) : RandomPokemon();
                            while (area.BannedPokemon.Contains(enc.Pokemon1))
                            {
                                enc.Pokemon1 = noLegendaries ? RandomPokemon(p => !p.Legendary) : RandomPokemon();
                            }
                        }
                    }
                    break;
            }
        }

        public void Game1To1Encounters(bool useTimeOfDay, bool usePowerLevels, bool noLegendaries)
        {
            var pickFrom = noLegendaries ? ValidPokemons.Where(p => !p.Legendary) : ValidPokemons;
            var enumerable = pickFrom as Pokemon[] ?? pickFrom.ToArray();

            //  Build the full 1-to-1 map
            var translateMap = new Dictionary<Pokemon, Pokemon>();
            var remainingLeft = ValidPokemons.ToList();
            var remainingRight = enumerable.ToList();

            while (remainingLeft.Count != 0)
            {
                if (usePowerLevels)
                {
                    var pickedLeft = Random.Next(remainingLeft.Count);
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
                    var pickedLeft = Random.Next(remainingLeft.Count);
                    var pickedRight = Random.Next(remainingRight.Count);
                    var pickedLeftP = remainingLeft[pickedLeft];
                    var pickedRightP = remainingRight[pickedRight];
                    remainingLeft.RemoveAt(pickedLeft);
                    while (pickedLeftP.Id == pickedRightP.Id &&
                           remainingRight.Count != 1)
                    {
                        //  Reroll for a different pokemon if at all possible
                        pickedRight = Random.Next(remainingRight.Count);
                        pickedRightP = remainingRight[pickedRight];
                    }

                    remainingRight.RemoveAt(pickedRight);
                    translateMap[pickedLeftP] = pickedRightP;
                }

                if (remainingRight.Count != 0)
                    continue;

                remainingRight.AddRange(enumerable);
            }

            //  Map remaining to themselves just in case
            var allPokes = ValidPokemons;
            foreach (var poke in allPokes)
            {
                if (!translateMap.ContainsKey(poke))
                    translateMap[poke] = poke;
            }

            foreach (var area in RomHandler.Encounters)
            foreach (var enc in area.Encounters)
            {
                //  Apply the map
                enc.Pokemon1 = translateMap[enc.Pokemon1];

                if (!area.BannedPokemon.Contains(enc.Pokemon1))
                    continue;

                //  Ignore the map and put a random non-banned poke
                var tempPickable = enumerable.ToList();
                tempPickable.RemoveAll(area.BannedPokemon.Contains);

                if (tempPickable.Count == 0)
                    throw new NotImplementedException("ERROR: Couldn\'t replace a wild Pokemon!");

                if (usePowerLevels)
                {
                    enc.Pokemon1 = PickWildPowerLvlReplacement(tempPickable, enc.Pokemon1, false, null);
                }
                else
                {
                    var picked = Random.Next(tempPickable.Count);
                    enc.Pokemon1 = tempPickable[picked];
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
            var pickFrom = noLegendaries ? ValidPokemons.Where(p => !p.Legendary) : ValidPokemons;
            var enumerable = pickFrom as Pokemon[] ?? pickFrom.ToArray();

            //  New: randomize the order encounter sets are randomized in.
            //  Leads to less predictable results for various modifiers.
            //  Need to keep the original ordering around for saving though.
            var scrambledEncounters = new List<EncounterSet>(RomHandler.Encounters);
            scrambledEncounters.Shuffle(Random);

            //  Assume EITHER catch em all OR type themed for now
            if (catchEmAll)
            {
                var allPokes = enumerable.ToList();

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
                    {
                        if (pickablePokemon.Count == 0)
                        {
                            //  No more pickable pokes left, take a random one
                            var tempPickable = enumerable.ToList();
                            tempPickable.RemoveAll(area.BannedPokemon.Contains);

                            if (tempPickable.Count == 0)
                                throw new NotImplementedException("ERROR: Couldn\'t replace a wild Pokemon!");

                            var picked = Random.Next(tempPickable.Count);
                            var pickedMn = tempPickable[picked];
                            areaMap[areaPk] = pickedMn;
                        }
                        else
                        {
                            var picked = Random.Next(allPokes.Count);
                            var pickedMn = allPokes[picked];
                            areaMap[areaPk] = pickedMn;
                            pickablePokemon.Remove(pickedMn);
                            if (allPokes != pickablePokemon)
                                allPokes.Remove(pickedMn);

                            if (allPokes.Count != 0)
                                continue;

                            allPokes.AddRange(enumerable);

                            if (pickablePokemon == allPokes)
                                continue;

                            pickablePokemon.AddRange(allPokes);
                            pickablePokemon.RemoveAll(area.BannedPokemon.Contains);
                        }
                    }

                    //  Apply the map
                    foreach (var enc in area.Encounters)
                        enc.Pokemon1 = areaMap[enc.Pokemon1];
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
                            cachedPokeLists[areaTheme] = pType;
                        }

                        possiblePokemon = new List<Pokemon>(cachedPokeLists[areaTheme]);
                        if (area.BannedPokemon.Count > 0)
                            possiblePokemon.RemoveAll(area.BannedPokemon.Contains);

                        if (possiblePokemon.Count < inArea.Count)
                            possiblePokemon = null;

                        iterLoops++;
                    }

                    if (possiblePokemon == null)
                        throw new NotImplementedException(
                            "Could not randomize an area in a reasonable amount of attempts.");

                    //  Build area map using type theme.
                    var areaMap = new Dictionary<Pokemon, Pokemon>();
                    foreach (var areaPk in inArea)
                    {
                        var picked = Random.Next(possiblePokemon.Count);
                        var pickedMn = possiblePokemon[picked];
                        areaMap[areaPk] = pickedMn;
                        possiblePokemon.RemoveAt(picked);
                    }

                    foreach (var enc in area.Encounters)
                        //  Apply the map
                        enc.Pokemon1 = areaMap[enc.Pokemon1];
                }
            }
            else if (usePowerLevels)
            {
                var allowedPokes = enumerable.ToList();
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

                    //  Apply the map
                    foreach (var enc in area.Encounters)
                        enc.Pokemon1 = areaMap[enc.Pokemon1];
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
                        var picked = noLegendaries ? RandomPokemon(p => !p.Legendary) : RandomPokemon();
                        while (areaMap.ContainsValue(picked) || area.BannedPokemon.Contains(picked))
                            picked = noLegendaries ? RandomPokemon(p => !p.Legendary) : RandomPokemon();

                        areaMap[areaPk] = picked;
                    }

                    foreach (var enc in area.Encounters)
                        //  Apply the map
                        enc.Pokemon1 = areaMap[enc.Pokemon1];
                }
            }
        }

        public void RandomizeMoveTypes()
        {
            foreach (var mv in ValidMoves)
            {
                if (mv.Id != Move.StruggleId && mv.Type != null)
                    mv.Type = RandomType();
            }
        }
        
        private Pokemon PickWildPowerLvlReplacement(
            IList<Pokemon> pokemonPool,
            Pokemon current,
            bool banSamePokemon,
            IList<Pokemon> usedUp)
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
                {
                    if (pk.BstForPowerLevels() >= minTarget &&
                        pk.BstForPowerLevels() <= maxTarget &&
                        (!banSamePokemon || !ReferenceEquals(pk, current)) &&
                        (usedUp == null || !usedUp.Contains(pk)) &&
                        !canPick.Contains(pk))
                        canPick.Add(pk);
                }

                minTarget = minTarget - currentBst / 20;
                maxTarget = maxTarget + currentBst / 20;
                expandRounds++;
            }

            return canPick[Random.Next(canPick.Count)];
        }

        public void RandomizeAbilities(bool evolutionSanity, bool allowWonderGuard, bool banTrappingAbilities, bool banNegativeAbilities)
        {

            var abilitiesPerPokemon = RomHandler.Game.AbilitiesPerPokemon;

            //  Abilities don't exist in some games...
            if (abilitiesPerPokemon == 0)
                return;

            var hasDwAbilities = abilitiesPerPokemon == 3;
            var bannedAbilities = new List<int>();
            if (!allowWonderGuard)
                bannedAbilities.Add(GlobalConstants.WonderGuardIndex);

            if (banTrappingAbilities)
                bannedAbilities.AddRange(GlobalConstants.BattleTrappingAbilities);

            if (banNegativeAbilities)
                bannedAbilities.AddRange(GlobalConstants.NegativeAbilities);

            var maxAbility = RomHandler.Abilities.Count - 1;
            if (evolutionSanity)
            {
                //  copy abilities straight up evolution lines
                //  still keep WG as an exception, though
                CopyUpEvolutionsHelper(
                    pokemon =>
                    {
                        if (pokemon.Ability1.Id == GlobalConstants.WonderGuardIndex ||
                            pokemon.Ability2.Id == GlobalConstants.WonderGuardIndex ||
                            pokemon.Ability3.Id == GlobalConstants.WonderGuardIndex)
                            return;

                        // Pick first ability
                        pokemon.Ability1 = RomHandler.Abilities[PickRandomAbility(maxAbility, bannedAbilities)];

                        // Second ability?
                        var ability2 = Random.NextDouble() < 0.5
                            ? PickRandomAbility(maxAbility, bannedAbilities, pokemon.Ability1.Id)
                            : 0;

                        pokemon.Ability2 = RomHandler.Abilities[ability2];

                        // Third ability?
                        if (!hasDwAbilities)
                            return;

                        var ability3 = PickRandomAbility(
                            maxAbility,
                            bannedAbilities,
                            pokemon.Ability1.Id,
                            pokemon.Ability2.Id);

                        pokemon.Ability3 = RomHandler.Abilities[ability3];
                    },
                    (evFrom, evTo, toMonIsFinalEvo) =>
                    {
                        if (evTo.Ability1.Id == GlobalConstants.WonderGuardIndex ||
                            evTo.Ability2.Id == GlobalConstants.WonderGuardIndex ||
                            evTo.Ability3.Id == GlobalConstants.WonderGuardIndex)
                            return;

                        evTo.Ability1 = evFrom.Ability1;
                        evTo.Ability2 = evFrom.Ability2;
                        evTo.Ability3 = evFrom.Ability3;
                    });
            }
            else
            {
                foreach (var pk in ValidPokemons)
                {
                    if (pk == null)
                        continue;

                    //  Don't remove WG if already in place.
                    if (pk.Ability1.Id == GlobalConstants.WonderGuardIndex ||
                        pk.Ability2.Id == GlobalConstants.WonderGuardIndex ||
                        pk.Ability3.Id == GlobalConstants.WonderGuardIndex)
                        continue;

                    //  Pick first ability
                    pk.Ability1 = RomHandler.Abilities[PickRandomAbility(maxAbility, bannedAbilities)];

                    var ability2 = Random.NextDouble() < 0.5
                        ? PickRandomAbility(maxAbility, bannedAbilities, pk.Ability1.Id)
                        : 0;

                    //  Second ability?
                    pk.Ability2 = RomHandler.Abilities[ability2];

                    //  Third ability?
                    if (!hasDwAbilities)
                        continue;

                    var ability3 = PickRandomAbility(maxAbility, bannedAbilities, pk.Ability1.Id, pk.Ability2.Id);
                    pk.Ability2 = RomHandler.Abilities[ability3];
                }
            }

            int PickRandomAbility(int max, ICollection<int> banned, params int[] alreadySet)
            {
                int newAbility;
                do
                {
                    newAbility = Random.Next(max) + 1;
                } while (banned.Contains(newAbility) && alreadySet.Any(t => t == newAbility));

                return newAbility;
            }
        }

        private static HashSet<Pokemon> PokemonInArea(EncounterSet area)
        {
            var inArea = new HashSet<Pokemon>();
            foreach (var enc in area.Encounters)
                inArea.Add(enc.Pokemon1);

            return inArea;
        }

        private List<Pokemon> PokemonOfType(Typing type, bool noLegendaries)
        {
            return ValidPokemons
                .Where(
                    pk => pk != null &&
                          (!noLegendaries || !pk.Legendary) &&
                          (pk.PrimaryType == type || pk.SecondaryType == type))
                .ToList();
        }

        public void RandomizeWildHeldItems(bool banBadItems)
        {
            var pokemon = ValidPokemons;

            foreach (var pk in pokemon)
            {
                if (pk.CommonHeldItem == null &&
                    pk.RareHeldItem == null &&
                    pk.DarkGrassHeldItem == null)
                    return;

                var canHaveDarkGrass = pk.DarkGrassHeldItem != null;

                //  No guaranteed item atm
                var decision = Random.NextDouble();
                if (decision < 0.5)
                {
                    //  No held item at all
                    pk.CommonHeldItem = RomHandler.Items[0];
                    pk.RareHeldItem = RomHandler.Items[0];
                }
                else if (decision < 0.65)
                {
                    //  Just a rare item
                    pk.CommonHeldItem = RomHandler.Items[0];
                    pk.RareHeldItem = RandomItem();
                }
                else if (decision < 0.8)
                {
                    //  Just a common item
                    pk.CommonHeldItem = RandomItem();
                    pk.RareHeldItem = RomHandler.Items[0];
                }
                else if (decision < 0.95)
                {
                    //  Both a common and rare item
                    pk.CommonHeldItem = RandomItem();
                    pk.RareHeldItem = RandomItem();

                    while (pk.RareHeldItem == pk.CommonHeldItem)
                        pk.RareHeldItem = RandomItem();
                }
                else
                {
                    //  Guaranteed item
                    canHaveDarkGrass = false;
                    var guaranteed = RandomItem();
                    pk.CommonHeldItem = guaranteed;
                    pk.RareHeldItem = guaranteed;
                }

                if (canHaveDarkGrass)
                {
                    var dgDecision = Random.NextDouble();
                    pk.DarkGrassHeldItem = dgDecision < 0.5 ? RandomItem() : RomHandler.Items[0];
                }
                else if (pk.DarkGrassHeldItem != null)
                {
                    pk.DarkGrassHeldItem = RomHandler.Items[0];
                }
            }
        }
    }
}