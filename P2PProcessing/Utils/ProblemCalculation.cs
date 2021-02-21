using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using P2PProcessing.ErrorHandling;
using P2PProcessing.Problems;

namespace P2PProcessing.Utils
{
    public static class ProblemCalculation
    {
        private static char[] Letters = "abcdefghijklmnoprstuqwxyz".ToCharArray();
        public static Problem CreateProblemFromHash(string hash, int minLength = 2, int maxLength = 5)
        {
            var assignments = getInitialChunks(minLength, maxLength);
            return Problem.FromAssignment(hash, Letters, assignments);
        }

        private static PayloadState[] getInitialChunks(int minLength, int maxLength)
        {
            List<PayloadState> assignments = new List<PayloadState>();
            for (int i = minLength; i <= maxLength; i++)
            {
                for (char c = 'a'; c <= 'z'; c++)
                {
                    var assignment = Free.Of(i, c.ToString());
                    assignments.Add(assignment);
                }
            }

            return assignments.ToArray();
        }
        public static IEnumerable<string> CombinationsWithRepetition(int length, char[] characters)
        {
            if (length <= 0)
            {
                yield return "";
            }
            else
            {
                foreach (var i in characters)
                {
                    foreach (var c in CombinationsWithRepetition(length - 1, characters))
                    {
                        yield return i + c;
                    }
                }
            }
        }

        public static string CheckPayload(string problemHash, int length, string startString, char[] characters, ref bool running)
        {
            var combinations = CombinationsWithRepetition(length - 1, characters);
            foreach (var combination in combinations)
            {
                if (!running) throw new ThreadException();

                var text = startString + combination;
                var hash = Hasher.getHashHexRepresentation(text);

                if (problemHash == hash)
                {
                    return text;
                }
            }

            return null;
        }
    }
}
