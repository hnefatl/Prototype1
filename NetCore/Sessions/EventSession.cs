using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NetCore.Messages;

namespace NetCore.Sessions
{
    public delegate void MessageReceivedHandler(EventSession Session, Message Msg);
    public class EventSession
        : Session
    {
        public event MessageReceivedHandler MessageReceived;

        public EventSession(Guid Id, ISessionCreator Parent)
            : base(Id, Parent)
        {
            MessageReceived = delegate { };
        }

        public void Send(Message Msg)
        {
            OnSendMessage(Msg);
        }
        public override void RegisterMessage(Message Msg)
        {
            MessageReceived(this, Msg);
        }
    }
}
