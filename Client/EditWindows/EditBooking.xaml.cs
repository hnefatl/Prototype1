using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Collections.ObjectModel;

using Data.Models;

namespace Client.EditWindows
{
    public partial class EditBooking
        : EditWindow<Booking>
    {
        // The user that's currently logged on
        public User CurrentUser { get; private set; }

        // All the Rooms that can be booked
        public ObservableCollection<Checkable<Room>> Rooms { get; set; }
        // Utility property to collect the selected rooms
        public List<Checkable<Room>> SelectedRooms { get { return Rooms.Where(r => r.Checked).ToList(); } }

        // Periods that can be booked in
        public ObservableCollection<TimeSlot> Periods { get; set; }
        // The single timeslot that's been booked
        private TimeSlot _SelectedTimeslot;
        public TimeSlot SelectedTimeslot
        {
            get { return _SelectedTimeslot; }
            set { _SelectedTimeslot = value; OnPropertyChanged("SelectedTimeslot"); }
        }

        // Types of booking (protected array with a public accessor property)
        protected readonly string[] _BookingTypes = Enum.GetNames(typeof(BookingType));
        public string[] BookingTypes { get { return _BookingTypes; } }
        // The currently selected recurrence mode
        protected BookingType _SelectedBookingType = BookingType.Single;
        public BookingType SelectedBookingType
        {
            get { return _SelectedBookingType; }
            set { _SelectedBookingType = value; OnPropertyChanged("SelectedBookingType"); }
        }

        // The subjects that can be selected
        public List<Subject> Subjects { get; private set; }
        // The subject that's been selected
        private Subject _SelectedSubject;
        public Subject SelectedSubject
        {
            get { return _SelectedSubject; }
            set { _SelectedSubject = value; OnPropertyChanged("SelectedSubject"); }
        }

        // The Teachers that can be selected
        public List<Teacher> Teachers { get; private set; }
        // The teacher that's currently selected
        private Teacher _SelectedTeacher;
        public Teacher SelectedTeacher
        {
            get { return _SelectedTeacher; }
            set { _SelectedTeacher = value; OnPropertyChanged("SelectedTeacher"); }
        }

        // The ID of the booking returned by this window. Set to either 0 to
        // represent a new booking or the ID of the booking being edited.
        protected int BookingId { get; set; }

        // Whether or not a deletion ws requested by the user
        public bool DeleteBooking { get; private set; }

        // The current date displayed on the Timetable
        public DateTime CurrentDate { get; set; }


        // For making a new booking given the time and room
        public EditBooking(User CurrentUser, bool NewBooking, TimeSlot StartTime, Room StartRoom)
            : this(CurrentUser, NewBooking, 0, new List<Room>(), StartRoom, StartTime, new List<Student>(), null, null, BookingType.Single)
        {
        }
        // For editing an existing booking given the booking itself
        public EditBooking(User CurrentUser, bool NewBooking, Booking Booking)
            : this(CurrentUser, NewBooking, Booking.Id, Booking.Rooms, null, Booking.TimeSlot, Booking.Students, Booking.Subject, Booking.Teacher, Booking.BookingType)
        {
        }

        // Derived constructor to handle the common tasks of the above two constructors
        public EditBooking(User CurrentUser, bool NewBooking, int Id, List<Room> SelectedRooms, Room StartRoom, TimeSlot TimeSlot, List<Student> SelectedStudents, Subject Subject, Teacher Teacher, BookingType BookingType) // For editing an existing booking
        {
            // Store necessary data
            this.CurrentUser = CurrentUser;
            SelectedBookingType = BookingType;
            BookingId = Id;
            DeleteBooking = false;
            SelectedTimeslot = TimeSlot;

            using (DataRepository Repo = new DataRepository())
            {
                // Store the rooms
                Rooms = new ObservableCollection<Checkable<Room>>(Repo.Rooms.ToList().Select(r1 => new Checkable<Room>(r1, (StartRoom != null && r1.Id == StartRoom.Id) || SelectedRooms.Any(r2 => r1.Id == r2.Id))));
                // Store the timeslots
                Periods = new ObservableCollection<TimeSlot>(Repo.Periods);

                // Store the subjects
                Subjects = Repo.Subjects.ToList();
                if (Subject != null) // Initialise if needed
                    SelectedSubject = Subject;

                // Store teacher list
                Teachers = Repo.Users.OfType<Teacher>().ToList();
                if (Teacher != null) // Initialise if needed
                    SelectedTeacher = Teacher;
                else if (CurrentUser is Teacher) // Else give default value
                    SelectedTeacher = (Teacher)CurrentUser;
            }

            // Initialise the UI
            InitializeComponent();

            // Perform initial selection of students
            StudentSelector.Students.Where(s => SelectedStudents.Contains(s.Value)).ToList().ForEach(s => s.Checked = true);

            if (!CurrentUser.IsAdmin) // Only let the teacher be changed if the user is an admin
                Combo_Teacher.IsEnabled = false;

            if (NewBooking) // Must be editing a booking to delete it
                Button_Delete.IsEnabled = false;
        }

        // Return the Booking object created by this window
        public override Booking GetItem()
        {
            // Validate
            if (SelectedTimeslot == null || SelectedRooms == null || SelectedRooms.Count == 0 || SelectedSubject == null || StudentSelector.SelectedStudents == null || SelectedTeacher == null)
                return null;

            return new Booking(SelectedTimeslot, SelectedRooms.Select(c => c.Value).ToList(), SelectedSubject,
                StudentSelector.SelectedStudents.ToList(), SelectedTeacher, SelectedBookingType)
            { Id = BookingId, Date = CurrentDate };
        }

        // When told to submit the changes
        private void Button_Submit_Click(object sender, RoutedEventArgs e)
        {
            // Perform validation
            string Error = string.Empty;
            if (SelectedRooms.Count == 0)
                Error = "You must select at least one room.";
            else if (SelectedSubject == null)
                Error = "You must select a subject.";
            else if (SelectedTeacher == null)
                Error = "You must select a teacher";
            else
            {
                // Check for conflicts
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

            // Display error or close the window
            if (!string.IsNullOrWhiteSpace(Error))
                MessageBox.Show(Error, "Error");
            else
            {
                DialogResult = true;
                Close();
            }
        }

        // When told to delete the item
        private void Button_Delete_Click(object sender, RoutedEventArgs e)
        {
            // Confirm then signal deletion
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
