using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Text.RegularExpressions;

using Data.Models;

namespace Client.EditWindows
{
    public partial class EditTeacher
        : EditWindow<Teacher>
    {
        // The first name of the teacher being edited
        protected string _FirstName;
        public string FirstName
        {
            get { return _FirstName; }
            set { _FirstName = value; OnPropertyChanged("FirstName"); }
        }

        // The last name of the teacher being edited
        protected string _LastName;
        public string LastName
        {
            get { return _LastName; }
            set { _LastName = value; OnPropertyChanged("LastName"); }
        }

        // The title of the teacher being edited (eg Mr, Mrs)
        protected string _TeacherTitle;
        public string TeacherTitle
        {
            get { return _TeacherTitle; }
            set { _TeacherTitle = value; OnPropertyChanged("TeacherTitle"); }
        }

        // The username of the teacher being edited
        protected string _LogonName;
        public string LogonName
        { get
            { return _LogonName; }
            set { _LogonName = value; OnPropertyChanged("LogonName"); }
        }

        // The access level of the teacher
        protected string _Access;
        public string Access
        { get
            { return _Access; }
            set { _Access = value; OnPropertyChanged("Access"); }
        }
        public string[] AccessModes { get { return Enum.GetNames(typeof(AccessMode)); } }

        // The email address of the teacher
        protected string _Email;
        public string Email
        {
            get { return _Email; }
            set { _Email = value; OnPropertyChanged("Email"); }
        }

        // Name of the department the teacher belongs to
        protected string _Department;
        public string Department
        {
            get { return _Department; }
            set { _Department = value; OnPropertyChanged("Department"); }
        }
        // Names of departments that can be selected
        public string[] Departments { get; set; }

        // Classes and Bookings can't be edited but need to be stored
        protected List<Class> Classes { get; set; }
        protected List<Booking> Bookings { get; set; }

        // The ID of the teacher being edited
        public int TeacherId { get; set; }

        public EditTeacher(Teacher Existing)
        {
            // Store the names of departments that can be chosen
            using (DataRepository Repo = new DataRepository())
                Departments = Repo.Departments.Select(d => d.Name).ToArray();

            InitializeComponent();

            // Initialise with empty values/existing values
            if (Existing == null)
            {
                FirstName = string.Empty;
                LastName = string.Empty;
                TeacherTitle = string.Empty;
                LogonName = string.Empty;
                Access = string.Empty;
                Email = string.Empty;
                Department = string.Empty;
                Classes = new List<Class>();
                Bookings = new List<Booking>();
                TeacherId = 0;
            }
            else
            {
                FirstName = Existing.FirstName;
                LastName = Existing.LastName;
                TeacherTitle = Existing.Title;
                LogonName = Existing.LogonName;
                Access = Enum.GetName(typeof(AccessMode), Existing.Access);
                Email = Existing.Email;
                Department = Existing.Department.Name;
                Classes = Existing.Classes;
                Bookings = Existing.Bookings;
                TeacherId = Existing.Id;
            }
        }

        public override Teacher GetItem()
        {
            Teacher New = new Teacher();

            try
            {
                // Fill out the details 
                New.FirstName = FirstName;
                New.LastName = LastName;
                New.Title = TeacherTitle;
                New.LogonName = LogonName;
                New.Access = (AccessMode)Enum.Parse(typeof(AccessMode), Access);
                New.Email = Email;
                // Get a reference to the actual department
                using (DataRepository Repo = new DataRepository())
                    New.Department = Repo.Departments.Single(d => d.Name == Department);
                New.Classes = Classes;
                New.Bookings = Bookings;
                New.Id = TeacherId;
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

            // Validate
            AccessMode Out;
            if (string.IsNullOrWhiteSpace(FirstName))
                Error = "You must enter a first name.";
            else if (string.IsNullOrWhiteSpace(LastName))
                Error = "You must enter a last name.";
            else if (string.IsNullOrWhiteSpace(TeacherTitle))
                Error = "You must enter a title.";
            else if (string.IsNullOrWhiteSpace(LogonName))
                Error = "You must enter a logon name.";
            else if (!Enum.TryParse(Access, out Out)) // Should never happen, we're using a combobox
                Error = "Invalid access mode.";
            else if (string.IsNullOrWhiteSpace(Department))
                Error = "You must enter a Department.";
            else if (!string.IsNullOrEmpty(Email) && !Regex.IsMatch(Email, @"[\w.-]+@[\w]+\.[.\w]+"))
                Error = "Invalid email address.";

            // Show an error or close
            if (!string.IsNullOrWhiteSpace(Error))
                MessageBox.Show(Error, "Error", MessageBoxButton.OK);
            else
            {
                DialogResult = true;
                Close();
            }
        }
    }
}
