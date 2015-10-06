using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using NetCore;
using NetCore.Messages;
using NetCore.Server;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            object PendingMessagesLock = new object();
            int PendingMessages = 0;

            Timer t = new Timer(o => Console.Title = PendingMessages.ToString(), null, 0, 10);

            Listener Listener = new Listener(new System.Net.IPEndPoint(System.Net.IPAddress.Any, 34652), new System.Collections.ObjectModel.ObservableCollection<Client>());
            Listener.ClientConnect += (Listener l, Client c) => Console.WriteLine(c.Username + " connected.");
            Listener.ClientDisconnect += (Listener l, Client c, DisconnectMessage m) => Console.WriteLine(c.Username + " disconnected " + (m.Reason == DisconnectType.Expected ? "expectedly" : "unexpectedly") + ".");
            Listener.ClientMessageReceived += (Listener l, Client c, Message m) => { lock (PendingMessagesLock) PendingMessages++; };

            Listener.Start(true);

            Random Gen = new Random();

            while (true)
            {
                ClientMessagePair p = Listener.Messages.Take();
                if (p.Message is TestMessage)
                {
                    Console.WriteLine(((TestMessage)p.Message).Message);

                    p.Client.Send(new TestMessage("Response"));
                }
                else
                    Console.WriteLine(p.Message.ToString());
                lock (PendingMessagesLock)
                    PendingMessages--;
            }
        }
    }
}
