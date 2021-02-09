using P2PProcessing.Protocol;
using P2PProcessing.Problems;
using P2PProcessing.Utils;
using System.Threading;
using System;

namespace P2PProcessing.States
{
    public abstract class State
    {
        protected Session session;
        protected Thread thread;
        public State(Session session)
        {
            this.session = session;
        }

        abstract public void OnMessage(Msg msg);
        public void CalculateNext()
        {
            if (this.session.currentProblem.Solution != null)
            {
                this.session.ChangeState(new NotWorkingState(this.session));
                return;
            }

            int length;
            string startString;
            PayloadState payloadState;
            try
            {
                var payloadIndex = this.session.currentProblem.GetFirstFreeIndex(out payloadState);
                var free = payloadState as Free;
                if (free != null)
                {
                    length = free.Length;
                    startString = free.StartString;
                    this.session.currentProblem.SetPayloadState(payloadIndex, Taken.Of(length, startString));
                }
                else
                {
                    var taken = payloadState as Taken;
                    length = taken.Length;
                    startString = taken.StartString;
                    this.session.currentProblem.SetPayloadState(payloadIndex, Taken.Of(length, startString));
                }
                this.session.BroadcastToConnectedNodes(ProblemUpdatedMsg.FromProblem(this.session.currentProblem));
                this.thread = new Thread(() => calculate(payloadIndex, length, startString));
                
                this.thread.Start();
            } catch (Exception e)
            {
                P2P.logger.Warn($"Not calculating next payload: {e.Message}");
            }
        }

        public void EndCalculating()
        {
            if (this.thread != null && this.thread.IsAlive)
            {
                this.thread.Interrupt();
                this.thread = null;
            }
        }

        private void calculate(int index, int length, string startString)
        {
            try
            {
                P2P.logger.Debug($"Starting calculatation of payload {index}");
                var result = ProblemCalculation.CheckPayload(this.session.currentProblem.Hash, length, startString);
                P2P.logger.Debug($"Calculated payload {index} with result {result}");

                this.session.HandlePayloadCalculated(index, result);
            } catch (Exception e)
            {
                P2P.logger.Warn($"Error calculating: {e.Message}");
                this.session.ChangeState(new NotWorkingState(this.session));
            }
        }
    }

    public class NotWorkingState : State
    {
        public NotWorkingState(Session session) : base(session) { }
        public override void OnMessage(Msg msg)
        {
            var updated = msg as ProblemUpdatedMsg;
            if (updated != null && this.session.currentProblem?.Solution == null)
            {
                this.session.currentProblem = updated.Problem;
                this.CalculateNext();
                session.ChangeState(new WorkingState(session));
            }
        }

    }

    public class WorkingState : State
    {
        public WorkingState(Session session) : base(session) { }
        public override void OnMessage(Msg msg)
        {
            if (msg is ProblemUpdatedMsg)
            {
                this.checkCollision(ref this.session.currentProblem, (msg as ProblemUpdatedMsg).Problem);
                P2P.logger.Info($"Current progress: {this.session.currentProblem.GetProgress()}%");
            }
            else if (msg is ProblemSolvedMsg)
            {
                var solved = msg as ProblemSolvedMsg;

                this.session.currentProblem = solved.Problem;
                P2P.logger.Info($"Someone found sollution: {solved.Problem.Solution} for hash {solved.Problem.Hash}");
                this.EndCalculating();
                this.session.ChangeState(new NotWorkingState(this.session));
            }
        }

        private void checkCollision(ref Problem ownProblem, Problem receivedProblem)
        {
            bool hasAbandonedPayload = false;

            for (int i = 0; i<ownProblem.Assignment.Length; i++)
            {
                if (ownProblem.Assignment[i] is Free && (receivedProblem.Assignment[i] is Taken || receivedProblem.Assignment[i] is Calculated))
                {
                    ownProblem.Assignment[i] = receivedProblem.Assignment[i];
                }
                else if (ownProblem.Assignment[i] is Taken && receivedProblem.Assignment[i] is Taken)
                {
                    var ownStatus = ownProblem.Assignment[i] as Taken;
                    var receivedStatus = receivedProblem.Assignment[i] as Taken;

                    if (ownStatus.Timestamp > receivedStatus.Timestamp)
                    {
                        P2P.logger.Debug($"Payload {i} has collided, stopping calculation because someone started first");
                        ownProblem.Assignment[i] = receivedProblem.Assignment[i];
                        hasAbandonedPayload = true;

                        this.EndCalculating();
                    }
                }
                else if (!(ownProblem.Assignment[i] is Calculated) && receivedProblem.Assignment[i] is Calculated)
                {
                    ownProblem.Assignment[i] = receivedProblem.Assignment[i];
                }
            }

            if (hasAbandonedPayload)
            {
                this.CalculateNext();
            }
        }
    }
}
