using System;
using RandomizerSharp.RomHandlers;

namespace RandomizerSharp.PokemonModel
{
    public static class GameExtensions
    {
        public static bool HasMoveTutors(this Game game)
        {
            switch (game)
            {
                case Game.Red:
                case Game.Blue:
                case Game.Green:
                case Game.Yellow:
                case Game.Black:
                case Game.White:
                case Game.Silver:
                case Game.Gold:
                case Game.Ruby:
                case Game.Sapphire:
                case Game.Diamond:
                case Game.Pearl:
                case Game.HeartGold:
                case Game.SoulSilver:
                case Game.X:
                case Game.Y:
                case Game.OmegaRuby:
                case Game.AlphaSapphire:
                case Game.Sun:
                case Game.Moon:
                    return false;

                case Game.Crystal:
                case Game.FireRed:
                case Game.LeafGreen:
                case Game.Emerald:
                case Game.Platinum:
                case Game.Black2:
                case Game.White2:
                    return true;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static bool SupportsFoarStartingMoves(this Game game)
        {
            switch (game)
            {
                // TODO: For some reason, the old randomizer claims that some gen2 games can't have 4 starting moves. Gotta check if this is true
                case Game.Silver:
                case Game.Gold:
                case Game.Crystal:
                    return false;

                case Game.Red:
                case Game.Blue:
                case Game.Green:
                case Game.Ruby:
                case Game.Sapphire:
                case Game.Emerald:
                case Game.FireRed:
                case Game.LeafGreen:
                case Game.Yellow:
                case Game.Diamond:
                case Game.Pearl:
                case Game.Platinum:
                case Game.HeartGold:
                case Game.SoulSilver:
                case Game.Black:
                case Game.White:
                case Game.Black2:
                case Game.White2:
                case Game.X:
                case Game.Y:
                case Game.OmegaRuby:
                case Game.AlphaSapphire:
                case Game.Sun:
                case Game.Moon:
                    return true;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static bool HasTimeBasedEncounters(this Game game)
        {
            switch (game)
            {
                case Game.X:
                case Game.Y:
                case Game.OmegaRuby:
                case Game.AlphaSapphire:
                case Game.Sun:
                case Game.Moon:
                case Game.Red:
                case Game.Blue:
                case Game.Green:
                case Game.Ruby:
                case Game.Sapphire:
                case Game.Emerald:
                case Game.FireRed:
                case Game.LeafGreen:
                case Game.Yellow:
                    
                // dppt technically do but we ignore them completely
                case Game.Diamond:
                case Game.Pearl:
                case Game.Platinum:
                    return false;

                case Game.Silver:
                case Game.Gold:
                case Game.Crystal:
                case Game.HeartGold:
                case Game.SoulSilver:
                case Game.Black:
                case Game.White:
                case Game.Black2:
                case Game.White2:
                    return true;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        public static int AbilitiesPerPokemon(this Game game)
        {
            switch (game)
            {
                case Game.Red:
                case Game.Blue:
                case Game.Green:
                case Game.Yellow:
                case Game.Silver:
                case Game.Gold:
                case Game.Crystal:
                    return 0;

                case Game.Ruby:
                case Game.Sapphire:
                case Game.Emerald:
                case Game.FireRed:
                case Game.LeafGreen:
                case Game.Diamond:
                case Game.Pearl:
                case Game.Platinum:
                case Game.HeartGold:
                case Game.SoulSilver:
                    return 2;

                case Game.Black:
                case Game.White:
                case Game.Black2:
                case Game.White2:
                case Game.X:
                case Game.Y:
                case Game.OmegaRuby:
                case Game.AlphaSapphire:
                case Game.Sun:
                case Game.Moon:
                    return 3;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static bool CanChangeStaticPokemon(this Game game)
        {
            switch (game)
            {
                // TODO: For some reason, the old randomizer claims that some gen2 games can't change static pokemon. Gotta check if this is true
                case Game.Silver:
                case Game.Gold:
                case Game.Crystal:
                    return false;

                case Game.Red:
                case Game.Blue:
                case Game.Green:
                case Game.Yellow:
                case Game.Ruby:
                case Game.Sapphire:
                case Game.Emerald:
                case Game.FireRed:
                case Game.LeafGreen:
                case Game.Diamond:
                case Game.Pearl:
                case Game.Platinum:
                case Game.HeartGold:
                case Game.SoulSilver:
                case Game.Black:
                case Game.White:
                case Game.Black2:
                case Game.White2:
                case Game.X:
                case Game.Y:
                case Game.OmegaRuby:
                case Game.AlphaSapphire:
                case Game.Sun:
                case Game.Moon:
                    return true;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static bool HasPhysicalSpecialSplit(this Game game)
        {
            switch (game)
            {
                case Game.Red:
                case Game.Blue:
                case Game.Green:
                case Game.Yellow:
                case Game.Silver:
                case Game.Gold:
                case Game.Crystal:
                    return false;

                case Game.Ruby:
                case Game.Sapphire:
                case Game.Emerald:
                case Game.FireRed:
                case Game.LeafGreen:
                case Game.Diamond:
                case Game.Pearl:
                case Game.Platinum:
                case Game.HeartGold:
                case Game.SoulSilver:
                case Game.Black:
                case Game.White:
                case Game.Black2:
                case Game.White2:
                case Game.X:
                case Game.Y:
                case Game.OmegaRuby:
                case Game.AlphaSapphire:
                case Game.Sun:
                case Game.Moon:
                    return true;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static int Generation(this Game game)
        {
            switch (game)
            {
                case Game.Red:
                case Game.Blue:
                case Game.Green:
                case Game.Yellow:
                    return 1;

                case Game.Silver:
                case Game.Gold:
                case Game.Crystal:
                    return 2;

                case Game.Ruby:
                case Game.Sapphire:
                case Game.Emerald:
                case Game.FireRed:
                case Game.LeafGreen:
                    return 3;

                case Game.Diamond:
                case Game.Pearl:
                case Game.Platinum:
                case Game.HeartGold:
                case Game.SoulSilver:
                    return 4;

                case Game.Black:
                case Game.White:
                case Game.Black2:
                case Game.White2:
                    return 5;

                case Game.X:
                case Game.Y:
                case Game.OmegaRuby:
                case Game.AlphaSapphire:
                    return 6;

                case Game.Sun:
                case Game.Moon:
                    return 7;

                default:
                    throw new ArgumentOutOfRangeException(nameof(game), game, null);
            }
        }
    }

    public enum Game
    {
        // Gen 1
        Red,
        Blue,
        Green,
        Yellow,

        // Gen 2
        Silver,
        Gold,
        Crystal,

        // Gen 3
        Ruby,
        Sapphire,
        Emerald,
        FireRed,
        LeafGreen,

        // Gen 4
        Diamond,
        Pearl,
        Platinum,
        HeartGold,
        SoulSilver,

        // Gen 5
        Black,
        White,
        Black2,
        White2,

        // Gen 6
        X,
        Y,
        OmegaRuby,
        AlphaSapphire,

        // Gen 7
        Sun,
        Moon
    }
}