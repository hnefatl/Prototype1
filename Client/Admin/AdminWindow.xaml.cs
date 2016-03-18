using System;
using System.Linq;
using System.Windows;
using System.ComponentModel;
using System.Collections.ObjectModel;

using Data.Models;
using NetCore.Client;

using Client.EditWindows;

namespace Client.Admin
{
    public partial class AdminWindow
        : Window, INotifyPropertyChanged
    {
        // Reference to the connection to the server
        public Connection Connection { get; set; }
        // Reference to the current user
        public User CurrentUser { get; set; }

        // List of all rooms in the system
        protected ObservableCollection<Room> _Rooms = new ObservableCollection<Room>();
        public ObservableCollection<Room> Rooms
        {
            get { return _Rooms; }
            set { _Rooms = value; OnPropertyChanged("Rooms"); }
        }

        // List of all periods in the system
        protected ObservableCollection<TimeSlot> _Periods = new ObservableCollection<TimeSlot>();
        public ObservableCollection<TimeSlot> Periods
        {
            get { return _Periods; }
            set { _Periods = value; OnPropertyChanged("Periods"); }
        }

        // List of all teachers in the system
        protected ObservableCollection<Teacher> _Teachers = new ObservableCollection<Teacher>();
        public ObservableCollection<Teacher> Teachers
        {
            get { return _Teachers; }
            set { _Teachers = value; OnPropertyChanged("Teachers"); }
        }

        // List of all students in the system
        protected ObservableCollection<Student> _Students = new ObservableCollection<Student>();
        public ObservableCollection<Student> Students
        {
            get { return _Students; }
            set { _Students = value; OnPropertyChanged("Students"); }
        }

        // List of all departments in the system
        protected ObservableCollection<Department> _Departments = new ObservableCollection<Department>();
        public ObservableCollection<Department> Departments
        {
            get { return _Departments; }
            set { _Departments = value; OnPropertyChanged("Departments"); }
        }

        // List of all classes in the system
        protected ObservableCollection<Class> _Classes = new ObservableCollection<Class>();
        public ObservableCollection<Class> Classes
        {
            get { return _Classes; }
            set { _Classes = value; OnPropertyChanged("Classes"); }
        }

        // List of all subjects in the system
        protected ObservableCollection<Subject> _Subjects = new ObservableCollection<Subject>();
        public ObservableCollection<Subject> Subjects
        {
            get { return _Subjects; }
            set { _Subjects = value; OnPropertyChanged("Subjects"); }
        }

        public AdminWindow(Connection Connection, User CurrentUser)
        {
            InitializeComponent();

            this.Connection = Connection;
            this.CurrentUser = CurrentUser;

            // Take a snapshot of the contents of the database
            using (DataRepository Repo = new DataRepository())
            {
                Rooms = new ObservableCollection<Room>(Repo.Rooms);
                Periods = new ObservableCollection<TimeSlot>(Repo.Periods);
                Teachers = new ObservableCollection<Teacher>(Repo.Users.OfType<Teacher>());
                Students = new ObservableCollection<Student>(Repo.Users.OfType<Student>());
                Departments = new ObservableCollection<Department>(Repo.Departments);
                Classes = new ObservableCollection<Class>(Repo.Classes);
                Subjects = new ObservableCollection<Subject>(Repo.Subjects);
            }
        }

        // Utility function that customises a generic item
        // Importantly, makes use of generics to reduce code bloat
        // Takes the abstract EditWindow to actually use (determines the type)
        private void EditData<T>(EditWindow<T> Wnd) where T : DataModel
        {
            // Show the window and store the close result
            bool? Result = Wnd.ShowDialog();

            // If closed succesfully, process the item
            if (Result.HasValue && Result.Value)
            {
                // Acquire the (generic) item
                T New = Wnd.GetItem();
                if (New != null)
                {
                    // Add the item to the relevant table
                    Type t = typeof(T);
                    using (DataRepository Repo = new DataRepository())
                    {
                        if (t == typeof(Booking))
                            Repo.Bookings.Add((Booking)(object)New);
                        else if (t == typeof(Class))
                            Repo.Classes.Add((Class)(object)New);
                        else if (t == typeof(Department))
                            Repo.Departments.Add((Department)(object)New);
                        else if (t == typeof(Room))
                            Repo.Rooms.Add((Room)(object)New);
                        else if (t == typeof(Student))
                            Repo.Users.Add((Student)(object)New);
                        else if (t == typeof(Subject))
                            Repo.Subjects.Add((Subject)(object)New);
                        else if (t == typeof(Teacher))
                            Repo.Users.Add((Teacher)(object)New);
                        else if (t == typeof(TimeSlot))
                            Repo.Periods.Add((TimeSlot)(object)New);
                    }
                }
            }
        }

        // Button handlers for the Room section
        private void Button_AddRoom_Click(object sender, RoutedEventArgs e)
        {
            EditData(new EditRoom(null));
        }
        private void Button_EditRoom_Click(object sender, RoutedEventArgs e)
        {
            EditData(new EditRoom((Room)List_Rooms.SelectedItem));
        }
        private void Button_DeleteRoom_Click(object sender, RoutedEventArgs e)
        {
            Room r = (Room)List_Rooms.SelectedItem;

            if (MessageBox.Show("Deleting this Room will force the deletion of " + r.Bookings.Count + " bookings.\n" +
                        "Please confirm this action.", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.No)
                return;

            using (DataRepository Repo = new DataRepository())
            {
                r.Bookings.ForEach(b => Repo.Bookings.Remove(b));
                Repo.Rooms.Remove(r);
            }
        }

        // Button handlers for the Period section
        private void Button_AddPeriod_Click(object sender, RoutedEventArgs e)
        {
            EditData(new EditPeriod(null));;
        }
        private void Button_EditPeriod_Click(object sender, RoutedEventArgs e)
        {
            EditData(new EditPeriod((TimeSlot)List_Periods.SelectedItem));;
        }
        private void Button_DeletePeriod_Click(object sender, RoutedEventArgs e)
        {
            TimeSlot t = (TimeSlot)List_Periods.SelectedItem;

            if (MessageBox.Show("Deleting this Period will force the deletion of " + t.Bookings.Count + " bookings.\n" +
                        "Please confirm this action.", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.No)
                return;

            using (DataRepository Repo = new DataRepository())
            {
                t.Bookings.ForEach(b => Repo.Bookings.Remove(b));
                Repo.Periods.Remove(t);
            }
        }

        // Button handlers for the Teacher section
        private void Button_AddTeacher_Click(object sender, RoutedEventArgs e)
        {
            EditData(new EditTeacher(null));;
        }
        private void Button_EditTeacher_Click(object sender, RoutedEventArgs e)
        {
            EditData(new EditTeacher((Teacher)List_Teachers.SelectedItem));;
        }
        private void Button_DeleteTeacher_Click(object sender, RoutedEventArgs e)
        {
            Teacher t = (Teacher)List_Teachers.SelectedItem;

            if (MessageBox.Show("Deleting this Teacher will force the deletion of " + t.Bookings.Count + " bookings and " + t.Classes.Count + " classes.\n" +
                        "Please confirm this action.", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.No)
                return;

            using (DataRepository Repo = new DataRepository())
            {
                t.Bookings.ForEach(b => Repo.Bookings.Remove(b));
                t.Classes.ForEach(c => Repo.Classes.Remove(c));
                Repo.Users.Remove(t);
            }
        }

        // Button handlers for the Student section
        private void Button_AddStudent_Click(object sender, RoutedEventArgs e)
        {
            EditData(new EditStudent(null));;
        }
        private void Button_EditStudent_Click(object sender, RoutedEventArgs e)
        {
            EditData(new EditStudent((Student)List_Students.SelectedItem));;
        }
        private void Button_DeleteStudent_Click(object sender, RoutedEventArgs e)
        {
            Student s = (Student)List_Students.SelectedItem;

            if (MessageBox.Show("Deleting this Student will remove it from " + s.Bookings.Count + " bookings and " + s.Classes.Count + " classes.\n" +
                "Please confirm this action.", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.No)
                return;

            using (DataRepository Repo = new DataRepository())
                Repo.Users.Remove(s);
        }

        // Button handlers for the Department section
        private void Button_AddDepartment_Click(object sender, RoutedEventArgs e)
        {
            EditData(new EditDepartment(null));;
        }
        private void Button_EditDepartment_Click(object sender, RoutedEventArgs e)
        {
            EditData(new EditDepartment((Department)List_Departments.SelectedItem));;
        }
        private void Button_DeleteDepartment_Click(object sender, RoutedEventArgs e)
        {
            Department d = (Department)List_Departments.SelectedItem;

            if (MessageBox.Show("Deleting this Department will force the deletion of " + d.Teachers.Count + " teachers, and " + d.Rooms.Count + " rooms.\n" +
                "Please confirm this action.", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.No)
                return;

            using (DataRepository Repo = new DataRepository())
            {
                d.Teachers.ForEach(t => Repo.Users.Remove(t));
                d.Rooms.ForEach(r => Repo.Rooms.Remove(r));
                Repo.Departments.Remove(d);
            }
        }

        // Button handlers for the Class section
        private void Button_AddClass_Click(object sender, RoutedEventArgs e)
        {
            EditData(new EditClass(null));
        }
        private void Button_EditClass_Click(object sender, RoutedEventArgs e)
        {
            EditData(new EditClass((Class)List_Classes.SelectedItem));
        }
        private void Button_DeleteClass_Click(object sender, RoutedEventArgs e)
        {
            Class c = (Class)List_Classes.SelectedItem;

            if (MessageBox.Show("Deleting this Class will remove it from 1 teacher, and " + c.Students.Count + " students.\n" +
                "Please confirm this action.", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.No)
                return;

            using (DataRepository Repo = new DataRepository())
                Repo.Classes.Remove(c);
        }

        // Button handlers for the Subject section
        private void Button_AddSubject_Click(object sender, RoutedEventArgs e)
        {
            EditData(new EditSubject(null));
        }
        private void Button_EditSubject_Click(object sender, RoutedEventArgs e)
        {
            EditData(new EditSubject((Subject)List_Subjects.SelectedItem));
        }
        private void Button_DeleteSubject_Click(object sender, RoutedEventArgs e)
        {
            Subject c = (Subject)List_Subjects.SelectedItem;

            if (MessageBox.Show("Deleting this Subject will remove it from " + c.Bookings.Count + " bookings.\n" +
                "Please confirm this action.", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.No)
                return;

            using (DataRepository Repo = new DataRepository())
                Repo.Subjects.Remove(c);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string PropertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(PropertyName));
        }
    }
}
