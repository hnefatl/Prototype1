using System;

using Shared;

namespace NetCore.Messages
{
    // Records the type of disconnection
    public enum DisconnectType
        : byte
    {
        Expected, // eg. Logoff, application closed
        Unexpected, // eg. Process crashed
    }

    //Message sent to the Server when a Client closes
    public class DisconnectMessage
        : Message
    {
        // Type of disconnection
        public DisconnectType Reason { get; protected set; }

        public DisconnectMessage()
            :this(DisconnectType.Unexpected)
        {
        }
        public DisconnectMessage(DisconnectType Reason)
        {
            this.Reason = Reason;
        }

        public override void Serialise(Writer Writer)
        {
            base.Serialise(Writer);

            Writer.Write((byte)Reason);
        }

        public override void Deserialise(Reader Reader)
        {
            Reason = (DisconnectType)Reader.ReadByte();
        }
    }
}
