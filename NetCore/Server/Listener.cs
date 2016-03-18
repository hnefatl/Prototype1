using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

using NetCore.Messages;

namespace NetCore.Server
{
    // Delegate functions for events created by the Listener class
    public delegate void ClientConnectHandler(Listener Sender, Client Client);
    public delegate void ClientDisconnectHandler(Listener Sender, Client Client, DisconnectMessage Message);
    public delegate void ClientMessageReceivedHandler(Listener Sender, Client Client, Message Message);

    // Used to listen for, accept, and control connections to Clients
    public class Listener
        : IDisposable
    {
        // The internal socket to listen on
        protected Socket Inner { get; set; }

        // The local network endpoint the server runs on
        public IPEndPoint Endpoint { get { return (IPEndPoint)Inner.LocalEndPoint; } }

        // A list of all clients that have connected to the Listener
        public IList<Client> Clients { get; protected set; }
        // A thread-safe, blocking queue of messages received
        public BlockingQueue<ClientMessagePair> Messages { get; protected set; }

        // If set to true, messages will be stored in the Messages list, for consumption later.
        // If set to false, messages will be handled by firing the message received event
        public bool BufferMessages { get; protected set; }

        // Events using the delegates defined above which are fired when something important happens
        public event ClientConnectHandler ClientConnect;
        public event ClientDisconnectHandler ClientDisconnect;
        public event ClientMessageReceivedHandler ClientMessageReceived;

        // Internal thread that accepts clients asynchronously
        protected Task AcceptingTask { get; set; }
        // Internal flag representing whether the Listener is currenly running
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
            // By default, use a List as the underlying collection
        }
        public Listener(IPEndPoint Endpoint, IList<Client> ClientListType)
        {
            // Create a new TCP/IP socket
            Inner = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Inner.Bind(Endpoint); // Bind to the local endpoint
            Inner.Listen(50); // Start listening with a backlog of 50 connections

            Clients = new List<Client>();
            Messages = new BlockingQueue<ClientMessagePair>();

            ClientConnect = delegate { };
            ClientDisconnect = delegate { };
            ClientMessageReceived = delegate { };

            Run = false;
        }
        public void Dispose()
        {
            // If disposed, just call stop, which performs correct cleanup
            Stop();
        }

        // Start running the listener on the endpoint provided earlier, handling messages as specified
        public void Start(bool BufferMessages)
        {
            // If told to start while already running, ignore the call
            if (AcceptingTask != null && AcceptingTask.Status == TaskStatus.Running)
                return;

            this.BufferMessages = BufferMessages;

            Run = true;
            // Start asynchronously accepting connections
            AcceptingTask = Task.Factory.StartNew(Accept);
        }
        public void Stop()
        {
            Run = false;
            lock (Clients)
            {
                foreach (Client c in Clients)
                {
                    try
                    {
                        // Send a disconnect message, then dispose the connection
                        c.Send(new DisconnectMessage(DisconnectType.Expected));
                    }
                    catch { }
                    finally { c.Dispose(); }
                }
            }
            Inner.Dispose();

            // Wait for the accepting task to end, just to ensure the
            // listener's completely shut down before control is released
            AcceptingTask.Wait();
        }

        protected void Accept()
        {
            while (Run)
            {
                Client New = null;
                try
                {
                    // Acept a client and exchange a ConnectionMessage
                    New = Client.Create(Inner.Accept());
                }
                catch (Exception e)
                {
                    if (e is SocketException) // Assume Listener's been told to stop
                        break; // Stop listening
                }

                // Assign event handlers to when a client's message is received
                New.MessageReceived += Client_MessageReceived;
                New.Disconnect += Client_Disconnect;
                lock (Clients) // Ensure thread safe access
                    Clients.Add(New);

                // Fire the ClientConnect event
                ClientConnect(this, New);

                New.Start(); // Set the client listening for new messages
            }
        }

        // Send a message to all connected clients
        public void Send(Message Msg)
        {
            lock (Clients) // Thread safe
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

        // Called when a message is received from a client
        protected void Client_MessageReceived(Client Sender, Message Msg)
        {
            if (BufferMessages) // Store message in message queue
                Messages.Add(new ClientMessagePair(Sender, Msg));
            else // Fire message received event
                ClientMessageReceived(this, Sender, Msg);
        }
        protected void Client_Disconnect(Client Sender, DisconnectMessage Msg)
        {
            // Detach event handlers
            Sender.Disconnect -= Client_Disconnect;
            Sender.MessageReceived -= Client_MessageReceived;

            Sender.Dispose();

            lock (Clients) // Thread safe
                Clients.Remove(Sender);

            // Fire disconnection event
            ClientDisconnect(this, Sender, Msg);
        }
    }

    // Groups together a Client with a Message, for utility
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
