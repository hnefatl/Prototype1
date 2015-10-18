using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Entity;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Shared;

namespace Data.Models
{
    /// <summary>
    /// Used to tie a start/end time together for neatness.
    /// The table of all established "periods", eg period 1-5, break, lunch. Bookings can be made outside of periods, they just need to be
    /// manually created when making a booking and are only shown when relevant.
    /// </summary>
    [Table("Periods")]
    public class TimeSlot
        : DataModel, INotifyPropertyChanged
    {
        private TimeSpan _Start;
        public TimeSpan Start
        {
            get
            {
                return _Start;
            }
            set
            {
                _Start = value;
                OnPropertyChanged("Start");
            }
        }
        private TimeSpan _End;
        public TimeSpan End
        {
            get
            {
                return _End;
            }
            set
            {
                _End = value;
                OnPropertyChanged("End");
            }
        }

        /// <summary>
        /// Should be null if not a period, else have the period name
        /// </summary>
        public string Name { get; set; }

        [NotMapped]
        public string TimeRange
        {
            get
            {
                return new DateTime(Start.Ticks).ToShortTimeString() + " - " + new DateTime(End.Ticks).ToShortTimeString();
            }
        }

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

        public override void Serialise(IWriter Out)
        {
            base.Serialise(Out);

            Out.Write(_Start.Ticks);
            Out.Write(_End.Ticks);
            Out.Write(Name);

            Out.Write(Bookings.Count);
            Bookings.ForEach(b => Out.Write(b.Id));
        }
        protected override void Deserialise(IReader In)
        {
            base.Deserialise(In);

            _Start = new TimeSpan(In.ReadInt64());
            _End = new TimeSpan(In.ReadInt64());
            Name = In.ReadString();

            Bookings = Enumerable.Repeat(new Booking(), In.ReadInt32()).ToList();
            Bookings.ForEach(b => b.Id = In.ReadInt32());
        }
        public override bool Expand(IDataRepository Repo)
        {
            try
            {
                Bookings.ForEach(b => b = Repo.Bookings.Where(b2 => b2.Id == b.Id).Single());
            }
            catch
            {
                return false;
            }
            return true;
        }

        public override string ToString()
        {
            if (!string.IsNullOrWhiteSpace(Name))
                return Name;
            else
                return TimeRange;
        }

        public static bool operator==(TimeSlot One, TimeSlot Two)
        {
            if (ReferenceEquals(One, Two))
                return true;

            return (object)One != null && (object)Two != null && One.Start == Two.Start && One.End == Two.End && One.Name == Two.Name;
        }
        public static bool operator!=(TimeSlot One, TimeSlot Two)
        {
            return !(One == Two);
        }

        public override bool Equals(object obj)
        {
            TimeSlot Obj = obj as TimeSlot;
            if (Obj == null)
                return false;
            return this == Obj;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        protected void OnPropertyChanged(string PropertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(PropertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
