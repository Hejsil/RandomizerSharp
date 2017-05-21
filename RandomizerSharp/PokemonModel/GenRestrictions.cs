namespace RandomizerSharp.PokemonModel
{
    public class GenRestrictions
    {
        public bool AllowGen1 { get; }
        public bool AllowGen2 { get; set; }
        public bool AllowGen3 { get; set; }
        public bool AllowGen4 { get; set; }
        public bool AllowGen5 { get; set; }

        public bool AssocG1G2 { get; set; }
        public bool AssocG1G4 { get; set; }
        public bool AssocG2G1 { get; set; }
        public bool AssocG2G3 { get; set; }
        public bool AssocG2G4 { get; set; }
        public bool AssocG3G2 { get; set; }
        public bool AssocG3G4 { get; set; }
        public bool AssocG4G1 { get; set; }
        public bool AssocG4G2 { get; set; }
        public bool AssocG4G3 { get; set; }

        public GenRestrictions()
        {
        }

        public GenRestrictions(int state)
        {
            AllowGen1 = (state & 1) > 0;
            AllowGen2 = (state & 2) > 0;
            AllowGen3 = (state & 4) > 0;
            AllowGen4 = (state & 8) > 0;
            AllowGen5 = (state & 16) > 0;

            AssocG1G2 = (state & 32) > 0;
            AssocG1G4 = (state & 64) > 0;

            AssocG2G1 = (state & 128) > 0;
            AssocG2G3 = (state & 256) > 0;
            AssocG2G4 = (state & 512) > 0;

            AssocG3G2 = (state & 1024) > 0;
            AssocG3G4 = (state & 2048) > 0;

            AssocG4G1 = (state & 4096) > 0;
            AssocG4G2 = (state & 8192) > 0;
            AssocG4G3 = (state & 16384) > 0;
        }

        public virtual bool NothingSelected() => !AllowGen1 && !AllowGen2 && !AllowGen3 && !AllowGen4 && !AllowGen5;

        public virtual int ToInt() => MakeIntSelected(
            AllowGen1,
            AllowGen2,
            AllowGen3,
            AllowGen4,
            AllowGen5,
            AssocG1G2,
            AssocG1G4,
            AssocG2G1,
            AssocG2G3,
            AssocG2G4,
            AssocG3G2,
            AssocG3G4,
            AssocG4G1,
            AssocG4G2,
            AssocG4G3);

        public virtual void LimitToGen(int generation)
        {
            if (generation < 2)
            {
                AllowGen2 = false;
                AssocG1G2 = false;
                AssocG2G1 = false;
            }
            if (generation < 3)
            {
                AllowGen3 = false;
                AssocG2G3 = false;
                AssocG3G2 = false;
            }
            if (generation < 4)
            {
                AllowGen4 = false;
                AssocG1G4 = false;
                AssocG2G4 = false;
                AssocG3G4 = false;
                AssocG4G1 = false;
                AssocG4G2 = false;
                AssocG4G3 = false;
            }
            if (generation < 5)
                AllowGen5 = false;
        }

        private int MakeIntSelected(params bool[] switches)
        {
            if (switches.Length > 32)
                return 0;
            var initial = 0;
            var state = 1;
            foreach (var b in switches)
            {
                initial |= b ? state : 0;
                state *= 2;
            }
            return initial;
        }
    }
}