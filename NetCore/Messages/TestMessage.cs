using System;

using Shared;

namespace NetCore.Messages
{
    // Test message, used in debugging
    public class TestMessage
        : Message
    {
        // Internal message
        public string Message { get; protected set; }

        public TestMessage()
            :this(null)
        {
        }
        public TestMessage(string Message)
        {
            this.Message = Message;
        }

        public override void Serialise(Writer Writer)
        {
            base.Serialise(Writer);

            Writer.Write(Message);
        }
        public override void Deserialise(Reader Reader)
        {
            Message = Reader.ReadString();
        }
    }
}
