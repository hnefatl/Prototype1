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

            DataRepository.DataChanged += DataRepository_DataChanged;

            using (DataRepository Repo = new DataRepository())
            {
                Rooms = new ObservableCollection<Room>(Repo.Rooms);
            }
        }

        private void DataRepository_DataChanged(List<DataModel> OldItems, List<DataModel> NewItems)
        {
            // Need to find out which Data type has been changed
            IEnumerable<DataModel> Temp = OldItems.Union(NewItems);
            if (Temp.Count() > 0)
            {
                Type t = Temp.First().GetType();
                using (DataRepository Repo = new DataRepository())
                {
                    if (t == typeof(Room))
                        Rooms = new ObservableCollection<Room>(Repo.Rooms);
                }
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

        private void Button_AddRoom_Click(object sender, RoutedEventArgs e)
        {
            Room New = EditRoom("Add Room", new Room());

            bool Existing = false;
            using (DataRepository Repo = new DataRepository())
            {
                if (New.Conflicts(Repo.Rooms.Cast<DataModel>().ToList()))
                    Existing = true;
                else
                    Repo.Rooms.Add(New);
            }
            if (Existing)
                MessageBox.Show("Another room with that name already exists.");
        }
        private void Button_EditRoom_Click(object sender, RoutedEventArgs e)
        {
            Room New = EditRoom("Edit Room", (Room)List_Rooms.SelectedItem);

            using (DataRepository Repo = new DataRepository())
            {
                Repo.Rooms.Add(New);
            }
        }
        private Room EditRoom(string WindowTitle, Room r)
        {
            Dictionary<string, EditItem> Fields = new Dictionary<string, EditItem>() {
                { "RoomName", new EditItem("Room Name", r.RoomName, o => (o is string && !string.IsNullOrWhiteSpace(o as string) ? null : "Room Name must be entered")) },
                { "StandardSeats", new EditItem("Standard Seats", r.StandardSeats, EditItem.NonNegativeIntegerValidator) },
                { "SpecialSeats", new EditItem("Special Seats", r.SpecialSeats, EditItem.NonNegativeIntegerValidator) },
                { "SpecialSeatType", new EditItem("Special Seat Type", r.SpecialSeatType) },
            };

            Dictionary<string, object> Results = EditWindow.Show(WindowTitle, Fields);
            return new Room { Id = r.Id, Bookings = r.Bookings, RoomName = (string)Results["RoomName"], StandardSeats = Convert.ToInt32(Results["StandardSeats"]), SpecialSeats = Convert.ToInt32(Results["SpecialSeats"]), SpecialSeatType = (string)Results["SpecialSeatType"] };
        }
    }
}
