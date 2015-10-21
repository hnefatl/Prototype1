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

namespace Client
{
    public partial class App
        : Application
    {
        public Connection Connection { get; set; }
        public User CurrentUser { get; set; }



        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (!Settings.Load())
                Environment.Exit(-1); // Fatal error

            Settings.Clear();
            Settings.Add("ServerAddress", "127.0.0.1");
            Settings.Add("ServerPort", "34652");
            Settings.Save();

            Connection = new Connection();

            NetHandler();

            TrayIcon Wnd = new TrayIcon(Connection, CurrentUser);
        }

        protected void NetHandler()
        {
            Connection.Disconnect += Connection_Disconnect;

            while (true)
            {
                // Try to connect
                bool Connected = Connection.Connect(Settings.Get<string>("ServerAddress"), Settings.Get<ushort>("ServerPort"), new ConnectMessage(Environment.UserName, Environment.MachineName));

                if (Connected)
                {
                    CurrentUser = DataRepository.Initialise(Connection, new ConnectMessage(Environment.UserName, Environment.MachineName));
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
