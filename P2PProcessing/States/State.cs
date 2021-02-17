using P2PProcessing.Protocol;
using P2PProcessing.Problems;
using P2PProcessing.Utils;
using System.Threading;
using System;
using P2PProcessing.ErrorHandling;

namespace P2PProcessing.States
{
    public abstract class State
    {
        protected Session session;
        protected Thread thread;
        protected bool running;
        public State(Session session)
        {
            this.session = session;
        }

        abstract public void OnMessage(Msg msg);
        abstract public void CalculateNext();

        public void EndCalculating()
        {
            this.running = false;
        }
    }

    public class NotWorkingState : State
    {
        public NotWorkingState(Session session) : base(session) 
        {
            this.running = false;
        }
        public override void OnMessage(Msg msg)
        {
            var updated = msg as ProblemUpdatedMsg;
            if (updated != null)
            {
                if (this.session.currentProblem == null || (this.session.currentProblem.Hash != updated.Problem.Hash))
                {
                    this.session.currentProblem = updated.Problem;
                }

                this.session.DetermineAction();
            }

        }

        public override void CalculateNext()
        {
            P2P.logger.Warn("Not working state cannot calculate problem");
        }

        public override string ToString()
        {
            return "Not Working";
        }

    }

    public class WorkingState : State
    {
        int calculatedPayloadIndex;
        public WorkingState(Session session) : base(session) 
        {
            this.running = true;
        }
        public override void OnMessage(Msg msg)
        {
            if (msg is ProblemUpdatedMsg)
            {
                this.checkCollision(ref this.session.currentProblem, (msg as ProblemUpdatedMsg).Problem);
                P2P.logger.Info($"Received problem updated with {(msg as ProblemUpdatedMsg).Problem.GetProgress()}% calculated payloads");
            }
            else if (msg is ProblemSolvedMsg)
            {
                var solved = msg as ProblemSolvedMsg;

                lock (this.session.currentProblem)
                {
                    this.session.currentProblem = solved.Problem;
                }
                P2P.logger.Info($"Someone found sollution: {solved.Problem.Solution} for hash {solved.Problem.Hash}\nChecked {session.currentProblem.GetProgress()}% payloads");
                this.EndCalculating();
                this.session.ChangeState(new NotWorkingState(this.session));
            }
        }

        private void checkCollision(ref Problem ownProblem, Problem receivedProblem)
        {
            bool hasAbandonedPayload = false;

            lock (this.session.currentProblem)
            {
                for (int i = 0; i<ownProblem.Assignment.Length; i++)
                {
                    if (ownProblem.Assignment[i] is Free && (receivedProblem.Assignment[i] is Taken || receivedProblem.Assignment[i] is Calculated))
                    {
                        ownProblem.Assignment[i] = receivedProblem.Assignment[i];
                    }
                    else if (ownProblem.Assignment[i] is Taken && receivedProblem.Assignment[i] is Taken && this.calculatedPayloadIndex == i)
                    {
                        var ownStatus = ownProblem.Assignment[i] as Taken;
                        var receivedStatus = receivedProblem.Assignment[i] as Taken;

                        if (ownStatus.Timestamp > receivedStatus.Timestamp)
                        {
                            P2P.logger.Info($"Payload {i} has collided, stopping calculation because someone started first");
                            ownProblem.Assignment[i] = Free.Of(receivedStatus.Length, receivedStatus.StartString);

                            hasAbandonedPayload = true;

                            this.EndCalculating();
                        }
                    }
                    else if (ownProblem.Assignment[i] is Taken && this.calculatedPayloadIndex != i && receivedProblem.Assignment[i] is Free)
                    {
                        ownProblem.Assignment[i] = receivedProblem.Assignment[i];
                    }
                    else if (!(ownProblem.Assignment[i] is Calculated) && receivedProblem.Assignment[i] is Calculated)
                    {
                        ownProblem.Assignment[i] = receivedProblem.Assignment[i];
                    }
                }

            }

            if (hasAbandonedPayload)
            {
                this.session.ChangeState(new NotWorkingState(this.session));
                this.session.DetermineAction();
            }
        }

        public override void CalculateNext()
        {
            lock (this.session.currentProblem)
            {
                if (this.session.currentProblem.Solution != null)
                {
                    this.session.ChangeState(new NotWorkingState(this.session));  //żeby nie było zapętlania 
                    return;
                }
            }

            int length;
            string startString;
            PayloadState payloadState;
            try
            {
                Thread.Sleep(new Random().Next(50));
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
                this.calculatedPayloadIndex = payloadIndex;
                this.thread = new Thread(() => calculate(payloadIndex, length, startString));

                this.thread.Start();
            }
            catch (Exception e)
            {
                P2P.logger.Warn($"Not calculating next payload: {e.Message}");
            }
        }

        private void calculate(int index, int length, string startString)
        {
            try
            {
                P2P.logger.Debug($"Starting calculatation of payload {index}");
                var result = ProblemCalculation.CheckPayload(this.session.currentProblem.Hash, length, startString, ref this.running);
                P2P.logger.Debug($"Calculated payload {index} with result {result}");

                this.session.HandlePayloadCalculated(index, result);
            }
            catch (ThreadException e) { }
            catch (Exception e)
            {
                P2P.logger.Warn(e.Message);
            }
        }

        public override string ToString()
        {
            return "Working";
        }
    }
}
