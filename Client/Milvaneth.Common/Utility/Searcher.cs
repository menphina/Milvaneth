using System;
using System.Collections.Generic;

namespace Milvaneth.Common
{
    public class Searcher
    {
        // mask true = has value

        public static int Search(IList<byte> text, IList<byte> pattern)
        {
            var mask = new bool[pattern.Count];
            for (var i = 0; i < mask.Length; i++) mask[i] = true;

            return Search(text, pattern, mask);
        }

        public static int Search(IList<byte> text, IList<byte> pattern, IList<bool> mask)
        {
            var t = text.Count;
            var p = pattern.Count;

            if (p > t)
            {
                return -1;
            }

            var prefixTable = GetDFA(pattern);

            var matchLength = 0;
            for (var i = 0; i < t; i++)
            {
                var reset = false;
                while (matchLength > 0 && pattern[matchLength] != text[i])
                {
                    if (!mask[matchLength])
                    {
                        if (matchLength > 0 && reset)
                        {
                            var kmpValue = Search(new ArraySegment<byte>(text as byte[], i - matchLength, matchLength),
                                new ArraySegment<byte>(pattern as byte[], 0, matchLength),
                                new ArraySegment<bool>(mask as bool[], 0, matchLength));
                            if (kmpValue != 0)
                            {
                                matchLength = 0;
                            }
                            else if (!mask[matchLength - 1])
                            {
                                matchLength++;
                            }
                        }
                        break;
                    }

                    matchLength = prefixTable[matchLength - 1];
                    reset = true;
                }

                if (reset && pattern[matchLength] == text[i - 1])
                {
                    matchLength++;
                }

                if (pattern[matchLength] == text[i] || !mask[matchLength])
                {
                    matchLength++;
                }

                if (matchLength == p)
                {
                    return i - (p - 1);
                }
            }

            return -1;
        }

        private static int[] GetDFA(IList<byte> pattern)
        {
            var p = pattern.Count;
            var dfa = new int[p];
            var longestPrefixIndex = 0;

            for (var i = 2; i < p; i++)
            {
                while (longestPrefixIndex > 0 && pattern[longestPrefixIndex + 1] != pattern[i])
                {
                    longestPrefixIndex = dfa[longestPrefixIndex];
                }

                if (pattern[longestPrefixIndex + 1] == pattern[i])
                {
                    longestPrefixIndex++;
                }
                dfa[i] = longestPrefixIndex;
            }

            return dfa;
        }
    }
}