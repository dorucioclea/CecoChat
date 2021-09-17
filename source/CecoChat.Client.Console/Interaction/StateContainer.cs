using System.Threading.Tasks;
using CecoChat.Client.Shared;

namespace CecoChat.Client.Console.Interaction
{
    public sealed class StateContainer
    {
        public StateContainer(MessagingClient client, MessageStorage storage)
        {
            Client = client;
            Storage = storage;
            Context = new StateContext();

            Users = new UsersState(this);
            Dialog = new DialogState(this);
            SendMessage = new SendMessageState(this);
            Final = new FinalState(this);
        }

        public MessagingClient Client { get; }
        public MessageStorage Storage { get; }
        public StateContext Context { get; }

        public State Users { get; }
        public State Dialog { get; }
        public State SendMessage { get; }
        public State Final { get; }
    }

    public sealed class StateContext
    {
        public bool ReloadData { get; set; }
        public long UserID { get; set; }
    }

    public sealed class FinalState : State
    {
        public FinalState(StateContainer states) : base(states)
        {}

        public override Task<State> Execute()
        {
            return null;
        }
    }
}