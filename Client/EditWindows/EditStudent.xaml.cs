﻿using System;
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

using Data.Models;

namespace Client.EditWindows
{
    public partial class EditStudent
        : Window, INotifyPropertyChanged
    {
        protected string _FirstName;
        public string FirstName { get { return _FirstName; } set { _FirstName = value; OnPropertyChanged("FirstName"); } }

        protected string _LastName;
        public string LastName { get { return _LastName; } set { _LastName = value; OnPropertyChanged("LastName"); } }

        protected string _LogonName;
        public string LogonName { get { return _LogonName; } set { _LogonName = value; OnPropertyChanged("LogonName"); } }

        protected string _Year;
        public string Year { get { return _Year; } set { _Year = value; OnPropertyChanged("Year"); } }

        protected string _Form;
        public string Form { get { return _Form; } set { _Form = value; OnPropertyChanged("Form"); } }

        protected string _Access;
        public string Access { get { return _Access; } set { _Access = value; OnPropertyChanged("Access"); } }
        public string[] AccessModes { get; set; }

        protected List<Booking> Bookings { get; set; }
        protected List<Class> Classes { get; set; }

        public int StudentId { get; set; }

        public EditStudent(Student Existing)
        {
            InitializeComponent();

            AccessModes = Enum.GetNames(typeof(AccessMode));

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

        public Student GetStudent()
        {
            Student New = new Student();

            try
            {
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
                Error = "You must enter a Year.";
            else if (!Enum.TryParse(Access, out OutAccess)) // Should never happen, we're using a combobox
                Error = "Invalid access mode.";

            if (!string.IsNullOrWhiteSpace(Error))
                MessageBox.Show(Error, "Error", MessageBoxButton.OK);
            else
            {
                DialogResult = true;
                Close();
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