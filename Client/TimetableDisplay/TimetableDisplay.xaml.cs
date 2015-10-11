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
using System.ComponentModel;
using Data;
using Data.Models;

namespace Client.TimetableDisplay
{
    public delegate void TileClickHandler(TimetableTile Tile);
    public partial class TimetableDisplay
        : UserControl, INotifyPropertyChanged
    {
        protected TimetableTile[,] Tiles { get; set; }

        protected float TileWidth = 100;
        protected float TileHeight = 100;
        protected float LeftWidth = 75;
        protected float TopHeight = 75;

        protected Brush MarginBrush = Brushes.LightGray;

        public event TileClickHandler TileClicked;

        public TimetableDisplay()
        {
            InitializeComponent();

            PropertyChanged = delegate { };
            TileClicked = delegate { };

            DataContext = this;
        }

        public void SetTimetable(User CurrentUser, DateTime Day)
        {
            DataSnapshot Frame = DataRepository.TakeSnapshot();

            Width = TileWidth * Frame.Periods.Count + LeftWidth;
            Height = TileHeight * Frame.Rooms.Count + TopHeight;

            Container.RowDefinitions.Clear();
            Container.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(TopHeight) });
            for (int y = 0; y < Frame.Rooms.Count; y++)
                Container.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(TileHeight) });

            Container.ColumnDefinitions.Clear();
            Container.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(LeftWidth) });
            for (int x = 0; x < Frame.Periods.Count; x++)
                Container.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(TileWidth) });


            // Add the top and left rows (headings and row names)
            Container.Children.Clear();
            for (int y = 0; y < Frame.Rooms.Count; y++)
            {
                TextBlock Child = new TextBlock();
                Child.Text = Frame.Rooms[y].RoomName;
                Child.FontSize = 16;
                Child.Margin = new Thickness(0, 0, 5, 0);
                Child.TextWrapping = TextWrapping.Wrap;
                Child.VerticalAlignment = VerticalAlignment.Center;
                Child.HorizontalAlignment = HorizontalAlignment.Right;

                Border LeftTile = new Border();
                LeftTile.Child = Child;
                LeftTile.Background = MarginBrush;
                LeftTile.SetValue(Grid.RowProperty, y + 1);
                LeftTile.SetValue(Grid.ColumnProperty, 0);
                Container.Children.Add(LeftTile);
            }

            for (int x = -1; x < Frame.Periods.Count; x++)
            {
                Grid TopTile = new Grid();
                TopTile.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) });
                TopTile.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) });

                TopTile.Background = MarginBrush;
                TopTile.VerticalAlignment = VerticalAlignment.Bottom;
                TopTile.HorizontalAlignment = HorizontalAlignment.Left;

                string Text = "";
                if (x >= 0 && !string.IsNullOrWhiteSpace(Frame.Periods[x].Name))
                    Text = Frame.Periods[x].Name;

                TopTile.Children.Add(new TextBlock() { Text = Text, FontSize = 16, Margin = new Thickness(2, 2, 2, 0), TextWrapping = TextWrapping.Wrap, Width = TileWidth });
                TopTile.Children.Add(new TextBlock()
                {
                    Text = x >= 0 ? Frame.Periods[x].TimeRange : "",
                    FontSize = 16,
                    Margin = new Thickness(2, 0, 2, 10),
                    TextWrapping = TextWrapping.Wrap,
                    Width = TileWidth
                });

                for (int y = 0; y < TopTile.Children.Count; y++)
                    TopTile.Children[y].SetValue(Grid.RowProperty, y);

                TopTile.SetValue(Grid.RowProperty, 0);
                TopTile.SetValue(Grid.ColumnProperty, x + 1);
                Container.Children.Add(TopTile);
            }

            // Add the main content
            List<Booking> RelevantBookings = Frame.Bookings.Where(b => b.MatchesDay(Day)).ToList();
            Tiles = new TimetableTile[Frame.Rooms.Count, Frame.Periods.Count];
            for (int y = 0; y < Frame.Rooms.Count; y++)
            {
                for (int x = 0; x < Frame.Periods.Count; x++)
                {
                    Booking Current = RelevantBookings.Where(b => b.TimeSlot == Frame.Periods[x] && b.Rooms.Contains(Frame.Rooms[y])).SingleOrDefault();

                    Tiles[y, x] = new TimetableTile();
                    Tiles[y, x].Booking = Current;
                    Tiles[y, x].Room = Frame.Rooms[y];
                    Tiles[y, x].Time = Frame.Periods[x];

                    Container.Children.Add(Tiles[y, x]);
                    Tiles[y, x].SetValue(Grid.RowProperty, y + 1); // Set y
                    Tiles[y, x].SetValue(Grid.ColumnProperty, x + 1); // Set x
                    Tiles[y, x].MouseLeftButtonDown += (o, e) => TileClicked((TimetableTile)o);
                    Tiles[y, x].HorizontalAlignment = HorizontalAlignment.Stretch;
                    Tiles[y, x].VerticalAlignment = VerticalAlignment.Stretch;

                    if (Current != null)
                    {
                        Tiles[y, x].Children.Add(new TextBlock() { Text = Current.Subject.SubjectName, FontSize = 16, Margin = new Thickness(5, 5, 5, 0), TextWrapping = TextWrapping.NoWrap, TextTrimming = TextTrimming.CharacterEllipsis });
                        Tiles[y, x].Children.Add(new TextBlock() { Text = Current.Teacher.Title + " " + Current.Teacher.LastName, FontSize = 16, Margin = new Thickness(5, 5, 5, 0), TextWrapping = TextWrapping.NoWrap, TextTrimming = TextTrimming.CharacterEllipsis });

                        // TODO: Still need some sort of marker to show which lessons the student's involved in
                        //if (CurrentUser is Student)
                        //    if (Current.Students.Contains(CurrentUser))
                        //        Tiles[y, x].Children.Add(new TextBlock() { Text = "Involved", FontSize = 16, Margin = new Thickness(5, 5, 5, 0), VerticalAlignment = VerticalAlignment.Bottom, TextWrapping = TextWrapping.NoWrap, TextTrimming = TextTrimming.CharacterEllipsis });

                        Tiles[y, x].Brush = new SolidColorBrush(Current.Subject.Colour);
                    }
                    else
                    {
                        Border AlignmentHelp = new Border();
                        AlignmentHelp.Height = TileHeight;
                        AlignmentHelp.Child = new TextBlock() { Text = "Empty", FontSize = 16, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };

                        Tiles[y, x].Brush = SystemColors.WindowBrush;
                        Tiles[y, x].Children.Add(AlignmentHelp);
                    }
                }
            }
        }

        protected void OnPropertyChanged(string PropertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(PropertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}