using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

using Shared;

namespace Data.Models
{
    // A timeslot is a school period - a start and end time of a lesson
    [Table("Periods")]
    public class TimeSlot
        : DataModel, INotifyPropertyChanged
    {
        private TimeSpan _Start;
        // The Start time of the period
        public TimeSpan Start
        {
            get { return _Start; }
            set { _Start = value; OnPropertyChanged("Start"); }
        }
        [NotMapped] // Start time in short time format (12:10), used for UI
        public string ShortStart { get { return new DateTime(Start.Ticks).ToShortTimeString(); } }

        private TimeSpan _End;
        // The End time of the period
        public TimeSpan End
        {
            get { return _End; }
            set { _End = value; OnPropertyChanged("End"); }
        }
        [NotMapped] // End time in short time format(13:10), used in UI
        public string ShortEnd { get { return new DateTime(End.Ticks).ToShortTimeString(); } }

        // Name of the period
        public string Name { get; set; }

        [NotMapped] // Utility property for UI - range of times
        public string TimeRange
        {
            get { return ShortStart + " - " + ShortEnd; }
        }

        // Bookings using this period
        public virtual List<Booking> Bookings { get; set; }

        public TimeSlot()
            : this(new TimeSpan(0, 0, 0), new TimeSpan(0, 0, 0))
        {
        }
        public TimeSlot(TimeSpan Start, TimeSpan End)
        {
            PropertyChanged = delegate { };

            this.Start = Start;
            this.End = End;

            Bookings = new List<Booking>();

            Name = string.Empty;
        }

        // Returns if the provided Time's time is in this period
        public bool IsCurrent(DateTime Time)
        {
            TimeSpan Mod = Time - Time.Date;
            return Start <= Mod && End >= Mod;
        }

        public override bool Conflicts(List<DataModel> Others)
        {
            return Others.Cast<TimeSlot>().Any(t => t.Id != Id && t.Name == Name || (t.Start == Start && t.End == End));
        }

        public override void Update(DataModel Other)
        {
            TimeSlot t = (TimeSlot)Other;

            Start = t.Start;
            End = t.End;
            Name = t.Name;
            Bookings.Clear();
            Bookings.AddRange(t.Bookings);
        }

        // Serialise properties and IDs
        public override void Serialise(Writer Out)
        {
            base.Serialise(Out);

            Out.Write(_Start.Ticks);
            Out.Write(_End.Ticks);
            Out.Write(Name);

            Out.Write(Bookings.Count);
            Bookings.ForEach(b => Out.Write(b.Id));
        }
        // Deserialise properties and IDs
        protected override void Deserialise(Reader In)
        {
            base.Deserialise(In);

            _Start = new TimeSpan(In.ReadInt64());
            _End = new TimeSpan(In.ReadInt64());
            Name = In.ReadString();

            Bookings = Enumerable.Repeat(new Booking(), In.ReadInt32()).ToList();
            Bookings.ForEach(b => b.Id = In.ReadInt32());
        }
        // Obtain references to other items
        public override bool Expand(IDataRepository Repo)
        {
            try
            {
                for (int x = 0; x < Bookings.Count; x++)
                    Bookings[x] = Repo.Bookings.SingleOrDefault(b => b.Id == Bookings[x].Id);
            }
            catch
            {
                return false;
            }
            return true;
        }
        // Add references to this to the related items
        public override void Attach()
        {
            Bookings.ForEach(b => b.TimeSlot = this);
        }
        // Remove references to this from the related items
        public override void Detach()
        {
            Bookings.ForEach(b => { if (b != null) b.TimeSlot = null; });
        }

        public override string ToString()
        {
            if (!string.IsNullOrWhiteSpace(Name))
                return Name;
            else
                return TimeRange;
        }

        public static bool operator ==(TimeSlot One, TimeSlot Two)
        {
            // If both object references are actually the same object, return true
            if (ReferenceEquals(One, Two))
                return true;

            // Equal if the name, start and end times all match for two non-null objects
            return (object)One != null && (object)Two != null && One.Start == Two.Start && One.End == Two.End && One.Name == Two.Name;
        }
        public static bool operator !=(TimeSlot One, TimeSlot Two)
        {
            // Required overload of !=, just invert the already overriden == operator
            return !(One == Two);
        }

        public override bool Equals(object obj)
        {
            // Required overload, check for null then do a standard equality check
            TimeSlot Obj = obj as TimeSlot;
            if (Obj == null)
                return false;
            return this == Obj;
        }
        public override int GetHashCode()
        {
            // Required overload, just perform the base function
            return base.GetHashCode();
        }

        // Utility function to fire the PropertyChanged event using less code
        protected void OnPropertyChanged(string PropertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(PropertyName));
        }
        // Event to be fired on a property changing - used for UI responsiveness
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
