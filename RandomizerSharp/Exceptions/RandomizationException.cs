using System;

namespace RandomizerSharp.Exceptions
{
    public class RandomizationException : Exception
    {
        public RandomizationException(string text) : base(text)
        {
        }
    }
}