using System;
using System.Net;
using System.Net.Sockets;
using System.IO;

using NetCore.Messages;
using Shared;

namespace NetCore.Server
{
    // Delegate functions for the events defined below
    public delegate void MessageReceivedHandler(Client Sender, Message Message);
    public delegate void DisconnectHandler(Client Sender, DisconnectMessage Message);

    // Used by Listener to represent a virtual "client" (wrapper around a socket, effectively).
    public class Client
        : IDisposable
    {
        // Local username of the user logged on with the client
        public string Username { get; protected set; }
        // Name of the machine the user's logged in on
        public string ComputerName { get; protected set; }
        // Remote network endpoint, hold IP and Port number
        public IPEndPoint Remote { get { return (IPEndPoint)Connection.RemoteEndPoint; } }

        // Internal Socket used for IO
        protected Socket Connection { get; set; }
        public bool Connected { get { return Connection.Connected; } }

        // Internal Stream used to read data from the network
        protected NetworkStream Stream { get; set; }
        // Readers/Writers from the stream (defined in Shared library)
        protected NetReader In { get; set; }
        protected NetWriter Out { get; set; }

        // Events for important actions
        public event MessageReceivedHandler MessageReceived;
        public event DisconnectHandler Disconnect;

        // Internal buffer object, used to receive messages into
        private byte[] Buffer { get; set; }

        // Private constructor - Clients can only be created by calling
        // the Create static method provided way below
        private Client(ConnectMessage m, Socket Connection)
        {
            Username = m.Username;
            ComputerName = m.ComputerName;

            this.Connection = Connection;

            Stream = new NetworkStream(Connection);
            In = new NetReader(Stream);
            Out = new NetWriter(Stream);

            MessageReceived = delegate { };
            Disconnect = delegate { };
        }
        public void Dispose()
        {
            // On disposal, send a disconnect message
            Send(new DisconnectMessage(DisconnectType.Expected));

            Stream.Dispose();
            In.Dispose();
            Out.Dispose();

            Connection.Close();
        }

        public void Start()
        {
            // Start reading from the stream
            if (Connected)
                StartRead();
        }
        public void Stop()
        {
            // Stopping is just disposing, effectively
            Dispose();
        }

        private void StartRead()
        {
            // Asynchronously read the notification byte
            In.BeginReadByte(Stream_ReadComplete);
        }
        // Called when an asynchronous read completes
        private void Stream_ReadComplete(IAsyncResult Result)
        {
            try
            {
                // Retrieve the notification byte
                In.EndReadByte(Result);
                Result.AsyncWaitHandle.Dispose();

                // Read a unknown type of message
                Message New = Message.ReadMessage(In);

                // If it's a disconnection, handle it internally
                if (New is DisconnectMessage)
                {
                    // Signal the disconnection
                    Disconnect(this, (DisconnectMessage)New);
                    return;
                }
                else // Not a disconnection, allow controlling code to handle it
                    MessageReceived(this, New);

                StartRead(); // Go for another message read
                return;
            }
            catch (ObjectDisposedException)
            { } // Stream was closed
            catch (IOException)
            { } // Stream was closed

            // If something went wrong, disconnect unexpectedly
            Disconnect(this, new DisconnectMessage(DisconnectType.Unexpected));
        }

        // Send a message to a single client
        public void Send(Message m)
        {
            try
            {
                lock (Connection) // Thread safe
                {
                    if (Connected)
                        m.Serialise(Out); // Serialise the message out
                }
            }
            catch
            {
                // If the send failed, treat it as a disconnect
                Disconnect(this, new DisconnectMessage(DisconnectType.Unexpected));
            }
        }

        // Provide nice description string
        public override string ToString()
        {
            if (!Connected)
                return "Disconnected client";
            else
                return Username + "@" + ComputerName + " on " + Remote.Address.ToString() + ":" + Remote.Port;
        }

        // Statically accept a client
        public static Client Create(Socket Connection)
        {
            // Create a new reader around the provided socket
            NetReader Reader = new NetReader(new NetworkStream(Connection));
            Reader.ReadByte(); // Read the single notification byte
            // Return a new client, initialising it using a received ConnectMessage
            return new Client(Message.ReadMessage<ConnectMessage>(Reader), Connection);
        }
    }
}
