using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;

using Data.Models;
using System.Windows.Controls;

namespace Client.EditWindows
{
    public partial class EditSubject
        : EditWindow<Subject>
    {
        // The Subject Name, as used in the UI
        protected string _SubjectName;
        public string SubjectName
        {
            get { return _SubjectName; }
            set { _SubjectName = value; OnPropertyChanged("SubjectName"); }
        }

        // Gets a Brush for the demonstration square by converting the components
        public SolidColorBrush Colour
        {
            get
            {
                return new SolidColorBrush(new Color()
                {
                    R = Convert.ToByte(ColourRed),
                    G = Convert.ToByte(ColourGreen),
                    B = Convert.ToByte(ColourBlue),
                    A = byte.MaxValue,
                });
            }
            set
            {
                ColourRed = Convert.ToString(value.Color.R);
                ColourGreen = Convert.ToString(value.Color.G);
                ColourBlue = Convert.ToString(value.Color.B);
                OnPropertyChanged("Colour");
            }
        }

        // The red component as used in the UI
        protected string _ColourRed;
        public string ColourRed
        {
            get { return _ColourRed; }
            set { _ColourRed = string.IsNullOrWhiteSpace(value) ? "0" : value; OnPropertyChanged("ColourRed"); OnPropertyChanged("Colour"); }
        }

        // The green component as used in the UI
        protected string _ColourGreen;
        public string ColourGreen
        {
            get { return _ColourGreen; }
            set { _ColourGreen = string.IsNullOrWhiteSpace(value) ? "0" : value; OnPropertyChanged("ColourGreen"); OnPropertyChanged("Colour"); }
        }

        // The blue component as used in the UI
        protected string _ColourBlue;
        public string ColourBlue
        {
            get { return _ColourBlue; }
            set { _ColourBlue = string.IsNullOrWhiteSpace(value) ? "0" : value; OnPropertyChanged("ColourBlue"); OnPropertyChanged("Colour"); }
        }

        // Stores the uneditable settings until the item's recreated
        protected int SubjectId { get; set; }
        protected List<Booking> Bookings { get; set; }

        public EditSubject(Subject Existing)
        {
            if (Existing == null)
            {
                SubjectId = 0;
                SubjectName = string.Empty;
                Colour = Brushes.Black;
                Bookings = new List<Booking>();
            }
            else
            {
                SubjectId = Existing.Id;
                SubjectName = Existing.SubjectName;
                Colour = new SolidColorBrush(Existing.Colour);
                Bookings = Existing.Bookings;
            }

            InitializeComponent();
        }

        protected void ComponentTextBox_TextChanged(object sender, TextCompositionEventArgs e)
        {
            byte Out;
            // Don't allow the user to enter something that's not a valid byte
            if (!byte.TryParse((sender as TextBox).Text + e.Text, out Out))
                e.Handled = true;
        }

        public override Subject GetItem()
        {
            // Fill out the details as necessary
            return new Subject() { Id = SubjectId, SubjectName = SubjectName, Colour = Colour.Color, Bookings = Bookings };
        }

        protected void Button_Back_Click(object sender, RoutedEventArgs e)
        {
            // Close with a negative flag
            DialogResult = false;
            Close();
        }

        protected void Button_Save_Click(object sender, RoutedEventArgs e)
        {
            string Error = null;

            // Validate
            byte Temp;
            if (string.IsNullOrWhiteSpace(SubjectName))
                Error = "You must enter a subject name.";
            else if (!byte.TryParse(ColourRed, out Temp))
                Error = "Invalid value for Red component.";
            else if (!byte.TryParse(ColourGreen, out Temp))
                Error = "Invalid value for Green component.";
            else if (!byte.TryParse(ColourBlue, out Temp))
                Error = "Invalid value for Blue component.";
            else
            {
                using (DataRepository Repo = new DataRepository())
                    if (Repo.Subjects.Any(s => s.SubjectName == SubjectName))
                        Error = "Another Subject with that name already exists.";
            }

            // Show an error message or close the window
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
