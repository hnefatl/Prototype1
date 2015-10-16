using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NetCore.Messages;

namespace NetCore.Sessions
{
    public delegate void SendMesssageHandler(Session Session, Message Msg);
    public abstract class Session
        : IDisposable
    {
        public Guid Id { get; set; }

        protected ISessionCreator Parent { get; set; }

        public Session(Guid Id, ISessionCreator Parent)
        {
            this.Id = Id;
            this.Parent = Parent;

            SendMessage = delegate { };
        }
        public void Dispose()
        {
            lock(Parent.Sessions)
            {
                Parent.Sessions.Remove(Id);
            }
        }

        public abstract void RegisterMessage(Message Msg);
        public event SendMesssageHandler SendMessage;
        protected void OnSendMessage(Message Msg)
        {
            if (SendMessage != null)
                SendMessage(this, Msg);
        }
    }
}
