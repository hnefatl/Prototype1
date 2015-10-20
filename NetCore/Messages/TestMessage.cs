using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Shared;

namespace NetCore.Messages
{
    public class TestMessage
        : Message
    {
        public string Message { get; protected set; }

        public TestMessage()
            :this(null)
        {
        }
        public TestMessage(string Message)
        {
            this.Message = Message;
        }

        public override void Serialise(IWriter Writer)
        {
            base.Serialise(Writer);

            Writer.Write(Message);
        }
        public override void Deserialise(IReader Reader)
        {
            Message = Reader.ReadString();
        }

        protected override int GetMessageSize()
        {
            return base.GetMessageSize() + NetReader.NetworkLength(Message);
        }
    }
}
