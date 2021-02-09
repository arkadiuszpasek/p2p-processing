using System.Linq;

namespace P2PProcessing.Problems
{
    public class Problem
    {
        public string Hash;
        public PayloadState[] Assignment;

        public Problem(string hash, PayloadState[] assignment)
        {
            this.Hash = hash;
            this.Assignment = assignment;
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
