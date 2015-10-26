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
using NetCore.Client;
using NetCore.Messages;
using NetCore.Messages.DataMessages;
using Data;
using Data.Models;

namespace Client
{
    public partial class MainWindow
        : Window, INotifyPropertyChanged
    {
        public Connection Connection { get; set; }

        protected Task NetTask { get; set; }
        
        protected DateTime _CurrentDay = DateTime.Now.Date;
        public DateTime CurrentDay
        {
            get { return _CurrentDay; }
            set { _CurrentDay = value; OnPropertyChanged("CurrentDay"); OnPropertyChanged("CurrentDayString"); Text_Day.GetBindingExpression(TextBlock.TextProperty).UpdateTarget(); }
        }
        public string CurrentDayString { get { return CurrentDay.DayOfWeek + ", " + CurrentDay.ToShortDateString(); } }

        public User CurrentUser { get; private set; }

        public MainWindow(Connection Connection, User CurrentUser)
        {
            InitializeComponent();
            PropertyChanged = delegate { };

            DataRepository.DataChanged += Data_DataChanged;
            Connection.Disconnect += Connection_Disconnect;

            this.Connection = Connection;
            this.CurrentUser = CurrentUser;

            Timetable.Dispatcher.Invoke((Action<User, DateTime>)Timetable.SetTimetable, CurrentUser, CurrentDay);
        }

        private void Connection_Disconnect(Connection Sender, DisconnectMessage Message)
        {
            Dispatcher.Invoke((Action)Close);
        }

        private void Timetable_TileClicked(TimetableTile Tile)
        {
            AddBooking Window = null;

            bool NewBooking = Tile.Booking == null;

            if (NewBooking) // New booking
                Window = new AddBooking(CurrentUser, true, Tile.Time, Tile.Room);
            else // Editing booking
                Window = new AddBooking(CurrentUser, false, Tile.Booking);
            Window.CurrentDate = CurrentDay;

            bool? Result = Window.ShowDialog();

            Booking b = Window.GetBooking();

            if (b != null && Result.HasValue && Result.Value) // Send new/edit/delete booking
            {
                bool Delete = Window.DeleteBooking;
                b.Id = Tile.Booking == null ? 0 : Tile.Booking.Id;

                using (DataRepository Repo = new DataRepository())
                {
                    if (Delete)
                        Repo.Bookings.Remove(Repo.Bookings.Where(b2 => b2.Id == b.Id).Single());
                    else
                        Repo.Bookings.Add(b);
                }
            }
        }

        protected void Data_DataChanged(List<DataModel> OldItems, List<DataModel> NewItems)
        {
            Timetable.Dispatcher.Invoke((Action<User, DateTime>)Timetable.SetTimetable, CurrentUser, CurrentDay);
        }

        protected void Button_PreviousDay_Click(object sender, RoutedEventArgs e)
        {
            CurrentDay = CurrentDay.AddDays(-1);
            Timetable.Dispatcher.Invoke((Action<User, DateTime>)Timetable.SetTimetable, CurrentUser, CurrentDay);
        }
        protected void Button_NextDay_Click(object sender, RoutedEventArgs e)
        {
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
