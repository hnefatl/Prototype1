using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

using Data.Models;

namespace Client.EditWindows
{
    // Window for editing a a Class object
    public partial class EditClass
        : EditWindow<Class>
    {
        // Name of the class
        protected string _ClassName;
        public string ClassName
        {
            get { return _ClassName; }
            set { _ClassName = value; OnPropertyChanged("ClassName"); }
        }

        // Name of the teacher teaching the class
        protected string _Teacher;
        public string Teacher
        {
            get { return _Teacher; }
            set { _Teacher = value; OnPropertyChanged("Teacher"); }
        }
        // Internal array of all teacher names
        public string[] Teachers { get; set; }
        // Parallel array to the above holding the IDs of the teachers
        public int[] TeacherIds { get; set; }

        // ID of the Class being edited/created
        public int ClassId { get; set; }

        public EditClass(Class Existing)
        {
            // Store the values that can be selected
            using (DataRepository Repo = new DataRepository())
            {
                IEnumerable<Teacher> ts = Repo.Users.OfType<Teacher>();
                Teachers = ts.Select(t => t.InformalName).ToArray();
                TeacherIds = ts.Select(t => t.Id).ToArray();
            }

            InitializeComponent();

            // If new Class, enter default values
            if (Existing == null)
            {
                ClassName = string.Empty;
                Teacher = string.Empty;
                ClassId = 0;
            }
            else // Existing class, load the values from it
            {
                ClassName = Existing.ClassName;
                Teacher = Existing.Owner.InformalName;
                ClassId = Existing.Id;

                // Select already involved students in advance
                Students.Students.Where(s => Existing.Students.Contains(s.Value)).ToList().ForEach(s => s.Checked = true);
            }
        }

        public override Class GetItem()
        {
            Class New = new Class();

            try
            {
                // Fill in values
                New.Id = ClassId;
                New.ClassName = ClassName;
                New.Students = Students.SelectedStudents;

                using (DataRepository Repo = new DataRepository())
                {
                    // Find the selected teacher
                    New.Owner = Repo.Users.OfType<Teacher>().Single(t => t.Id == TeacherIds[Combo_Teacher.SelectedIndex]);
                }
            }
            catch
            {
                return null;
            }

            return New;
        }

        private void Button_Back_Click(object sender, RoutedEventArgs e)
        {
            // Set the "cancel" flag and close
            DialogResult = false;
            Close();
        }
        private void Button_Save_Click(object sender, RoutedEventArgs e)
        {
            string Error = null;

            // Perform validation 
            using (DataRepository Repo = new DataRepository())
            {
                if (string.IsNullOrWhiteSpace(ClassName))
                    Error = "You must enter a class name.";
                else if (string.IsNullOrWhiteSpace(Teacher))
                    Error = "You must enter a Teacher.";
                else if (Repo.Classes.Any(c => c.Id != ClassId && c.ClassName == ClassName))
                    Error = "Another class with that name already exists.";
            }

            // Error or close window
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
