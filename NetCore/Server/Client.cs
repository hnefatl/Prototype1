using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;

using NetCore.Messages;

namespace NetCore.Server
{
    public delegate void MessageReceivedHandler(Client Sender, Message Message);
    public delegate void DisconnectHandler(Client Sender, DisconnectMessage Message);

    // Used by RBSListener to represent a virtual "client" (wrapped around a socket, effectively).
    public class Client
        : IDisposable
    {
        public string Username { get; protected set; }
        public string ComputerName { get; protected set; }
        public IPEndPoint Remote { get { return (IPEndPoint)Connection.Client.RemoteEndPoint; } }

        protected TcpClient Connection { get; set; }
        public bool Connected { get { return Connection.Connected; } }

        protected NetworkStream Stream { get; set; }
        protected NetReader In { get; set; }
        protected NetWriter Out { get; set; }

        public event MessageReceivedHandler MessageReceived;
        public event DisconnectHandler Disconnect;

        private byte[] Buffer { get; set; }

        private Client(ConnectMessage m, TcpClient Connection)
        {
            Username = m.Username;
            ComputerName = m.ComputerName;

            this.Connection = Connection;

            Stream = Connection.GetStream();
            In = new NetReader(Stream);
            Out = new NetWriter(Stream);

            MessageReceived = delegate { };
            Disconnect = delegate { };
        }
        public void Dispose()
        {
            Send(new DisconnectMessage(DisconnectType.Expected));

            Stream.Dispose();
            In.Dispose();
            Out.Dispose();

            Connection.Close();
        }

        public void Start()
        {
            if (Connected)
                StartRead();
        }
        public void Stop()
        {
            Dispose();
        }

        private void StartRead()
        {
            // Read the notification byte
            In.BeginReadByte(Stream_ReadComplete);
        }
        private void Stream_ReadComplete(IAsyncResult Result)
        {
            try
            {
                In.EndReadByte(Result);
                Result.AsyncWaitHandle.Dispose();

                Message New;
                New = Message.ReadMessage(In);

                if (New is DisconnectMessage)
                {
                    Disconnect(this, (DisconnectMessage)New);
                    return;
                }
                else
                    MessageReceived(this, New);

                StartRead(); // Go for another receive
                return;
            }
            catch (ObjectDisposedException)
            { } // Stream was closed
            catch (IOException)
            { } // Stream was closed

            Disconnect(this, new DisconnectMessage(DisconnectType.Unexpected));
        }

        public void Send(Message m)
        {
            try
            {
                lock (Connection)
                    if (Connected)
                        Out.Write(m);
            }
            catch
            {
                Disconnect(this, new DisconnectMessage(DisconnectType.Unexpected));
            }
        }

        public override string ToString()
        {
            if (!Connected)
                return "Disconnected client";
            else
                return Username + "@" + ComputerName + " on " + Remote.Address.ToString() + ":" + Remote.Port;
        }

        public static Client Create(TcpClient Connection)
        {
            NetReader Reader = new NetReader(Connection.GetStream()); // Don't Dispose this - it closes the underlying stream
            Reader.ReadByte(); // Read the notification byte
            return new Client(Message.ReadMessage<ConnectMessage>(Reader), Connection);
        }
    }
}
