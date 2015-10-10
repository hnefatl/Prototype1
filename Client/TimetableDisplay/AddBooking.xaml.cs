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
        public List<Checkable<Room>> SelectedRooms { get { return Rooms.Where(r => r.Checked).ToList(); } }

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

        public List<Checkable<Student>> Students { get; set; }
        public ObservableCollection<Checkable<Student>> FilteredStudents { get; set; }
        public List<Checkable<Student>> SelectedStudents { get { return Students.Where(s => s.Checked).ToList(); } }

        public readonly Dictionary<string, Func<Checkable<Student>, string, bool>> Filters = new Dictionary<string, Func<Checkable<Student>, string, bool>>()
        {
            { "No Filter", (s, f) => true },
            { "Checked", (s, f) => s.Checked },
            { "Unchecked", (s, f) => !s.Checked },
            { "First Name", (s, f) => s.Value.FirstName.ToLower().Contains(f.ToLower()) },
            { "Last Name", (s, f) => s.Value.LastName.ToLower().Contains(f.ToLower()) },
            { "Form", (s, f) => s.Value.Form.ToLower().Contains(f.ToLower()) },
            { "Year", (s, f) => Convert.ToString(s.Value.Year).ToLower().Contains(f.ToLower()) }
        };
        public List<string> FilterValues { get { return Filters.Keys.ToList(); } }

        protected bool _ExistingPeriod = true;
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
            : this(CurrentUser, new List<Room>(), null, new List<Student>())
        {
        }
        public AddBooking(User CurrentUser, List<Room> SelectedRooms, TimeSlot TimeSlot, List<Student> SelectedStudents) // For editing an existing booking
        {
            PropertyChanged = delegate { };

            this.CurrentUser = CurrentUser;

            using (DataRepository Repo = new DataRepository())
            {
                Rooms = new ObservableCollection<Checkable<Room>>(Repo.Rooms.ToList().Select(r1 => new Checkable<Room>(r1, SelectedRooms.Any(r2 => r1.Id == r2.Id))));
                Periods = new ObservableCollection<TimeSlot>(Repo.Periods);
                Students = Repo.Students.ToList().Select(s1 => new Checkable<Student>(s1, SelectedStudents.Any(s2 => s2.Id == s1.Id))).ToList();
                FilteredStudents = new ObservableCollection<Checkable<Student>>(Students);

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

        public void UpdateFilter()
        {
            if (!Dispatcher.CheckAccess())
                Dispatcher.Invoke((Action)UpdateFilter);
            else
            {
                string Filter = Text_StudentFilter.Text;
                string FilterType = FilterValues[Combo_FilterType.SelectedIndex];

                IEnumerable<Checkable<Student>> Filtered = Students.Where(s => Filters[FilterType](s, Filter));

                FilteredStudents.Clear();
                foreach (Checkable<Student> s in Filtered)
                    FilteredStudents.Add(s);
            }
        }
        
        private void Combo_FilterType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateFilter();
        }
        private void Text_StudentFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateFilter();
        }
    }
}
