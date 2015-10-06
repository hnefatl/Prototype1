using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Shared;

namespace NetCore.Messages
{
    public class ConnectMessage
        : Message
    {
        public string Username { get; protected set; }
        public string ComputerName { get; protected set; }

        public ConnectMessage()
            :this(null, null)
        {
        }
        public ConnectMessage(string Username, string ComputerName)
        {
            this.Username = Username;
            this.ComputerName = ComputerName;
        }

        public override void Serialise(IWriter Writer)
        {
            base.Serialise(Writer);

            Writer.Write(Username);
            Writer.Write(ComputerName);
        }
        public override void Deserialise(IReader Reader)
        {
            Username = Reader.ReadString();
            ComputerName = Reader.ReadString();
        }

        protected override int GetMessageSize()
        {
            return base.GetMessageSize() + NetReader.NetworkLength(Username) + NetReader.NetworkLength(ComputerName);
        }
    }
}
