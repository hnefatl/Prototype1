using System;

using Shared;
using Data;

namespace NetCore.Messages.DataMessages
{
    // Sent by Server on Client connection
    public class InitialiseMessage
        : Message
    {
        // Frame of all data in the repository
        public DataSnapshot Snapshot { get; set; }

        public InitialiseMessage()
            :this(new DataSnapshot())
        {
        }
        public InitialiseMessage(DataSnapshot Frame)
        {
            Snapshot = Frame;
        }

        public override void Serialise(Writer Out)
        {
            base.Serialise(Out);

            Snapshot.Serialise(Out);
        }
        public override void Deserialise(Reader In)
        {
            Snapshot.Deserialise(In);
        }
    }
}
