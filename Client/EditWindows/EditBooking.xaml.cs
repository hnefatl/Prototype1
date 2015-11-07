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

namespace Client.EditWindows
{
    public partial class EditBooking
        : EditWindow<Booking>
    {
        public User CurrentUser { get; private set; }

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

        protected List<Checkable<Student>> RawStudents { get; set; }
        public List<Checkable<Student>> Students { get; set; }
        public ObservableCollection<Checkable<Student>> FilteredStudents { get; set; }
        public List<Checkable<Student>> SelectedStudents { get { return Students.Where(s => s.Checked).ToList(); } }

        public List<string> ClassNames { get; set; }
        public List<Class> Classes { get; set; }

        protected readonly string[] _BookingTypes = Enum.GetNames(typeof(BookingType));
        public string[] BookingTypes { get { return _BookingTypes; } }

        protected BookingType _SelectedBookingType = BookingType.Single;
        public BookingType SelectedBookingType
        {
            get { return _SelectedBookingType; }
            set { _SelectedBookingType = value; OnPropertyChanged("SelectedBookingType"); }
        }

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

        public List<Subject> Subjects { get; private set; }
        private Subject _SelectedSubject;
        public Subject SelectedSubject
        {
            get { return _SelectedSubject; }
            set { _SelectedSubject = value; OnPropertyChanged("SelectedSubject"); }
        }

        public List<Teacher> Teachers { get; private set; }
        private Teacher _SelectedTeacher;
        public Teacher SelectedTeacher
        {
            get { return _SelectedTeacher; }
            set { _SelectedTeacher = value; OnPropertyChanged("SelectedTeacher"); }
        }

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
                OnPropertyChanged("NewPeriod");
            }
        }
        public bool NewPeriod { get { return !ExistingPeriod; } set { ExistingPeriod = !value; } }

        public int BookingId { get; set; }

        public bool DeleteBooking { get; private set; }

        public DateTime CurrentDate { get; set; }

        public EditBooking(User CurrentUser, bool NewBooking) // For making a new booking
            : this(CurrentUser, NewBooking, 0, new List<Room>(), null, null, new List<Student>(), null, null, BookingType.Single)
        {
        }
        public EditBooking(User CurrentUser, bool NewBooking, TimeSlot StartTime, Room StartRoom) // For making a new booking
            : this(CurrentUser, NewBooking, 0, new List<Room>(), StartRoom, StartTime, new List<Student>(), null, null, BookingType.Single)
        {
        }
        public EditBooking(User CurrentUser, bool NewBooking, Booking Booking)
            : this(CurrentUser, NewBooking, Booking.Id, Booking.Rooms, null, Booking.TimeSlot, Booking.Students, Booking.Subject, Booking.Teacher, Booking.BookingType)
        {
        }
        public EditBooking(User CurrentUser, bool NewBooking, int Id, List<Room> SelectedRooms, Room StartRoom, TimeSlot TimeSlot, List<Student> SelectedStudents, Subject Subject, Teacher Teacher, BookingType BookingType) // For editing an existing booking
        {
            this.CurrentUser = CurrentUser;
            SelectedBookingType = BookingType;
            BookingId = Id;
            DeleteBooking = false;

            using (DataRepository Repo = new DataRepository())
            {
                Classes = Repo.Classes.ToList();
                ClassNames = Repo.Classes.Select(c => c.ClassName).ToList();
                ClassNames.Insert(0, "All students");

                Rooms = new ObservableCollection<Checkable<Room>>(Repo.Rooms.ToList().Select(r1 => new Checkable<Room>(r1, (StartRoom != null && r1.Id == StartRoom.Id) || SelectedRooms.Any(r2 => r1.Id == r2.Id))));
                Periods = new ObservableCollection<TimeSlot>(Repo.Periods);
                RawStudents = Repo.Users.OfType<Student>().Select(s1 => new Checkable<Student>(s1, SelectedStudents.Any(s2 => s2.Id == s1.Id))).ToList();
                Students = RawStudents.ToList();
                FilteredStudents = new ObservableCollection<Checkable<Student>>(Students);

                Subjects = Repo.Subjects.ToList();
                if (Subject != null)
                    SelectedSubject = Subject;

                Teachers = Repo.Users.OfType<Teacher>().ToList();
                if (Teacher != null)
                    SelectedTeacher = Teacher;
                else if (CurrentUser is Teacher)
                    SelectedTeacher = (Teacher)CurrentUser;

                if (TimeSlot == null)
                {
                    ExistingPeriod = true;
                    SelectedTimeslot = new TimeSlot(new TimeSpan(9, 0, 0), new TimeSpan(10, 0, 0));
                }
                else
                {
                    ExistingPeriod = Periods.Contains(TimeSlot);
                    SelectedTimeslot = TimeSlot;
                }
            }

            InitializeComponent();

            // If in release mode, restrict some of the options available 
#if !DEBUG
            if (!CurrentUser.IsAdmin) // Only let the teacher be changed if the user is an admin
                Combo_Teacher.IsEnabled = false;

            if (CurrentUser != Teacher && !CurrentUser.IsAdmin) // Only let any edits be made if the current user is the owner/admin
                Container.IsEnabled = false;
#endif

            if (NewBooking) // Must be making a new booking
                Button_Delete.IsEnabled = false;
        }

        public override Booking GetItem()
        {
            if (SelectedTimeslot == null || SelectedRooms == null || SelectedRooms.Count == 0 || SelectedSubject == null || SelectedStudents == null || SelectedTeacher == null)
                return null;

            return new Booking(SelectedTimeslot, SelectedRooms.Select(c => c.Value).ToList(), SelectedSubject,
                SelectedStudents.Select(c => c.Value).ToList(), SelectedTeacher, SelectedBookingType)
            { Id = BookingId, Date = CurrentDate };
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

        private void Combo_Classes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Students.Clear();
            if (Combo_Classes.SelectedIndex == 0) // First item is always "No class"
            {
                foreach (Checkable<Student> s in RawStudents)
                    Students.Add(s);
            }
            else
            {
                Class Class = Classes.Single(c => c.ClassName == (string)Combo_Classes.SelectedItem);
                foreach (Checkable<Student> s in RawStudents.Where(s => Class.Students.Contains(s.Value)))
                    Students.Add(s);
            }
            UpdateFilter();
        }

        private void Button_Submit_Click(object sender, RoutedEventArgs e)
        {
            string Error = string.Empty;
            if (SelectedRooms.Count == 0)
                Error = "You must select at least one room.";
            else if (SelectedSubject == null)
                Error = "You must select a subject.";
            else if (SelectedTeacher == null)
                Error = "You must select a teacher";
            else
            {
                Booking b = GetItem();
                if (b == null)
                    Error = "Invalid booking.";
                else
                {
                    using (DataRepository Repo = new DataRepository())
                        if (b.Conflicts(Repo.Bookings.Cast<DataModel>().ToList()))
                            Error = "Booking conflicts with another booking.";
                }
            }

            if (!string.IsNullOrWhiteSpace(Error))
                MessageBox.Show(Error, "Error");
            else
            {
                DialogResult = true;
                Close();
            }
        }
        private void Button_Delete_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult r = MessageBox.Show("Are you sure you want to delete this booking?", "Delete Booking", MessageBoxButton.YesNo);
            if (r == MessageBoxResult.Yes)
            {
                DeleteBooking = true;
                DialogResult = true;
                Close();
            }
        }
    }
}
