using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rensoft
{
    public class RsRandom : Random
    {
        public static int seed;

        public RsRandom() : base(getSeed()) { }

        private static int getSeed()
        {
            int newSeed = (int)DateTime.Now.Ticks;
            if (newSeed == seed)
            {
                seed++;
            }
            else
            {
                seed = newSeed;
            }
            return seed;
        }

        public static string GenerateString(int length)
        {
            return GenerateString(length, new char[0]);
        }

        public static string GenerateString(int length, char[] excludeChars)
        {
            RsRandom random = new RsRandom();

            string result = string.Empty;
            while (result.Length < length)
            {
                char c = (char)random.Next(33, 126);
                if (!excludeChars.Contains(c))
                {
                    result += c;
                }
            }
            return result;
        }
    }
}
