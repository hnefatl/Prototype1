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
                Teachers = new ObservableCollection<Teacher>(Repo.Teachers);
                Students = new ObservableCollection<Student>(Repo.Students);
                Departments = new ObservableCollection<Department>(Repo.Departments);
                Classes = new ObservableCollection<Class>(Repo.Classes);
            }
        }

        private void DataRepository_DataChanged(List<DataModel> OldItems, List<DataModel> NewItems)
        {
            // Need to find out which Data type has been changed
            IEnumerable<DataModel> Temp = OldItems.Union(NewItems);
            if (Temp.Count() > 0)
            {
                Type t = Temp.First().GetType();
                using (DataRepository Repo = new DataRepository())
                {
                    if (t == typeof(Room))
                        Rooms = new ObservableCollection<Room>(Repo.Rooms);
                    else if (t == typeof(TimeSlot))
                        Periods = new ObservableCollection<TimeSlot>(Repo.Periods);
                    else if (t == typeof(Teacher))
                        Teachers = new ObservableCollection<Teacher>(Repo.Teachers);
                    else if (t == typeof(Student))
                        Students = new ObservableCollection<Student>(Repo.Students);
                    else if (t == typeof(Department))
                        Departments = new ObservableCollection<Department>(Repo.Departments);
                    else if (t == typeof(Class))
                        Classes = new ObservableCollection<Class>(Repo.Classes);
                }
            }
        }

        private void Button_AddRoom_Click(object sender, RoutedEventArgs e)
        {
            EditRoom(null);
        }
        private void Button_EditRoom_Click(object sender, RoutedEventArgs e)
        {
            EditRoom((Room)List_Rooms.SelectedItem);
        }
        private void EditRoom(Room Existing)
        {
            EditRoom Wnd = new EditRoom(Existing);
            bool? Result = Wnd.ShowDialog();

            if (Result.HasValue && Result.Value)
            {
                Room New = Wnd.GetRoom();
                if (New != null)
                {
                    using (DataRepository Repo = new DataRepository())
                        Repo.Rooms.Add(New);
                }
            }
        }
        private void Button_DeleteRoom_Click(object sender, RoutedEventArgs e)
        {
            using (DataRepository Repo = new DataRepository())
                Repo.Rooms.Remove((Room)List_Rooms.SelectedItem);
        }

        private void Button_AddPeriod_Click(object sender, RoutedEventArgs e)
        {
            EditPeriod(null);
        }
        private void Button_EditPeriod_Click(object sender, RoutedEventArgs e)
        {
            EditPeriod((TimeSlot)List_Periods.SelectedItem);
        }
        private void EditPeriod(TimeSlot Existing)
        {
            EditPeriod Wnd = new EditPeriod(Existing);
            bool? Result = Wnd.ShowDialog();

            if (Result.HasValue && Result.Value)
            {
                TimeSlot New = Wnd.GetPeriod();
                if (New != null)
                {
                    using (DataRepository Repo = new DataRepository())
                        Repo.Periods.Add(New);
                }
            }
        }
        private void Button_DeletePeriod_Click(object sender, RoutedEventArgs e)
        {
            using (DataRepository Repo = new DataRepository())
                Repo.Periods.Remove((TimeSlot)List_Periods.SelectedItem);
        }



        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string PropertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(PropertyName));
        }
    }
}
