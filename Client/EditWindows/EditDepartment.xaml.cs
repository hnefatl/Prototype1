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

namespace Client.EditWindows
{
    public partial class EditDepartment
        : EditWindow<Department>
    {
        protected string _DepartmentName;
        public string DepartmentName
        {
            get { return _DepartmentName; }
            set { _DepartmentName = value; OnPropertyChanged("DepartmentName"); }
        }

        protected ObservableCollection<Checkable<Teacher>> _Teachers;
        public ObservableCollection<Checkable<Teacher>> Teachers
        {
            get { return _Teachers; }
            set { _Teachers = value; OnPropertyChanged("Teachers"); }
        }

        protected ObservableCollection<Checkable<Room>> _Rooms;
        public ObservableCollection<Checkable<Room>> Rooms
        {
            get { return _Rooms; }
            set { _Rooms = value; OnPropertyChanged("Rooms"); }
        }

        protected int DepartmentId { get; set; }

        public EditDepartment(Department Existing)
        {
            using (DataRepository Repo = new DataRepository())
            {
                Teachers = new ObservableCollection<Checkable<Teacher>>(Repo.Users.OfType<Teacher>().Select(t => new Checkable<Teacher>(t, Existing != null && Existing.Teachers.Contains(t))));
                Rooms = new ObservableCollection<Checkable<Room>>(Repo.Rooms.Select(r => new Checkable<Room>(r, Existing != null && Existing.Rooms.Contains(r))));
            }

            InitializeComponent();

            if (Existing == null)
            {
                DepartmentId = 0;
                DepartmentName = string.Empty;
            }
            else
            {
                DepartmentId = Existing.Id;
                DepartmentName = Existing.Name;
            }
        }

        public override Department GetItem()
        {
            Department New = new Department();

            try
            {
                New.Id = DepartmentId;
                New.Name = DepartmentName;
                New.Teachers = Teachers.Where(t => t.Checked).Select(t => t.Value).ToList();
                New.Rooms = Rooms.Where(r => r.Checked).Select(r => r.Value).ToList();
            }
            catch
            {
                return null;
            }

            return New;
        }

        private void Button_Back_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
        private void Button_Save_Click(object sender, RoutedEventArgs e)
        {
            string Error = null;

            using (DataRepository Repo = new DataRepository())
            {
                if (string.IsNullOrWhiteSpace(DepartmentName))
                    Error = "You must enter a department name.";
                else if (Repo.Departments.Any(d => d.Id != DepartmentId && d.Name == DepartmentName))
                    Error = "Another department with that name already exists.";
            }

            if (Error != null)
                MessageBox.Show(Error, "Error", MessageBoxButton.OK);
            else
            {
                DialogResult = true;
                Close();
            }
        }
    }
}
