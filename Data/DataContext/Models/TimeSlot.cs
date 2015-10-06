﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Entity;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.DataContext.Models
{
    /// <summary>
    /// Used to tie a start/end time together for neatness.
    /// The table of all established "periods", eg period 1-5, break, lunch. Bookings can be made outside of periods, they just need to be
    /// manually created when making a booking and are only shown when relevant.
    /// </summary>
    [Table("Periods")]
    public class TimeSlot
        : INotifyPropertyChanged
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

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

        public TimeSlot()
            : this(new TimeSpan(0, 0, 0), new TimeSpan(0, 0, 0))
        {
        }
        public TimeSlot(TimeSpan Start, TimeSpan End)
        {
            PropertyChanged = delegate { };

            this.Start = Start;
            this.End = End;
        }

        public virtual List<Booking> Bookings { get; set; }

        public override string ToString()
        {
            if (!string.IsNullOrWhiteSpace(Name))
                return Name;
            else
                return TimeRange;
        }

        public static bool operator==(TimeSlot One, TimeSlot Two)
        {
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
