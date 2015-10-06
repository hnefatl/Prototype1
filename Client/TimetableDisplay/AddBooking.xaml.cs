using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using Data.Models;

namespace Client.TimetableDisplay
{
    public partial class AddBooking
        : Window, INotifyPropertyChanged
    {
        public ObservableCollection<Checkable<Room>> Rooms { get; set; }
        public List<Checkable<Room>> SelectedRooms
        {
            get
            {
                return Rooms.Where(r => r.Checked).ToList();
            }
        }

        public ObservableCollection<TimeSlot> Periods { get; set; }
        private TimeSlot _SelectedTimeslot;
        public TimeSlot SelectedTimeslot
        {
            get
            {
                return _SelectedTimeslot;
            }
            set
            {
                _SelectedTimeslot = value;
                OnPropertyChanged("SelectedTimeslot");
            }
        }

        protected bool _ExistingPeriod;
        public bool ExistingPeriod
        {
            get
            {
                return _ExistingPeriod;
            }
            set
            {
                _ExistingPeriod = value;
                OnPropertyChanged("ExistingPeriod");
            }
        }

        public User CurrentUser { get; private set; }

        public AddBooking(User CurrentUser) // For making a new booking
            : this(CurrentUser, new List<Room>(), null)
        {
        }
        public AddBooking(User CurrentUser, List<Room> SelectedRooms, TimeSlot TimeSlot) // For editing an existing booking
        {
            PropertyChanged = delegate { };

            this.CurrentUser = CurrentUser;

            using (DataRepository Repo = new DataRepository())
            {
                Rooms = new ObservableCollection<Checkable<Room>>(Repo.Rooms.ToList().Select(r1 => new Checkable<Room>(r1) { Checked = (SelectedRooms.Any(r2 => r1.RoomName == r2.RoomName)) }));
                Periods = new ObservableCollection<TimeSlot>(Repo.Periods);

                if (TimeSlot == null)
                {
                    ExistingPeriod = false;
                    SelectedTimeslot = new TimeSlot(new TimeSpan(9, 0, 0), new TimeSpan(10, 0, 0));
                }
                else
                {
                    ExistingPeriod = Periods.Contains(TimeSlot);
                    SelectedTimeslot = TimeSlot;
                }
            }

            InitializeComponent();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string PropertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(PropertyName));
        }
    }
}
