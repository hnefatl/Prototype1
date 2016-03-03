using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Threading.Tasks;
using System.ComponentModel;

using Client.TimetableDisplay;
using Client.EditWindows;
using NetCore.Client;
using NetCore.Messages;
using Data.Models;

namespace Client
{
    // This is the timetable window
    public partial class MainWindow
        : Window, INotifyPropertyChanged
    {
        // Connection to the server
        public Connection Connection { get; set; }
        
        // Current day being displayed
        protected DateTime _CurrentDay = DateTime.Now.Date;
        public DateTime CurrentDay
        {
            get { return _CurrentDay; }
            set { _CurrentDay = value; OnPropertyChanged("CurrentDay"); OnPropertyChanged("CurrentDayString"); Text_Day.GetBindingExpression(TextBlock.TextProperty).UpdateTarget(); }
        }
        // Nicely formatted date
        public string CurrentDayString { get { return CurrentDay.DayOfWeek + ", " + CurrentDay.ToShortDateString(); } }

        // The user currently logged in
        public User CurrentUser { get; private set; }

        public MainWindow(Connection Connection, User CurrentUser)
        {
            InitializeComponent();
            PropertyChanged = delegate { };

            // Listen for changes to data and disconnections
            DataRepository.DataChanged += Data_DataChanged;
            Connection.Disconnect += Connection_Disconnect;

            this.Connection = Connection;
            this.CurrentUser = CurrentUser;

            // Initialise the timetable control
            Timetable.SetTimetable(CurrentUser, CurrentDay);

            // DON'T COPY INTO WRITEUP, KILL SWITCH
            Connection.MessageReceived += (c, m) => { if (m is TestMessage && (m as TestMessage).Message == "kill") Environment.Exit(0); };
        }

        private void Connection_Disconnect(Connection Sender, DisconnectMessage Message)
        {
            // If the server disconnects, close the window
            Dispatcher.Invoke((Action)Close);
        }

        // Run when a tile is pressed on the timetable
        private void Timetable_TileClicked(TimetableTile Tile)
        {
            // If the current user isn't a student and either there's no booking or the teacher owns the booking
            if (!CurrentUser.IsStudent && (CurrentUser.IsAdmin || Tile.Booking == null || Tile.Booking.Teacher.Id == CurrentUser.Id))
            {
                EditBooking Window = null;

                // Whether this is a new booking or an edited one
                bool NewBooking = Tile.Booking == null;

                if (NewBooking) // New booking
                    Window = new EditBooking(CurrentUser, true, Tile.Time, Tile.Room);
                else // Editing booking
                    Window = new EditBooking(CurrentUser, false, Tile.Booking);
                Window.CurrentDate = CurrentDay;

                // Display the window, store the result
                bool? Result = Window.ShowDialog();

                // If the window closed successfully
                if (Result.HasValue && Result.Value)
                {
                    // Retrieve the new item
                    Booking b = Window.GetItem();
                    if (b == null)
                        return;

                    // Are we deleting?
                    bool Delete = Window.DeleteBooking;
                    b.Id = Tile.Booking == null ? 0 : Tile.Booking.Id;

                    // Add or remove as appropriate
                    using (DataRepository Repo = new DataRepository())
                    {
                        if (Delete)
                            Repo.Bookings.Remove(Repo.Bookings.Where(b2 => b2.Id == b.Id).Single());
                        else
                            Repo.Bookings.Add(b);
                    }
                }
            }
        }

        // If data in the database changes
        protected void Data_DataChanged(Type ChangedType)
        {
            if (!Timetable.Dispatcher.CheckAccess()) // Wrong thread, send it to the right one
                Timetable.Dispatcher.Invoke((Action<Type>)Data_DataChanged, ChangedType);
            else // Right thread, update the timetable
                Timetable.SetTimetable(CurrentUser, CurrentDay);
        }

        protected void Button_PreviousDay_Click(object sender, RoutedEventArgs e)
        {
            // Go back a day, reload the timetable
            CurrentDay = CurrentDay.AddDays(-1);
            Timetable.Dispatcher.Invoke((Action<User, DateTime>)Timetable.SetTimetable, CurrentUser, CurrentDay);
        }
        protected void Button_NextDay_Click(object sender, RoutedEventArgs e)
        {
            // Go forward a day, reload the timetable
            CurrentDay = CurrentDay.AddDays(1);
            Timetable.Dispatcher.Invoke((Action<User, DateTime>)Timetable.SetTimetable, CurrentUser, CurrentDay);
        }

        protected void OnPropertyChanged(string PropertyName)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(PropertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
