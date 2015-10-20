using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading;

using NetCore;
using NetCore.Messages;
using NetCore.Client;

namespace RBSClient
{
    class Program
    {
        static void Main(string[] args)
        {
            object PendingLock = new object();
            int Pending = 0;

            Connection c = new Connection();
            if (!c.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 34652), new ConnectMessage("Keith", "Keith-PC")))
            {
                Console.WriteLine("Failed to connect");
                Console.ReadKey(true);
                return;
            }

            Console.WriteLine("Connected");

            c.MessageReceived += (Connection Sender, Message m) =>
            {
                if (m is TestMessage)
                    Console.WriteLine(((TestMessage)m).Message);
                else
                    Console.WriteLine(m.ToString());
                lock (PendingLock)
                    Pending--;
            };
            c.Disconnect += (Connection Sender, DisconnectMessage m) =>
            {
                if (m.Reason == DisconnectType.Expected)
                    Console.WriteLine("Server disconnected with unknown reason.");
                else
                    Console.WriteLine("Server disconnected.");
            };

            for (int x = 0; x < 10000; x++)
            {
                if (c.Connected)
                {
                    c.Send(new TestMessage("Request"));
                    lock (PendingLock)
                        Pending++;
                }
                else
                    return;
            }

            while (Pending > 0)
                Thread.Sleep(10);

            c.Close(DisconnectType.Expected);
        }
    }
}
