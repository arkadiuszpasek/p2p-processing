using System.Linq;

namespace P2PProcessing.Problems
{
    class Problem
    {
        string hash;
        PayloadState[] assignement;

        public int GetProgress()
        {
            return assignement.Aggregate(0, (acc, payload) => payload is Calculated ? acc + 1 : acc);
        }
    }

}
