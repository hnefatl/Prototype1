using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Shared;

namespace Data.Models
{
    /// <summary>
    /// A Booking is a effectively a single lesson.
    /// </summary>
    [Table("Bookings")]
    public class Booking
        : DataModel
    {
        /// <summary>
        /// The Date of the booking (the time part is irrelevant)
        /// </summary>
        [NotMapped]
        public DateTime Date
        {
            get
            {
                return DateTime.FromBinary(Ticks).Date;
            }
            set
            {
                Ticks = value.Date.ToBinary();
            }
        }
        public long Ticks { get; set; }

        public BookingType BookingType { get; set; }

        /// <summary>
        /// The duration of the booking (used to work out which period for display purposes)
        /// </summary>
        public virtual TimeSlot TimeSlot { get; set; }

        /// <summary>
        /// Rooms used by this booking
        /// </summary>
        public virtual List<Room> Rooms { get; set; }
        /// <summary>
        /// The subject of this booking
        /// </summary>
        public virtual Subject Subject { get; set; }

        public virtual List<Student> Students { get; set; }
        public virtual Teacher Teacher { get; set; }

        public Booking()
            : this(null, new List<Room>(), null, new List<Student>(), null, BookingType.Single)
        {
        }
        public Booking(TimeSlot Time, List<Room> Rooms, Subject Subject, List<Student> Students, Teacher Teacher, BookingType BookingType)
        {
            TimeSlot = Time;
            this.Rooms = Rooms;
            this.Subject = Subject;
            this.Students = Students;
            this.Teacher = Teacher;
            this.BookingType = BookingType;
        }

        public bool MatchesDay(DateTime Day)
        {
            return (BookingType == BookingType.Single && Day.Date == Date) ||
                    (BookingType == BookingType.Weekly && (Day.Date - Date).Days % 7 == 0) ||
                    (BookingType == BookingType.Fortnightly && (Day.Date - Date).Days % 14 == 0) ||
                    (BookingType == BookingType.Monthly && (Day.Date - Date).Days % 31 == 0);
        }
        public override bool Conflicts(List<DataModel> AllBookings)
        {
            return AllBookings.Cast<Booking>().Any(b => b.Id != Id && b.Date == Date && b.TimeSlot == TimeSlot && b.Rooms.Intersect(Rooms).Count() != 0);
        }

        public override void Update(DataModel Other)
        {
            Booking b = (Booking)Other;
            Ticks = b.Ticks;
            BookingType = b.BookingType;
            TimeSlot = b.TimeSlot;
            Rooms = b.Rooms;
            Subject = b.Subject;
            Students = b.Students;
            Teacher = b.Teacher;
        }

        public override void Serialise(IWriter Out)
        {
            base.Serialise(Out);

            Out.Write(Ticks);
            Out.Write((int)BookingType);
            Out.Write(TimeSlot.Id);
            Out.Write(Rooms.Count);
            foreach (Room r in Rooms)
                Out.Write(r.Id);
            Out.Write(Subject.Id);
            Out.Write(Students.Count);
            foreach (Student s in Students)
                Out.Write(s.Id);
            Out.Write(Teacher.Id);
        }
        protected override void Deserialise(IReader In)
        {
            base.Deserialise(In);

            Ticks = In.ReadInt64();
            BookingType = (BookingType)In.ReadInt32();
            TimeSlot.Id = In.ReadInt32();
            Rooms = new List<Room>(In.ReadInt32());
            for (int x = 0; x < Rooms.Capacity; x++)
                Rooms.Add(new Room() { Id = In.ReadInt32() });
            Subject.Id = In.ReadInt32();
            Students = new List<Student>(In.ReadInt32());
            for (int x = 0; x < Students.Capacity; x++)
                Students.Add(new Student() { Id = In.ReadInt32() });
            Teacher.Id = In.ReadInt32();
        }
        public override bool Expand(IDataRepository Repo)
        {
            try
            {
                TimeSlot = Repo.Periods.Where(t => t.Id == TimeSlot.Id).Single();
                for (int x = 0; x < Rooms.Count; x++)
                    Rooms[x] = Repo.Rooms.Where(r => Rooms[x].Id == r.Id).Single();
                Subject = Repo.Subjects.Where(s => s.Id == Subject.Id).Single();
                for (int x = 0; x < Students.Count; x++)
                    Students[x] = (Student)Repo.Users.Where(s => Students[x].Id == s.Id).Single();
                Teacher = (Teacher)Repo.Users.Where(t => t.Id == Teacher.Id).Single();
            }
            catch
            {
                return false;
            }
            return true;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Booking))
                return false;

            Booking b = (obj as Booking);
            return b == null || ReferenceEquals(this, obj) || (b.BookingType == BookingType && b.Date == Date && b.Id == Id &&
                b.Rooms == Rooms && b.Students == Students && b.Subject == Subject && b.Teacher == Teacher && b.Ticks == Ticks &&
                b.TimeSlot == TimeSlot);
        }
        public static bool operator ==(Booking One, Booking Two)
        {
            if (ReferenceEquals(One, null))
            {
                if (ReferenceEquals(Two, null))
                    return true;
                return false;
            }
            if (ReferenceEquals(Two, null))
                return false;

            return ReferenceEquals(One, Two) || (One.BookingType == Two.BookingType && One.Date == Two.Date && One.Id == Two.Id &&
                One.Rooms == Two.Rooms && One.Students == Two.Students && One.Subject == Two.Subject && One.Teacher == Two.Teacher && One.Ticks == Two.Ticks &&
                One.TimeSlot == Two.TimeSlot);
        }
        public static bool operator !=(Booking One, Booking Two)
        {
            return !(One == Two);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public enum BookingType
    {
        /// <summary>
        /// A one off booking
        /// </summary>
        Single,
        /// <summary>
        /// Occurs every 7 days
        /// </summary>
        Weekly,
        /// <summary>
        /// Occurs every 14 days
        /// </summary>
        Fortnightly,
        /// <summary>
        /// Occurs every 30 days
        /// </summary>
        Monthly,
    }
}