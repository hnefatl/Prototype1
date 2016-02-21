using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.ComponentModel;

using Data.Models;

namespace Client.EditWindows
{
    // Used to select students from a list
    public partial class StudentSelector
        : UserControl, INotifyPropertyChanged
    {
        // Dictionary containing the name of the filter mapped to the actual filter function.
        // Each function takes a Checkable<Student>, the filter text, and returns a bool indicating
        // whether the Student should be included in the displayed list
        // Functions are specified using lambda functions for simplicity
        public readonly Dictionary<string, Func<Checkable<Student>, string, bool>> Filters = new Dictionary<string, Func<Checkable<Student>, string, bool>>()
        {
            { "No Filter", (s, f) => true },
            { "Checked", (s, f) => s.Checked },
            { "Unchecked", (s, f) => !s.Checked },
            { "First Name", (s, f) => s.Value.FirstName.ToLower().Contains(f.ToLower()) },
            { "Last Name", (s, f) => s.Value.LastName.ToLower().Contains(f.ToLower()) },
            { "Form", (s, f) => s.Value.Form.ToLower().Contains(f.ToLower()) },
            { "Year", (s, f) => Convert.ToString(s.Value.Year).ToLower().Contains(f.ToLower()) }
        };

        // The Keys from the dictionary, used for displaying in UI
        public List<string> FilterValues { get { return Filters.Keys.ToList(); } }

        // The internal list of all students
        public List<Checkable<Student>> Students { get; set; }
        // The list of students displayed after filtering
        public ObservableCollection<Checkable<Student>> FilteredStudents { get; set; }
        // The list of students actually selected
        public List<Student> SelectedStudents { get { return Students.Where(s => s.Checked).Select(s => s.Value).ToList(); } }

        // The list of classnames, and the respective Class objects
        public List<string> ClassNames { get; set; }
        public List<Class> Classes { get; set; }

        public StudentSelector()
        {
            using (DataRepository Repo = new DataRepository())
            {
                // Get the classes
                Classes = Repo.Classes.ToList();
                // Get the names of the classes
                ClassNames = Repo.Classes.Select(c => c.ClassName).ToList();
                // Insert a "dummy" class which ignores the selection
                ClassNames.Insert(0, "All students");

                // Get the students
                Students = Repo.Users.OfType<Student>().Select(s => new Checkable<Student>(s)).ToList();
                // Initialise the filtered student list
                FilteredStudents = new ObservableCollection<Checkable<Student>>(Students);
            }

            InitializeComponent();
        }

        // Call when the list of filtered students needs updating
        public void UpdateFilter()
        {
            // Prevents calls before the UI is up and running
            if (!IsInitialized)
                return;

            // Check we're running on the UI thread
            if (!Dispatcher.CheckAccess())
                Dispatcher.Invoke((Action)UpdateFilter);
            else
            {
                // Grab the filter text from the UI
                string Filter = Text_StudentFilter.Text;
                // Grab the type of filter from the UI
                string FilterType = FilterValues[Combo_FilterType.SelectedIndex];

                // Filter by the filter text
                IEnumerable<Checkable<Student>> Filtered = Students.Where(s => Filters[FilterType](s, Filter));

                // If we're filtering by class, run the secondary filetr
                if (Combo_Classes.SelectedIndex != 0)
                {
                    Class Class = Classes.Single(c => c.ClassName == (string)Combo_Classes.SelectedItem);
                    Filtered = Filtered.Where(s => Class.Students.Contains(s.Value));
                }

                // Update the list of filtered students that the UI sees
                FilteredStudents.Clear();
                foreach (Checkable<Student> s in Filtered)
                    FilteredStudents.Add(s);
            }
        }

        // These next 3 event handlers just update the filter if any relevant control was changed
        private void Combo_FilterType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateFilter();
        }
        private void Text_StudentFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateFilter();
        }
        private void Combo_Classes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateFilter();
        }


        public void OnPropertyChanged(string PropertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(PropertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
