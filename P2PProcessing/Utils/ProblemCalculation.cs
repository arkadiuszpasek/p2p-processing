using System;
using System.Collections.Generic;
using System.Text;
using P2PProcessing.Problems;

namespace P2PProcessing.Utils
{
    public static class ProblemCalculation
    {
        public static Problem CreateProblemFromHash(string hash)
        {
            var assignments = getInitialChunks();
            return new Problem(hash, assignments);
        }

        private static PayloadState[] getInitialChunks(int minLength = 10, int maxLength = 11)
        {
            List<PayloadState> assignments = new List<PayloadState>();
            for (int i = minLength; i <= maxLength; i++)
            {
                for (char c = 'a'; c <= 'z'; c++)
                {
                    var assignment = new Free(i, c.ToString());
                    assignments.Add(assignment);
                }
            }

            return assignments.ToArray();
        }
    }
}
