using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.ComponentModel;

using Data;
using Data.Models;

namespace Client.TimetableDisplay
{
    // Delegate function signature for the event indicating
    // a tile has been clicked
    public delegate void TileClickHandler(TimetableTile Tile);

    // Shows a table of Rooms and Timeslots where each cell represents a Booking
    public partial class TimetableDisplay
        : UserControl, INotifyPropertyChanged
    {
        // The internal array of Tiles used
        protected TimetableTile[,] Tiles { get; set; }

        // The dimensions of a single tile
        protected float TileWidth = 100;
        protected float TileHeight = 100;

        // The dimensions of the top/left headings of the table
        protected float LeftWidth = 75;
        protected float TopHeight = 75;

        // Brush used to colour the background of the headings
        protected Brush MarginBrush = Brushes.LightGray;

        // Event indicating a tile has been clicked
        public event TileClickHandler TileClicked;

        public TimetableDisplay()
        {
            InitializeComponent();

            PropertyChanged = delegate { };
            TileClicked = delegate { };

            DataContext = this;
        }

        // Organises the table for a given user on a particular day
        public void SetTimetable(User CurrentUser, DateTime Day)
        {
            // Get the current database state
            DataSnapshot Frame = DataRepository.TakeSnapshot();
            
            // Generate the correct number of rows
            Container.RowDefinitions.Clear();
            // Top header row
            Container.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) });
            // Actual rows of the table
            for (int y = 0; y < Frame.Rooms.Count; y++)
                Container.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(TileHeight) });

            // Generate the columns
            Container.ColumnDefinitions.Clear();
            // Left-hand side-heading column
            Container.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(LeftWidth) });
            // Actual columns in the table
            for (int x = 0; x < Frame.Periods.Count; x++)
                Container.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(TileWidth) });


            // Add the left-hand bar, contains room names and a tooltip
            Container.Children.Clear();
            for (int y = 0; y < Frame.Rooms.Count; y++)
            {
                // Create a textblock displaying the room name
                TextBlock Child = new TextBlock();
                Child.Text = Frame.Rooms[y].RoomName;
                // Set standard font, margin, wrapping style etc
                Child.FontSize = 16;
                Child.Margin = new Thickness(0, 0, 5, 0);
                Child.TextWrapping = TextWrapping.Wrap;
                Child.VerticalAlignment = VerticalAlignment.Center;
                Child.HorizontalAlignment = HorizontalAlignment.Right;

                // Create the alignment control for nice layout
                Border LeftTile = new Border();
                // Set tooltip to useful information
                LeftTile.ToolTip = "Standard Seats: " + Frame.Rooms[y].StandardSeats + (Frame.Rooms[y].SpecialSeats == 0 ? "" : "\n" + Frame.Rooms[y].SpecialSeatType + ": " + Frame.Rooms[y].SpecialSeats);
                // Set the UI child of this control to be the texblock above
                LeftTile.Child = Child;
                // Background colour
                LeftTile.Background = MarginBrush;

                // Positioning on the layout grid
                LeftTile.SetValue(Grid.RowProperty, y + 1);
                LeftTile.SetValue(Grid.ColumnProperty, 0);

                // Add the controls to the grid
                Container.Children.Add(LeftTile);
            }

            // Add the top heading, contains timeslot name and time interval
            for (int x = -1; x < Frame.Periods.Count; x++)
            {
                // Use a grid for ease of layout
                Grid TopTile = new Grid();
                TopTile.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) });
                TopTile.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) });

                // Set background and child alignments
                TopTile.Background = MarginBrush;
                TopTile.VerticalAlignment = VerticalAlignment.Bottom;
                TopTile.HorizontalAlignment = HorizontalAlignment.Left;

                // If we're not filling out the top-left corner cell, store the timeslot name
                string Text = "";
                if (x >= 0 && !string.IsNullOrWhiteSpace(Frame.Periods[x].Name))
                    Text = Frame.Periods[x].Name;

                // First textblock - timeslot name
                TopTile.Children.Add(new TextBlock() { Text = Text, FontSize = 16, Margin = new Thickness(2, 2, 2, 0), TextWrapping = TextWrapping.Wrap, Width = TileWidth });
                // Second textblock - timeslot duration
                TopTile.Children.Add(new TextBlock() { Text = x >= 0 ? Frame.Periods[x].TimeRange : "", FontSize = 16, Margin = new Thickness(2, 0, 2, 10), TextWrapping = TextWrapping.Wrap, Width = TileWidth });

                // Set the position of each textblock within the local grid
                for (int y = 0; y < TopTile.Children.Count; y++)
                    TopTile.Children[y].SetValue(Grid.RowProperty, y);

                // Algin the local grid within the table's grid
                TopTile.SetValue(Grid.RowProperty, 0);
                TopTile.SetValue(Grid.ColumnProperty, x + 1);
                Container.Children.Add(TopTile);
            }

            // Add the main content
            // Find bookings on this day
            List<Booking> RelevantBookings = Frame.Bookings.Where(b => b.MatchesDay(Day)).ToList();
            // Initialise the internal array of tiles
            Tiles = new TimetableTile[Frame.Rooms.Count, Frame.Periods.Count];
            for (int y = 0; y < Frame.Rooms.Count; y++)
            {
                for (int x = 0; x < Frame.Periods.Count; x++)
                {
                    // Get the booking (or null) for this combination of room and timeslot
                    Booking Current = RelevantBookings.Where(b => b.TimeSlot == Frame.Periods[x] && b.Rooms.Contains(Frame.Rooms[y])).SingleOrDefault();

                    // Create the timetable tile
                    Tiles[y, x] = new TimetableTile(Current, Frame.Periods[x], Frame.Rooms[y], CurrentUser);

                    // Layout
                    Container.Children.Add(Tiles[y, x]);
                    Tiles[y, x].SetValue(Grid.RowProperty, y + 1);
                    Tiles[y, x].SetValue(Grid.ColumnProperty, x + 1);

                    // Hook up the tile clicked handler
                    Tiles[y, x].MouseLeftButtonDown += (o, e) => TileClicked((TimetableTile)o);
                }
            }
        }

        // Standard INotifyPropertyChanged interface implementation, for UI bindings
        protected void OnPropertyChanged(string PropertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(PropertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}