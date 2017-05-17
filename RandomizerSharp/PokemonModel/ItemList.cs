using System;

namespace RandomizerSharp.PokemonModel
{
    public class ItemList
    {
        private readonly bool[] _isBanned;
        private readonly bool[] _isTm;

        public ItemList(int highestIndex)
        {
            _isBanned = new bool[highestIndex + 1];
            _isTm = new bool[highestIndex + 1];
            for (var i = 1; i <= highestIndex; i++)
                _isBanned[i] = true;
        }

        public bool IsTm(int index)
        {
            if (index < 0 || index >= _isTm.Length)
                return false;
            return _isTm[index];
        }

        public bool IsAllowed(int index)
        {
            if (index < 0 || index >= _isTm.Length)
                return false;
            return _isBanned[index];
        }

        public void BanSingles(params int[] indexes)
        {
            foreach (var index in indexes)
                _isBanned[index] = false;
        }

        public void BanRange(int startIndex, int length)
        {
            for (var i = 0; i < length; i++)
                _isBanned[i + startIndex] = false;
        }

        public void TmRange(int startIndex, int length)
        {
            for (var i = 0; i < length; i++)
                _isTm[i + startIndex] = true;
        }

        public int RandomItem(Random random)
        {
            var chosen = 0;
            while (!_isBanned[chosen])
                chosen = random.Next(_isBanned.Length);

            return chosen;
        }

        public int RandomNonTm(Random random)
        {
            var chosen = 0;
            while (!_isBanned[chosen] || _isTm[chosen])
                chosen = random.Next(_isBanned.Length);
            return chosen;
        }

        public int RandomTm(Random random)
        {
            var chosen = 0;
            while (!_isTm[chosen])
                chosen = random.Next(_isBanned.Length);
            return chosen;
        }

        public ItemList Copy()
        {
            var other = new ItemList(_isBanned.Length - 1);
            Array.Copy(_isBanned, 0, other._isBanned, 0, _isBanned.Length);
            Array.Copy(_isTm, 0, other._isTm, 0, _isTm.Length);
            return other;
        }
    }
}