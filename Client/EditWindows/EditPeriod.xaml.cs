using System;
using System.Linq;
using System.Windows;

using Data.Models;

namespace Client.EditWindows
{
    public partial class EditPeriod
        : EditWindow<TimeSlot>
    {
        // Name of the period being edited
        protected string _PeriodName;
        public string PeriodName
        {
            get { return _PeriodName; }
            set { _PeriodName = value; OnPropertyChanged("PeriodName"); }
        }

        // String representation of the start time
        protected string _Start;
        public string Start
        {
            get { return _Start; }
            set { _Start = value; OnPropertyChanged("Start"); }
        }

        // String representation of the end time
        protected string _End;
        public string End
        {
            get { return _End; }
            set { _End = value; OnPropertyChanged("End"); }
        }

        // ID of the period being edited
        public int PeriodId { get; set; }

        public EditPeriod(TimeSlot Existing)
        {
            InitializeComponent();

            // Initialise with new/existing data
            if (Existing == null)
            {
                PeriodName = string.Empty;
                Start = string.Empty;
                End = string.Empty;
                PeriodId = 0;
            }
            else
            {
                PeriodName = Existing.Name;
                // Convert the TimeSpan objects into hh:mm format by padding and concatenating srings
                Start = Convert.ToString(Existing.Start.Hours).PadLeft(2, '0') + ":" + Convert.ToString(Existing.Start.Minutes).PadLeft(2, '0');
                End = Convert.ToString(Existing.End.Hours).PadLeft(2, '0') + ":" + Convert.ToString(Existing.End.Minutes).PadLeft(2, '0');

                PeriodId = Existing.Id;
            }
        }

        public override TimeSlot GetItem()
        {
            TimeSlot New = new TimeSlot();
            try
            {
                // Fill out the fields, parsing data
                New.Name = PeriodName;
                New.Start = TimeSpan.Parse(Start);
                New.End = TimeSpan.Parse(End);
                New.Id = PeriodId;
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
                // Validation
                TimeSpan StartOut;
                TimeSpan EndOut;
                if (string.IsNullOrWhiteSpace(PeriodName))
                    Error = "You must enter a period name.";
                else if (string.IsNullOrWhiteSpace(Start) || !TimeSpan.TryParse(Start, out StartOut) || !CompatibleTime(StartOut))
                    Error = "Invalid start time format. Must be in the format \"hh:mm\"";
                else if (string.IsNullOrWhiteSpace(End) || !TimeSpan.TryParse(End, out EndOut) || !CompatibleTime(EndOut))
                    Error = "Invalid end time format. Must be in the format \"hh:mm\"";
                else if (StartOut > EndOut)
                    Error = "Start time must be before End time.";
                else if (Repo.Periods.Any(p => p.Name == PeriodName))
                    Error = "Another Period with that name already exists.";
            }

            // Error message or close window
            if (!string.IsNullOrWhiteSpace(Error))
                MessageBox.Show(Error, "Error", MessageBoxButton.OK);
            else
            {
                DialogResult = true;
                Close();
            }
        }

        // Simple validation function that checks the timespan is represents a valid time
        private bool CompatibleTime(TimeSpan t)
        {
            return t.Hours < 24 && t.Hours >= 0 && t.Minutes >= 0 && t.Seconds >= 0;
        }
    }
}
