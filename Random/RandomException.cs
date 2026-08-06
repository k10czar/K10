using System;

namespace Rogue.RNG
{
    public class RandomException : Exception
    {
        public RandomException(string message) : base(message) { }
    }
}