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

        public int ClassId { get; set; }

        public EditClass(Class Existing)
        {
            using (DataRepository Repo = new DataRepository())
            {
                Teachers = Repo.Users.OfType<Teacher>().Select(t => t.InformalName).ToArray();
            }

            InitializeComponent();
        }

        public override Class GetItem()
        {
            Class New = new Class();

            try
            {

            }
            catch
            {
                return null;
            }

            return New;
        }
    }
}
