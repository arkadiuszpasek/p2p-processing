using System.Linq;
using System.Xml.Serialization;

namespace P2PProcessing.Problems
{
    public class Problem
    {
        public string Hash;
        public PayloadState[] Assignment;

        public static Problem FromAssignment(string hash, PayloadState[] assignment)
        {
            var problem = new Problem();
            problem.Hash = hash;
            problem.Assignment = assignment;

            return problem;
        }

        public int GetProgress()
        {
            return 100 * Assignment.Aggregate(0, (acc, payload) => payload is Calculated ? acc + 1 : acc) / Assignment.Length;
        }

        public override string ToString()
        {
            return $"Hash: {Hash}, \n {string.Join<PayloadState>(",", Assignment)}";
        }
    }

}
