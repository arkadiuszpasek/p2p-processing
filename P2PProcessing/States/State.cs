using P2PProcessing.Protocol;

namespace P2PProcessing.States
{
    public abstract class State
    {
        Session session;
        abstract public void OnMessage(Msg msg);
    }

    public class WorkingState : State
    {
        public override void OnMessage(Msg msg)
        {

        }

    }
}
