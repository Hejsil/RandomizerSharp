namespace RandomizerSharp.PokemonModel
{
    public static class Exp
    {
        public enum Curve : byte
        {
            Slow = 5,
            MediumSlow = 3,
            MediumFast = 0,
            Fast = 4,
            Erratic = 1,
            Fluctuating = 2
        }

        public static Curve FromByte(byte curve)
        {
            switch (curve)
            {
                case 0:
                    return Curve.MediumFast;
                case 1:
                    return Curve.Erratic;
                case 2:
                    return Curve.Fluctuating;
                case 3:
                    return Curve.MediumSlow;
                case 4:
                    return Curve.Fast;
                case 5:
                    return Curve.Slow;
            }

            return Curve.MediumFast;
        }

        public static byte ToByte(this Curve curve)
        {
            switch (curve)
            {
                case Curve.Slow:
                    return 5;
                case Curve.MediumSlow:
                    return 3;
                case Curve.MediumFast:
                    return 0;
                case Curve.Fast:
                    return 4;
                case Curve.Erratic:
                    return 1;
                case Curve.Fluctuating:
                    return 2;
            }
            return 0; // default
        }
    }
}