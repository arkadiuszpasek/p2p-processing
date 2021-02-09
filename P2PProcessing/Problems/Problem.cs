using System.Linq;
using System.Xml.Serialization;
using P2PProcessing.ErrorHandling;

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

        public void SetPayloadState(int index, PayloadState state)
        {
            Assignment[index] = state;
        }

        public int GetFirstFreeIndex(out PayloadState state)
        {
            for (int i = 0; i < Assignment.Length; i++)
            {
                if (Assignment[i] is Free)
                {
                    state = Assignment[i];
                    return i;
                }
            }
            for (int i = 0; i < Assignment.Length; i++)
            {
                if (Assignment[i] is Taken)
                {
                    state = Assignment[i];
                    return i;
                }
            }

            throw new SessionException("All payloads are calculated");
        }

        public override string ToString()
        {
            return $"Hash: {Hash}, \n {string.Join<PayloadState>(",", Assignment)}";
        }
    }

}
