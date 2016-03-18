using System;
using System.Windows;
using System.Threading;
using System.Threading.Tasks;

using Data.Models;
using NetCore.Client;
using NetCore.Messages;

namespace Client
{
    public partial class App
        : Application
    {
        // Reference to the conenction to the server
        public Connection Connection { get; set; }
        // Reference to the current user
        public User CurrentUser { get; set; }
        // Reference to the current room
        public Room CurrentRoom { get; set; }

        // Internal thread to handle connections to the server
        protected Task NetTask { get; set; }

        // Hides the "MainWindow" property of the base class with a TrayIcon
        protected new TrayIcon MainWindow
        {
            get
            {
                // If running on a non-UI thread, invoke on the UI thread
                if (!Dispatcher.CheckAccess())
                    return (TrayIcon)Dispatcher.Invoke((Func<TrayIcon>)(() => { return this.MainWindow; }));
                else
                    return (TrayIcon)base.MainWindow; // Get the actual mainwindow cast to a TrayIcon
            }
            set
            {
                if (!Dispatcher.CheckAccess())
                    Dispatcher.Invoke((Action<TrayIcon>)(v => MainWindow = v), value);
                else
                    base.MainWindow = value;
            }
        }

        // On startup, load settings and start connecting
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (!Settings.Load())
            {
                // Failed to load settings, print error
                MessageBox.Show("Failed to load settings. Please contact an administrator.");
                Environment.Exit(-1); // Fatal error
            }

            Connection = new Connection();

            // Synchronously connect so that nothing happens until we're connected
            NetHandler();

            // Initialise the tray icon
            MainWindow = new TrayIcon(Connection, CurrentUser, CurrentRoom);

            // Show the logon message
            MainWindow.ShowBalloon("Room Booking System started", "The Room Booking System client has started.",
                System.Windows.Forms.ToolTipIcon.Info);
        }

        protected void NetHandler()
        {
            Connection.Disconnect += Connection_Disconnect;

            // Grab the address and port
            string Address = Settings.Get<string>("ServerAddress");
            ushort Port = Settings.Get<ushort>("ServerPort");

            // Keep trying to connect
            bool Connected = false;
            while (!Connected)
            {
                // Connect with given info, store success/failure
                Connected = Connection.Connect(Address, Port, new ConnectMessage(Environment.UserName, Environment.MachineName));

                if (Connected)
                {
                    // Initialise the database, store the retrieved user/room combo
                    Tuple<User, Room> Result = DataRepository.Initialise(Connection, new ConnectMessage(Environment.UserName, Environment.MachineName));
                    CurrentUser = Result.Item1;
                    CurrentRoom = Result.Item2;
                    if (CurrentUser == null) // Failed to initialise
                        continue; // Resume trying to connect

                    if (MainWindow != null)
                    {
                        // Show the tray icon again if necessary
                        MainWindow.Show();

                        MainWindow.ShowBalloon("Room Booking System started", "The Room Booking System client has started.",
                            System.Windows.Forms.ToolTipIcon.Info);
                    }
                }
                else
                    Thread.Sleep(1000); // Wait for an interval then try again
            }
        }

        protected void Connection_Disconnect(Connection Sender, DisconnectMessage Message)
        {
            // On disconnect, make sure we're invoked on the UI thread
            if (!Dispatcher.CheckAccess())
                Dispatcher.Invoke((Action<Connection, DisconnectMessage>)Connection_Disconnect, Sender, Message);
            else
            {
                Connection.Disconnect -= Connection_Disconnect;

                // Close all windows unless they're the tray icon
                foreach (Window w in Windows)
                {
                    if (w != MainWindow)
                    {
                        if (!w.Dispatcher.CheckAccess())
                            w.Dispatcher.Invoke((Action)w.Close);
                        else
                            w.Close();
                    }
                }
                // Only hide the tray icon, not close
                MainWindow.Hide();

                // Show the disconnection message
                MessageBox.Show("Lost connection to the server. Will continue trying to connect in the background.");

                // Restart the connection task
                NetTask = Task.Factory.StartNew(NetHandler);
            }
        }

        // Upon the client exiting, send a disconnect message
        private void Application_Exit(object sender, ExitEventArgs e)
        {
            try
            {
                Connection.Close(DisconnectType.Expected);
                NetTask.Dispose();
            }
            catch { }
        }
    }
}
