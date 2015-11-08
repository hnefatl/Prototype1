using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Threading;

using Data.Models;
using NetCore.Client;
using NetCore.Messages;
using Client.Admin;
using Shared;

namespace Client
{
    public partial class App
        : Application
    {
        public Connection Connection { get; set; }
        public User CurrentUser { get; set; }
        public Room CurrentRoom { get; set; }


        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (!Settings.Load())
                Environment.Exit(-1); // Fatal error

            Connection = new Connection();

            NetHandler();

            MainWindow = new TrayIcon(Connection, CurrentUser, CurrentRoom);
        }

        protected void NetHandler()
        {
            Connection.Disconnect += Connection_Disconnect;

            string Address = Settings.Get<string>("ServerAddress");
            ushort Port = Settings.Get<ushort>("ServerPort");

            while (true)
            {
                // Try to connect
                bool Connected = Connection.Connect(Address, Port, new ConnectMessage(Environment.UserName, Environment.MachineName));

                if (Connected)
                {
                    Tuple<User, Room> Result = DataRepository.Initialise(Connection, new ConnectMessage(Environment.UserName, Environment.MachineName));
                    CurrentUser = Result.Item1;
                    CurrentRoom = Result.Item2;
                    if (CurrentUser == null) // Failed to initialise
                        continue;

                    break;
                }
                Thread.Sleep(1000); // Wait for an interval then try again
            }
        }

        protected void Connection_Disconnect(Connection Sender, DisconnectMessage Message)
        {
            Connection.Disconnect -= Connection_Disconnect;
            
            Environment.Exit(34652);
            //MessageBox.Show("Lost connection to the server. Will continue trying to connect in the background.");

            //NetTask = Task.Factory.StartNew(NetHandler); // Start reconnecting
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            try
            {
                Connection.Close(DisconnectType.Expected);
            }
            catch { }
        }
    }
}
