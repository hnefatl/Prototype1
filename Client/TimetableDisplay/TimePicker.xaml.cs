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
using System.Text.RegularExpressions;

namespace Client.TimetableDisplay
{
    public enum TimePickerType
        : byte
    {
        None = 0,
        Hours = 1,
        Minutes = 2,
        Seconds = 4,

        HoursMinutes = Hours | Minutes,
        HoursMinutesSeconds = Hours | Minutes | Seconds,
        HoursSeconds = Hours | Seconds,
        MinutesSeconds = Minutes | Seconds,
    }

    public partial class TimePicker
        : UserControl, INotifyPropertyChanged
    {
        public int Hours
        {
            get
            {
                return Time.Hours;
            }
            set
            {
                Time = Time.Add(new TimeSpan(value, 0, 0));
            }
        }
        public int Minutes
        {
            get
            {
                return Time.Minutes;
            }
            set
            {
                Time = Time.Add(new TimeSpan(0, value, 0));
            }
        }
        public int Seconds
        {
            get
            {
                return Time.Seconds;
            }
            set
            {
                Time = Time.Add(new TimeSpan(0, 0, value));
            }
        }

        public TimeSpan Time
        {
            get
            {
                return (TimeSpan)GetValue(TimeProperty);
            }
            set
            {
                SetValue(TimeProperty, value);
            }
        }

        public static readonly DependencyProperty TimeProperty = DependencyProperty.Register("Time", typeof(TimeSpan), typeof(TimePicker),
            new FrameworkPropertyMetadata(OnTimePropertyChanged) { BindsTwoWayByDefault = true });
        private static void OnTimePropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            TimePicker Sender = sender as TimePicker;
            if (Sender != null)
            {
                Sender.OnPropertyChanged("Time");
                Sender.OnPropertyChanged("Hours");
                Sender.OnPropertyChanged("Minutes");
                Sender.OnPropertyChanged("Seconds");
            }
        }

        public TimePickerType TimePickerType
        {
            get
            {
                return (Text_Hours.Visibility == Visibility.Visible ? TimePickerType.Hours : TimePickerType.None) |
                       (Text_Minutes.Visibility == Visibility.Visible ? TimePickerType.Minutes : TimePickerType.None) |
                       (Text_Seconds.Visibility == Visibility.Visible ? TimePickerType.Seconds : TimePickerType.None);
            }
            set
            {
                Text_Hours.Visibility = ((value & TimePickerType.Hours) == TimePickerType.Hours) ? Visibility.Visible : Visibility.Collapsed;
                Text_Minutes.Visibility = ((value & TimePickerType.Minutes) == TimePickerType.Minutes) ? Visibility.Visible : Visibility.Collapsed;
                Text_Seconds.Visibility = ((value & TimePickerType.Seconds) == TimePickerType.Seconds) ? Visibility.Visible : Visibility.Collapsed;

                if (Text_Hours.Visibility == Visibility.Visible && (Text_Minutes.Visibility == Visibility.Visible || Text_Seconds.Visibility == Visibility.Visible))
                    Colon1.Visibility = Visibility.Visible;
                else
                    Colon1.Visibility = Visibility.Collapsed;

                if (Text_Minutes.Visibility == Visibility.Visible && Text_Seconds.Visibility == Visibility.Visible)
                    Colon2.Visibility = Visibility.Visible;
                else
                    Colon2.Visibility = Visibility.Collapsed;
            }
        }

        public TimePicker()
        {
            InitializeComponent();
            TimePickerType = TimePickerType.HoursMinutesSeconds;

            PropertyChanged = delegate { };
        }

        private void Text_Hours_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
                Hours++;
            else if (e.Key == Key.Down)
                Hours--;
            else if (e.Key == Key.Space)
                e.Handled = true;
        }
        private void Text_Minutes_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
                Minutes++;
            else if (e.Key == Key.Down)
                Minutes++;
            else if (e.Key == Key.Space)
                e.Handled = true;
        }
        private void Text_Seconds_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
                Seconds++;
            else if (e.Key == Key.Down)
                Seconds--;
            else if (e.Key == Key.Space)
                e.Handled = true;
        }

        private void Text_Hours_TextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox Sender = (TextBox)e.OriginalSource;
            string Contents = Sender.Text.Remove(Sender.SelectionStart, Sender.SelectionLength).Insert(Sender.CaretIndex, e.Text);
            int Out;
            if (!int.TryParse(Contents, out Out) || Out < 0 || Out > 23)
                e.Handled = true;
        }
        private void Text_Minutes_TextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox Sender = (TextBox)e.OriginalSource;
            string Contents = Sender.Text.Remove(Sender.SelectionStart, Sender.SelectionLength).Insert(Sender.CaretIndex, e.Text);
            int Out;
            if (!int.TryParse(Contents, out Out) || Out < 0 || Out > 59)
                e.Handled = true;
        }
        private void Text_Seconds_TextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox Sender = (TextBox)e.OriginalSource;
            string Contents = Sender.Text.Remove(Sender.SelectionStart, Sender.SelectionLength).Insert(Sender.CaretIndex, e.Text);
            int Out;
            if (!int.TryParse(Contents, out Out) || Out < 0 || Out > 59)
                e.Handled = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string PropertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(PropertyName));
        }
    }
}