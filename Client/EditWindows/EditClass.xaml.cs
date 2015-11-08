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
using System.Collections.ObjectModel;
using System.ComponentModel;

using Data.Models;

namespace Client.EditWindows
{
    public partial class EditClass
        : EditWindow<Class>
    {
        protected string _ClassName;
        public string ClassName
        {
            get { return _ClassName; }
            set { _ClassName = value; OnPropertyChanged("ClassName"); }
        }

        protected string _Teacher;
        public string Teacher
        {
            get { return _Teacher; }
            set { _Teacher = value; OnPropertyChanged("Teacher"); }
        }
        public string[] Teachers { get; set; }
        public int[] TeacherIds { get; set; }

        public int ClassId { get; set; }

        public EditClass(Class Existing)
        {
            using (DataRepository Repo = new DataRepository())
            {
                IEnumerable<Teacher> ts = Repo.Users.OfType<Teacher>();
                Teachers = ts.Select(t => t.InformalName).ToArray();
                TeacherIds = ts.Select(t => t.Id).ToArray();
            }

            InitializeComponent();

            if (Existing == null)
            {
                ClassName = string.Empty;
                Teacher = string.Empty;
                ClassId = 0;
            }
            else
            {
                ClassName = Existing.ClassName;
                Teacher = Existing.Owner.InformalName;
                ClassId = Existing.Id;
            }

            if (Existing != null)
                Students.Students.Where(s => Existing.Students.Contains(s.Value)).ToList().ForEach(s => s.Checked = true);
        }

        public override Class GetItem()
        {
            Class New = new Class();

            try
            {
                New.Id = ClassId;
                New.ClassName = ClassName;

                using (DataRepository Repo = new DataRepository())
                {
                    New.Owner = Repo.Users.OfType<Teacher>().Single(t => t.Id == TeacherIds[Combo_Teacher.SelectedIndex]);
                    New.Students = Students.SelectedStudents;
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
            DialogResult = false;
            Close();
        }
        private void Button_Save_Click(object sender, RoutedEventArgs e)
        {
            string Error = null;

            using (DataRepository Repo = new DataRepository())
            {
                if (string.IsNullOrWhiteSpace(ClassName))
                    Error = "You must enter a class name.";
                else if (string.IsNullOrWhiteSpace(Teacher))
                    Error = "You must enter a Teacher.";
                else if (Repo.Classes.Any(c => c.Id != ClassId && c.ClassName == ClassName))
                    Error = "Another class with that name already exists.";
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
