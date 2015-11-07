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
        public Connection Connection { get; set; }
        public User CurrentUser { get; set; }

        protected ObservableCollection<Room> _Rooms = new ObservableCollection<Room>();
        public ObservableCollection<Room> Rooms
        {
            get { return _Rooms; }
            set { _Rooms = value; OnPropertyChanged("Rooms"); }
        }

        protected ObservableCollection<TimeSlot> _Periods = new ObservableCollection<TimeSlot>();
        public ObservableCollection<TimeSlot> Periods
        {
            get { return _Periods; }
            set { _Periods = value; OnPropertyChanged("Periods"); }
        }

        protected ObservableCollection<Teacher> _Teachers = new ObservableCollection<Teacher>();
        public ObservableCollection<Teacher> Teachers
        {
            get { return _Teachers; }
            set { _Teachers = value; OnPropertyChanged("Teachers"); }
        }

        protected ObservableCollection<Student> _Students = new ObservableCollection<Student>();
        public ObservableCollection<Student> Students
        {
            get { return _Students; }
            set { _Students = value; OnPropertyChanged("Students"); }
        }

        protected ObservableCollection<Department> _Departments = new ObservableCollection<Department>();
        public ObservableCollection<Department> Departments
        {
            get { return _Departments; }
            set { _Departments = value; OnPropertyChanged("Departments"); }
        }

        protected ObservableCollection<Class> _Classes = new ObservableCollection<Class>();
        public ObservableCollection<Class> Classes
        {
            get { return _Classes; }
            set { _Classes = value; OnPropertyChanged("Classes"); }
        }

        public AdminWindow(Connection Connection, User CurrentUser)
        {
            InitializeComponent();

            this.Connection = Connection;
            this.CurrentUser = CurrentUser;

            DataRepository.DataChanged += DataRepository_DataChanged;

            using (DataRepository Repo = new DataRepository())
            {
                Rooms = new ObservableCollection<Room>(Repo.Rooms);
                Periods = new ObservableCollection<TimeSlot>(Repo.Periods);
                Teachers = new ObservableCollection<Teacher>(Repo.Users.OfType<Teacher>());
                Students = new ObservableCollection<Student>(Repo.Users.OfType<Student>());
                Departments = new ObservableCollection<Department>(Repo.Departments);
                Classes = new ObservableCollection<Class>(Repo.Classes);
            }
        }

        private void DataRepository_DataChanged(Type ChangedType)
        {
            using (DataRepository Repo = new DataRepository(false))
            {
                if (ChangedType == typeof(Room))
                    Rooms = new ObservableCollection<Room>(Repo.Rooms);
                else if (ChangedType == typeof(TimeSlot))
                    Periods = new ObservableCollection<TimeSlot>(Repo.Periods);
                else if (ChangedType == typeof(Teacher))
                    Teachers = new ObservableCollection<Teacher>(Repo.Users.OfType<Teacher>());
                else if (ChangedType == typeof(Student))
                    Students = new ObservableCollection<Student>(Repo.Users.OfType<Student>());
                else if (ChangedType == typeof(User))
                {
                    Teachers = new ObservableCollection<Teacher>(Repo.Users.OfType<Teacher>());
                    Students = new ObservableCollection<Student>(Repo.Users.OfType<Student>());
                }
                else if (ChangedType == typeof(Department))
                    Departments = new ObservableCollection<Department>(Repo.Departments);
                else if (ChangedType == typeof(Class))
                    Classes = new ObservableCollection<Class>(Repo.Classes);
            }
        }

        private void EditData<T>(EditWindow<T> Wnd) where T : DataModel
        {
            bool? Result = Wnd.ShowDialog();

            if (Result.HasValue && Result.Value)
            {
                T New = Wnd.GetItem();
                if (New != null)
                {
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
            {
                Repo.Users.Remove(s);
            }
        }
        
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

            if (MessageBox.Show("Deleting this Department will force the deletion of " + d.Teachers.Count + " teachers, and " + d.Rooms + " rooms.\n" +
                "Please confirm this action.", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.No)
                return;

            using (DataRepository Repo = new DataRepository())
            {
                d.Teachers.ForEach(t => Repo.Users.Remove(t));
                d.Rooms.ForEach(r => Repo.Rooms.Remove(r));
                Repo.Departments.Remove(d);
            }
        }
        
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
            {
                Repo.Classes.Remove(c);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string PropertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(PropertyName));
        }
    }
}
