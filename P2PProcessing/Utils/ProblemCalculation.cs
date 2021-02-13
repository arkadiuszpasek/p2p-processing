using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using P2PProcessing.Problems;

namespace P2PProcessing.Utils
{
    public static class ProblemCalculation
    {
        private static char[] Letters = "abcdefghijklmnoprstuwxyz".ToCharArray();
        public static Problem CreateProblemFromHash(string hash)
        {
            var assignments = getInitialChunks();
            return Problem.FromAssignment(hash, assignments);
        }

        private static PayloadState[] getInitialChunks(int minLength = 2, int maxLength = 5)
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
        public static IEnumerable<string> CombinationsWithRepetition(int length)
        {
            if (length <= 0)
                yield return "";
            else
            {
                foreach (var i in Letters)
                    foreach (var c in CombinationsWithRepetition(length - 1))
                        yield return i + c;
            }
        }

        public static string CheckPayload(string problemHash, int length, string startString)
        {
            var combinations = CombinationsWithRepetition(length - 1);
            foreach (var combination in combinations)
            {
                var text = startString + combination;
                var hash = Hasher.getHashHexRepresentation(text);

                P2P.logger.Debug($"Trying combination {text}");

                if (problemHash == hash)
                {
                    return text;
                }
            }

            return null;
        }
    }
}
