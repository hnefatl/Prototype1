using System;

using Shared;

namespace NetCore.Messages
{
    // Message used when a client connects to the server
    public class ConnectMessage
        : Message
    {
        // Logged on user's username
        public string Username { get; protected set; }
        // Computer name of the client
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

        public override void Serialise(Writer Writer)
        {
            base.Serialise(Writer);

            Writer.Write(Username);
            Writer.Write(ComputerName);
        }
        public override void Deserialise(Reader Reader)
        {
            Username = Reader.ReadString();
            ComputerName = Reader.ReadString();
        }
    }
}
