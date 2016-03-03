using System;
using System.Collections.Generic;
using System.Windows;

using Data.Models;

namespace Client.EditWindows
{
    public partial class EditStudent
        : EditWindow<Student>
    {
        // First name of the student being edited
        protected string _FirstName;
        public string FirstName
        {
            get { return _FirstName; }
            set { _FirstName = value; OnPropertyChanged("FirstName"); }
        }

        // Last name of the student being edited
        protected string _LastName;
        public string LastName
        {
            get { return _LastName; }
            set { _LastName = value; OnPropertyChanged("LastName"); }
        }

        // Username of the student being edited
        protected string _LogonName;
        public string LogonName
        {
            get { return _LogonName; }
            set { _LogonName = value; OnPropertyChanged("LogonName"); }
        }

        // Year the student's in
        protected string _Year;
        public string Year
        {
            get { return _Year; }
            set { _Year = value; OnPropertyChanged("Year"); }
        }

        // Form the student's in
        protected string _Form;
        public string Form
        {
            get { return _Form; }
            set { _Form = value; OnPropertyChanged("Form"); }
        }

        // Access level of the student
        protected string _Access;
        public string Access
        {
            get { return _Access; }
            set { _Access = value; OnPropertyChanged("Access"); }
        }
        // List of possible access modes
        public string[] AccessModes { get; set; }

        // Bookings and classes can't be edited from this window but need storing
        protected List<Booking> Bookings { get; set; }
        protected List<Class> Classes { get; set; }

        // ID of the student being edited
        public int StudentId { get; set; }

        public EditStudent(Student Existing)
        {
            // Fill out the types of AccessMode
            AccessModes = Enum.GetNames(typeof(AccessMode));

            InitializeComponent();

            // Initialise with empty/existing information
            if (Existing != null)
            {
                FirstName = Existing.FirstName;
                LastName = Existing.LastName;
                LogonName = Existing.LogonName;
                Year = Convert.ToString(Existing.Year);
                Form = Existing.Form;
                Access = Enum.GetName(typeof(AccessMode), Existing.Access);
                Bookings = Existing.Bookings;
                Classes = Existing.Classes;
                StudentId = Existing.Id;
            }
            else
            {
                FirstName = string.Empty;
                LastName = string.Empty;
                LogonName = string.Empty;
                Year = string.Empty;
                Form = string.Empty;
                Access = Enum.GetName(typeof(AccessMode), AccessMode.Student);
                Bookings = new List<Booking>();
                Classes = new List<Class>();
                StudentId = 0;
            }
        }

        public override Student GetItem()
        {
            Student New = new Student();

            try
            {
                // Fill out the new details, parsing where necessary
                New.FirstName = FirstName;
                New.LastName = LastName;
                New.LogonName = LogonName;
                New.Year = Convert.ToInt32(Year);
                New.Form = Form;
                New.Access = (AccessMode)Enum.Parse(typeof(AccessMode), Access);
                New.Bookings = Bookings;
                New.Classes = Classes;
                New.Id = StudentId;
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

            // Perform validation
            AccessMode OutAccess;
            int OutInt;
            if (string.IsNullOrWhiteSpace(FirstName))
                Error = "You must enter a first name.";
            else if (string.IsNullOrWhiteSpace(LastName))
                Error = "You must enter a last name.";
            else if (string.IsNullOrWhiteSpace(LogonName))
                Error = "You must enter a logon name.";
            else if (!int.TryParse(Year, out OutInt) || OutInt < 0)
                Error = "Year must be a non-negative integer.";
            else if (string.IsNullOrWhiteSpace(Form))
                Error = "You must enter a Form.";
            else if (!Enum.TryParse(Access, out OutAccess)) // Should never happen, we're using a combobox
                Error = "Invalid access mode.";

            // Print the error message or close 
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
