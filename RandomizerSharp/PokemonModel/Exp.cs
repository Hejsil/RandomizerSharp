namespace RandomizerSharp.PokemonModel
{
    public static class Exp
    {
        public static ExpCurve FromByte(byte curve)
        {
            switch (curve)
            {
                case 0:
                    return ExpCurve.MediumFast;
                case 1:
                    return ExpCurve.Erratic;
                case 2:
                    return ExpCurve.Fluctuating;
                case 3:
                    return ExpCurve.MediumSlow;
                case 4:
                    return ExpCurve.Fast;
                case 5:
                    return ExpCurve.Slow;
            }

            return ExpCurve.MediumFast;
        }

        public static byte ToByte(this ExpCurve expCurve)
        {
            switch (expCurve)
            {
                case ExpCurve.Slow:
                    return 5;
                case ExpCurve.MediumSlow:
                    return 3;
                case ExpCurve.MediumFast:
                    return 0;
                case ExpCurve.Fast:
                    return 4;
                case ExpCurve.Erratic:
                    return 1;
                case ExpCurve.Fluctuating:
                    return 2;
            }
            return 0; // default
        }
    }
}