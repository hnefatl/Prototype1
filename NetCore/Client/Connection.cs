using System;
using System.Net;
using System.Net.Sockets;

using NetCore.Messages;
using Shared;

namespace NetCore.Client
{
    // Delegate function signatures for the events defined below
    public delegate void DisconnectHandler(Connection Sender, DisconnectMessage Message);
    public delegate void MessageReceivedHandler(Connection Sender, Message Message);

    // Used by the actual network client
    public class Connection
        : IDisposable
    {
        // Inner socket to wrap around, and use for network IO
        public Socket Inner { get; protected set; }

        // Flag representing connection status. Really just a proxy for the internal socket's flag
        public bool Connected { get { return Inner.Connected; } }

        // The stream being read from
        protected NetworkStream Stream { get; set; }
        protected NetReader In { get; set; }
        protected NetWriter Out { get; set; }

        // Events to signal disconection and messages being received
        public event DisconnectHandler Disconnect;
        public event MessageReceivedHandler MessageReceived;

        // Internal buffer of data received
        private byte[] Buffer { get; set; }

        public Connection()
        {
            Disconnect = delegate { };
            MessageReceived = delegate { };
        }
        public void Dispose()
        {
            try
            {
                // On Dispose, send an unexpected disconnect signal.
                // If we've already disconnected cleanly, this will just fail
                Send(new DisconnectMessage(DisconnectType.Unexpected));
            }
            catch { }

            Stream.Dispose();
            Out.Dispose();
        }

        // Alternative overloaded signature for next function
        public bool Connect(string ServerAddress, ushort Port, ConnectMessage ConnectionMessage)
        {
            IPAddress Target;
            if (!IPAddress.TryParse(ServerAddress, out Target))
                return false;

            return Connect(new IPEndPoint(Target, Port), ConnectionMessage);
        }
        // Attempts to connect to a specified server with a given connection message
        public bool Connect(IPEndPoint Server, ConnectMessage ConnectionMessage)
        {
            // Initialise socket for TCP/IP communications
            Inner = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                // Try to connect
                Inner.Connect(Server);

                // Set up IO streams around the socket
                Stream = new NetworkStream(Inner);
                In = new NetReader(Stream);
                Out = new NetWriter(Stream);

                // Send off the initial connection message
                Send(ConnectionMessage);

                // Begin reading from the Server
                StartRead();

                return true;
            }
            catch
            {
                return false;
            }
        }
        public void Close(DisconnectType Reason)
        {
            Send(new DisconnectMessage(Reason));

            Dispose();
        }

        protected void StartRead()
        {
            // Read the notification byte
            In.BeginReadByte(Stream_ReadComplete);
        }
        protected void Stream_ReadComplete(IAsyncResult Result)
        {
            try
            {
                // Read and ignore the notification byte
                In.EndReadByte(Result);
                Result.AsyncWaitHandle.Dispose();

                // Read in the message preceded by the notification byte
                Message New = Message.ReadMessage(In);

                if (New is DisconnectMessage) // D/C if neccessary
                {
                    Disconnect(this, (DisconnectMessage)New);
                    return;
                }
                else // Otherwise fire the message received event
                    MessageReceived(this, New);

                StartRead(); // Go for another read
            }
            catch
            {
                Disconnect(this, new DisconnectMessage(DisconnectType.Unexpected));
            }
        }
        // Try to send a message to the Server
        public bool Send(Message Msg)
        {
            try
            {
                lock (Inner) // Thread safe
                    Msg.Serialise(Out);
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}
