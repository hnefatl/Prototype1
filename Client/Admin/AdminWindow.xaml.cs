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
using System.ComponentModel;
using System.Collections.ObjectModel;

using Data.Models;

namespace Client.Admin
{
    public partial class AdminWindow
        : Window, INotifyPropertyChanged
    {
        protected ObservableCollection<Checkable<Room>> _Rooms = new ObservableCollection<Checkable<Room>>();
        public ObservableCollection<Checkable<Room>> Rooms
        {
            get { return _Rooms; }
            set { _Rooms = value; OnPropertyChanged("Rooms"); }
        }
        public List<Room> SelectedRooms { get { return Rooms.Where(c => c.Checked).Select(c => c.Value).ToList(); } }

        public AdminWindow()
        {
            InitializeComponent();

            using (DataRepository Repo = new DataRepository())
            {

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
