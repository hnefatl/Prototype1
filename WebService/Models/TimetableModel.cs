using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Data.DataContext;

namespace Data.Models
{
    public class TimetableModel
    {
        public List<Room> Rooms { get; set; }
        public List<TimeSlot> Times { get; set; }

        public Booking[,] Bookings { get; set; } // Simple 2D grid for ease of interpretation on the View

        public DateTime Day { get; set; }

        public bool ValidTimetable { get; set; }

        public TimetableModel()
        {
            ValidTimetable = false;
        }
        public TimetableModel(List<Room> Rooms, List<TimeSlot> Times, Booking[,] Bookings, DateTime Day)
        {
            this.Rooms = Rooms;
            this.Times = Times;
            this.Bookings = Bookings;
            this.Day = Day;
            ValidTimetable = true;
        }
    }

    public class TimeSlot
    {
        public TimeSpan Start { get; set; }
        public TimeSpan End { get; set; }
        public string Name { get; set; }

        public TimeSlot()
        {
        }
        public TimeSlot(TimeSpan Start, TimeSpan End, string Name)
        {
            this.Start = Start;
            this.End = End;
            this.Name = Name;
        }

        public string ToShortTimeSpan()
        {
            return new DateTime(Start.Ticks).ToShortTimeString() + "-" + new DateTime(End.Ticks).ToShortTimeString();
        }
    }
}