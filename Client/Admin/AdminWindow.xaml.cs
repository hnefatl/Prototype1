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
using NetCore.Client;

namespace Client.Admin
{
    public partial class AdminWindow
        : Window, INotifyPropertyChanged
    {
        public Connection Connection { get; set; }
        public User CurrentUser { get; set; }

        protected ObservableCollection<Room> _Rooms = new ObservableCollection<Room>();
        public ObservableCollection<Room> Rooms
        {
            get { return _Rooms; }
            set { _Rooms = value; OnPropertyChanged("Rooms"); OnPropertyChanged("SelectedRooms"); OnPropertyChanged("SingleRoomSelected"); OnPropertyChanged("SelectedRoom"); }
        }
        public List<Room> SelectedRooms { get { return List_Rooms.SelectedItems.Cast<Room>().ToList(); } }
        public Room SelectedRoom { get { return (Room)List_Rooms.SelectedItem; } }
        public bool SingleRoomSelected { get { return SelectedRooms.Count == 1; } }

        public AdminWindow(Connection Connection, User CurrentUser)
        {
            InitializeComponent();

            this.Connection = Connection;
            this.CurrentUser = CurrentUser;

            using (DataRepository Repo = new DataRepository())
            {
                Rooms = new ObservableCollection<Room>(Repo.Rooms);
            }
        }

        private void Button_Save_Click(object sender, RoutedEventArgs e)
        {
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
