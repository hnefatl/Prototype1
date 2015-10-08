using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using System.Data.Entity;
using System.Threading.Tasks;
using System.ComponentModel;

using Client.TimetableDisplay;
using NetCore;
using NetCore.Messages;
using NetCore.Client;
using Data;
using Data.Models;

namespace Client
{
    public partial class MainWindow
        : Window, INotifyPropertyChanged
    {
        public Connection Connection { get; set; }

        protected Task NetTask { get; set; }

        private bool _Connected;
        public bool Connected
        {
            get
            {
                return _Connected;
            }
            protected set
            {
                _Connected = value;
                OnPropertyChanged("Connected");
            }
        }

        public User CurrentUser { get; private set; }

        public MainWindow()
        {
            InitializeComponent();
            PropertyChanged = delegate { };
            Loaded += OnLoaded;

            CurrentUser = new Teacher();

            Connected = false;
            Connection = new Connection();
        }
        protected override void OnClosed(EventArgs e)
        {
            Connection.Close(DisconnectType.Expected);
            base.OnClosed(e);
        }
        protected void OnLoaded(object sender, RoutedEventArgs e)
        {
            NetHandler();



            Timetable.SetTimetable(DateTime.Now.Date);
        }

        private void Timetable_TileClicked(TimetableTile Tile)
        {
            AddBooking Window = null;
            if (Tile.Booking == null) // New booking
                Window = new AddBooking(CurrentUser);
            else // Editing booking
                Window = new AddBooking(CurrentUser, Tile.Booking.Rooms.ToList(), Tile.Booking.TimeSlot);

            bool? Result = Window.ShowDialog();
        }

        protected void NetHandler()
        {
            Connection.Disconnect += Connection_Disconnect;
            Connection.MessageReceived += Connection_MessageReceived;

            while (true)
            {
                // Try to connect
                Connected = Connection.Connect(Settings.Get<string>("ServerAddress"), Settings.Get<ushort>("ServerPort"), new ConnectMessage(Environment.UserName, Environment.MachineName));

                if (Connected)
                {
                    Connection.Send(new ConnectMessage(Environment.UserName, Environment.MachineName));
                    if (!DataRepository.Initialise(Connection))
                        continue; // Try again
                    break;
                }
                Thread.Sleep(5000); // Wait 5 seconds then try again
            }
        }

        protected void Connection_Disconnect(Connection Sender, DisconnectMessage Message)
        {
            Connection.Disconnect -= Connection_Disconnect;
            Connection.MessageReceived -= Connection_MessageReceived;

            Dispatcher.Invoke((Action)Hide);

            MessageBox.Show("Lost connection to the server. Will continue trying to connect in the background.");

            NetTask = Task.Factory.StartNew(NetHandler); // Start reconnecting
        }
        protected void Connection_MessageReceived(Connection Sender, Message Message)
        {

        }

        protected void OnPropertyChanged(string PropertyName)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(PropertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
