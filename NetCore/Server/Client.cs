using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;

using NetCore.Messages;
using NetCore.Sessions;

namespace NetCore.Server
{
    public delegate void DisconnectHandler(Client Sender, DisconnectMessage Message);

    public class Client
        : IDisposable, ISessionCreator
    {
        public string Username { get; protected set; }
        public string ComputerName { get; protected set; }
        public IPEndPoint Remote { get { return (IPEndPoint)Connection.Client.RemoteEndPoint; } }

        protected TcpClient Connection { get; set; }
        public bool Connected { get { return Connection.Connected; } }

        protected NetworkStream Stream { get; set; }
        protected NetReader In { get; set; }
        protected NetWriter Out { get; set; }

        public event DisconnectHandler Disconnect;

        public Dictionary<Guid, Session> Sessions { get; protected set; }

        private Client(ConnectMessage m, TcpClient Connection)
        {
            Username = m.Username;
            ComputerName = m.ComputerName;

            this.Connection = Connection;

            Stream = Connection.GetStream();
            In = new NetReader(Stream);
            Out = new NetWriter(Stream);

            Sessions = new Dictionary<Guid, Session>();

            Disconnect = delegate { };
        }
        public void Dispose()
        {
            Sessions.Clear();
            try
            {
                Out.Write(false); // Sending a non-session message
                Out.Write(new DisconnectMessage(DisconnectType.Expected)); // Send the d/c message
            }
            catch { }

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

        public Session CreateSession<T>() where T : Session
        {
            T Session = (T)Activator.CreateInstance(typeof(T), Guid.NewGuid(), this);
            Session.SendMessage += Session_SendMessage;

            lock (Session)
                Sessions.Add(Session.Id, Session);

            return Session;
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

                Guid Id = new Guid(In.ReadBytes(16));

                Message New;
                New = Message.ReadMessage(In);

                if (New is DisconnectMessage)
                {
                    Disconnect(this, (DisconnectMessage)New);
                    return;
                }
                else
                {
                    lock (Sessions)
                    {
                        if (Sessions.ContainsKey(Id))
                            Sessions[Id].RegisterMessage(New);
                    }
                }

                StartRead(); // Go for another receive
                return;
            }
            catch (ObjectDisposedException)
            { } // Stream was closed
            catch (IOException)
            { } // Stream was closed

            Disconnect(this, new DisconnectMessage(DisconnectType.Unexpected));
        }

        protected void Session_SendMessage(Session Sender, Message Msg)
        {
            try
            {
                lock (Connection)
                {
                    if (Connected)
                    {
                        Out.Write(true); // Indicates it's a session message
                        Out.Write(Sender.Id.ToByteArray()); // The Session Id
                        Out.Write(Msg); // The Session's message
                    }
                }
            }
            catch
            {
                Disconnect(this, new DisconnectMessage(DisconnectType.Unexpected));
            }
        }

        public override string ToString()
        {
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
