using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

using NetCore.Messages;

namespace NetCore.Client
{
    public delegate void DisconnectHandler(Connection Sender, DisconnectMessage Message);
    public delegate void MessageReceivedHandler(Connection Sender, Message Message);

    // Used by the actual network client
    public class Connection
        : IDisposable
    {
        public Socket Inner { get; protected set; }

        public bool Connected { get { return Inner.Connected; } }

        protected NetworkStream Stream { get; set; }
        protected NetReader In { get; set; }
        protected NetWriter Out { get; set; }
        
        public event DisconnectHandler Disconnect;
        public event MessageReceivedHandler MessageReceived;

        private byte[] Buffer { get; set; }

        public Connection()
        {
            Inner = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        
            Disconnect = delegate { };
            MessageReceived = delegate { };
        }
        public void Dispose()
        {
            Send(new DisconnectMessage(DisconnectType.Unexpected));

            Stream.Dispose();
            Out.Dispose();
        }

        public bool Connect(string ServerAddress, ushort Port, ConnectMessage ConnectionMessage)
        {
            IPAddress Target;
            if (!IPAddress.TryParse(ServerAddress, out Target))
                return false;

            return Connect(new IPEndPoint(Target, Port), ConnectionMessage);
        }
        public bool Connect(IPEndPoint Server, ConnectMessage ConnectionMessage)
        {
            Inner = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                Inner.Connect(Server);

                Stream = new NetworkStream(Inner);
                In = new NetReader(Stream);
                Out = new NetWriter(Stream);

                Send(ConnectionMessage);

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

                StartRead(); // Go for another read
                return;
            }
            catch (ObjectDisposedException)
            { } // Socket closed
            catch (IOException)
            { } // Stream was closed

            Disconnect(this, new DisconnectMessage(DisconnectType.Unexpected));
        }

        public bool Send(Message Msg)
        {
            try
            {
                lock (Inner)
                   if (Connected)
                        Msg.Serialise(Out);
            }
            catch (ObjectDisposedException)
            { return false; }
            catch (IOException)
            { return false; }

            return true;
        }
    }
}
