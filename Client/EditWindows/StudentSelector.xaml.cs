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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.ComponentModel;

using Data.Models;

namespace Client.EditWindows
{
    public partial class StudentSelector
        : UserControl, INotifyPropertyChanged
    {
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

        public List<string> FilterValues { get { return Filters.Keys.ToList(); } }

        protected List<Checkable<Student>> RawStudents { get; set; }
        protected List<Checkable<Student>> Students { get; set; }
        public ObservableCollection<Checkable<Student>> FilteredStudents { get; set; }
        public List<Student> SelectedStudents { get { return Students.Where(s => s.Checked).Select(s => s.Value).ToList(); } }

        public List<string> ClassNames { get; set; }
        public List<Class> Classes { get; set; }

        public StudentSelector()
        {
            using (DataRepository Repo = new DataRepository())
            {
                Classes = Repo.Classes.ToList();
                ClassNames = Repo.Classes.Select(c => c.ClassName).ToList();
                ClassNames.Insert(0, "All students");

                RawStudents = Repo.Users.OfType<Student>().Select(s => new Checkable<Student>(s)).ToList();
                Students = RawStudents.ToList();
                FilteredStudents = new ObservableCollection<Checkable<Student>>(Students);
            }

            InitializeComponent();
        }


        public void UpdateFilter()
        {
            if (!Dispatcher.CheckAccess())
                Dispatcher.Invoke((Action)UpdateFilter);
            else
            {
                string Filter = Text_StudentFilter.Text;
                string FilterType = FilterValues[Combo_FilterType.SelectedIndex];

                IEnumerable<Checkable<Student>> Filtered = Students.Where(s => Filters[FilterType](s, Filter));

                FilteredStudents.Clear();
                foreach (Checkable<Student> s in Filtered)
                    FilteredStudents.Add(s);
            }
        }

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
            Students.Clear();
            if (Combo_Classes.SelectedIndex == 0) // First item is always "No class"
            {
                foreach (Checkable<Student> s in RawStudents)
                    Students.Add(s);
            }
            else
            {
                Class Class = Classes.Single(c => c.ClassName == (string)Combo_Classes.SelectedItem);
                foreach (Checkable<Student> s in RawStudents.Where(s => Class.Students.Contains(s.Value)))
                    Students.Add(s);
            }
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
