using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using NetCore.Messages;

namespace NetCore.Sessions
{
    // Doesn't need any fancy thread-safe operations as we're using a BlockingQueue => BlockingCollection<ConcurrentQueue>
    public class SyncSession
        : Session
    {
        protected Mutex MessagesQueued { get; set; }
        protected BlockingQueue<Message> Messages { get; set; }

        public SyncSession(Guid Id, ISessionCreator Parent)
            : base(Id, Parent)
        {
            MessagesQueued = new Mutex(false);
            Messages = new BlockingQueue<Message>();
        }

        public void Send(Message Msg)
        {
            OnSendMessage(Msg);
        }
        public Message Receive()
        {
            return Messages.Take();
        }

        public override void RegisterMessage(Message Msg)
        {
            Messages.Add(Msg);
        }
    }
}
