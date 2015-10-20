using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Shared;
using Data;
using Data.Models;

namespace NetCore.Messages.DataMessages
{
    public class InitialiseMessage
        : Message
    {
        public DataSnapshot Snapshot { get; set; }

        public InitialiseMessage()
            :this(new DataSnapshot())
        {
        }
        public InitialiseMessage(DataSnapshot Frame)
        {
            Snapshot = Frame;
        }

        public override void Serialise(IWriter Out)
        {
            base.Serialise(Out);

            Snapshot.Serialise(Out);
        }
        public override void Deserialise(IReader In)
        {
            Snapshot.Deserialise(In);
        }
    }
}
