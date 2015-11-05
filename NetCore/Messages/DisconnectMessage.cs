using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Shared;

namespace NetCore.Messages
{
    public enum DisconnectType
        : byte
    {
        Expected, // ie. logoff
        Unexpected, // ie. process ended
    }

    public class DisconnectMessage
        : Message
    {
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

        protected override int GetMessageSize()
        {
            return base.GetMessageSize() + NetReader.NetworkLength(Reason);
        }
    }
}
