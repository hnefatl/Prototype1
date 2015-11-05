using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using NetCore.Messages;

namespace NetCore.Server
{
    public delegate void ClientConnectHandler(Listener Sender, Client Client);
    public delegate void ClientDisconnectHandler(Listener Sender, Client Client, DisconnectMessage Message);
    public delegate void ClientMessageReceivedHandler(Listener Sender, Client Client, Message Message);

    public class Listener
        : IDisposable
    {
        protected Socket Inner { get; set; }

        public IPEndPoint Endpoint { get { return (IPEndPoint)Inner.LocalEndPoint; } }

        public IList<Client> Clients { get; protected set; }
        public BlockingQueue<ClientMessagePair> Messages { get; protected set; }

        public bool BufferMessages { get; protected set; }

        public event ClientConnectHandler ClientConnect;
        public event ClientDisconnectHandler ClientDisconnect;
        public event ClientMessageReceivedHandler ClientMessageReceived;

        protected Task AcceptingTask { get; set; }
        protected bool Run { get; set; }

        public Listener(int Port)
            : this(new IPEndPoint(IPAddress.Any, Port))
        {
        }
        public Listener(string IP, int Port)
            : this(new IPEndPoint(IPAddress.Parse(IP), Port))
        {
        }
        public Listener(IPEndPoint Endpoint)
            : this(Endpoint, new List<Client>())
        {
            // Be default, the Clients list is a List
        }
        public Listener(IPEndPoint Endpoint, IList<Client> ClientListType)
        {
            // Allow support for non-List collections of clients (ie. ObservableCollection)
            Clients = ClientListType;

            Inner = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Inner.Bind(Endpoint);
            Inner.Listen(50);

            Clients = new List<Client>();
            Messages = new BlockingQueue<ClientMessagePair>();

            ClientConnect = delegate { };
            ClientDisconnect = delegate { };
            ClientMessageReceived = delegate { };

            Run = false;
        }
        public void Dispose()
        {
            Stop();
        }

        public void Start(bool BufferMessages)
        {
            if (AcceptingTask != null && AcceptingTask.Status == TaskStatus.Running)
                return;

            this.BufferMessages = BufferMessages;

            Run = true;
            AcceptingTask = Task.Factory.StartNew(Accept);
        }
        public void Stop()
        {
            Run = false;
            foreach (Client c in Clients)
            {
                try
                {
                    c.Send(new DisconnectMessage(DisconnectType.Expected));
                    c.Dispose();
                }
                catch { }
            }
            Inner.Dispose();

            AcceptingTask.Wait();
        }

        protected void Accept()
        {
            while (Run)
            {
                Client New = null;
                try
                {
                    New = Client.Create(Inner.Accept());
                }
                catch (Exception e)
                {
                    if (e is SocketException) // Assume Listener's been told to stop
                        break;
                }

                New.MessageReceived += Client_MessageReceived;
                New.Disconnect += Client_Disconnect;
                lock (Clients)
                    Clients.Add(New);

                ClientConnect(this, New);

                New.Start(); // Set the client listening for messages
            }
        }

        public void Send(Message Msg)
        {
            lock (Clients)
            {
                foreach (Client c in Clients)
                {
                    try
                    {
                        c.Send(Msg);
                    }
                    catch { }
                }
            }
        }

        protected void Client_MessageReceived(Client Sender, Message Msg)
        {
            if (BufferMessages)
                Messages.Add(new ClientMessagePair(Sender, Msg));
            else
                ClientMessageReceived(this, Sender, Msg);
        }
        protected void Client_Disconnect(Client Sender, DisconnectMessage Msg)
        {
            Sender.Disconnect -= Client_Disconnect;
            Sender.MessageReceived -= Client_MessageReceived;

            Sender.Dispose();

            lock (Clients)
                Clients.Remove(Sender);

            ClientDisconnect(this, Sender, Msg);
        }
    }

    public class ClientMessagePair
    {
        public Client Client { get; set; }
        public Message Message { get; set; }

        public ClientMessagePair()
            : this(null, null)
        {
        }
        public ClientMessagePair(Client Client, Message Message)
        {
            this.Client = Client;
            this.Message = Message;
        }
    }
}
