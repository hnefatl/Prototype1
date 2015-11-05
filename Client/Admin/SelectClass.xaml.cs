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

using Data;
using Data.Models;

namespace Client.Admin
{
    public partial class SelectClass
        : Window, INotifyPropertyChanged
    {
        protected ObservableCollection<Class> _Classes;
        public ObservableCollection<Class> Classes
        {
            get { return _Classes; }
            set { _Classes = value; OnPropertyChanged("Classes"); }
        }
        protected Class _SelectedClass;
        public Class SelectedClass
        {
            get { return _SelectedClass; }
            set { _SelectedClass = value; OnPropertyChanged("SelectedClass"); }
        }



        public SelectClass()
        {
            InitializeComponent();
        }



        public void OnPropertyChanged(string PropertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(PropertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
