using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.ComponentModel.DataAnnotations.Schema;

using Shared;

namespace Data.Models
{
    // Recurrence type of the booking
    public enum BookingType
    {
        // A one off booking
        Single,
        // Occurs every 7 days
        Weekly,
        // Occurs every 14 days
        Fortnightly,
        // Occurs every 4 weeks
        Monthly,
    }

    // A Booking is a effectively a single lesson.
    [Table("Bookings")]
    public class Booking
        : DataModel
    {
        // The Date of the booking (the time part is irrelevant)
        [NotMapped]
        public DateTime Date
        {
            get { return DateTime.FromBinary(Ticks).Date; }
            set { Ticks = value.Date.ToBinary(); }
        }
        // Representation of the Date as a primitive datatype for storage in the database
        public long Ticks { get; set; }

        public BookingType BookingType { get; set; }
        
        // The duration of the booking (used to work out which period for display purposes)
        public virtual TimeSlot TimeSlot { get; set; }
        
        // Rooms used by this booking
        public virtual List<Room> Rooms { get; set; }
        // The subject of this booking
        public virtual Subject Subject { get; set; }

        // Students attending this booking
        public virtual List<Student> Students { get; set; }
        // Teacher who made the booking
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

        // Determines if this booking will happen on a given day
        public bool MatchesDay(DateTime Day)
        {
            return (BookingType == BookingType.Single && Day.Date == Date) ||
                    (BookingType == BookingType.Weekly && (Day.Date - Date).Days % 7 == 0) ||
                    (BookingType == BookingType.Fortnightly && (Day.Date - Date).Days % 14 == 0) ||
                    (BookingType == BookingType.Monthly && (Day.Date - Date).Days % 28 == 0);
        }
        public override bool Conflicts(List<DataModel> AllBookings)
        {
            // Checks for conflicts between booking objects
            return AllBookings.Cast<Booking>().Any(b => b.Id != Id && b.Date == Date && b.TimeSlot == TimeSlot && b.Rooms.Intersect(Rooms).Count() != 0);
        }

        public override void Update(DataModel Other)
        {
            // Copy data from other item into this
            Booking b = (Booking)Other;
            Ticks = b.Ticks;
            BookingType = b.BookingType;
            TimeSlot = b.TimeSlot;
            Rooms = b.Rooms;
            Subject = b.Subject;
            Students.Clear();
            Students.AddRange(b.Students);
            Teacher = b.Teacher;
        }

        // Serialise to an output stream
        public override void Serialise(Writer Out)
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
        // Deserialise from an input stream
        protected override void Deserialise(Reader In)
        {
            base.Deserialise(In);

            Ticks = In.ReadInt64();
            BookingType = (BookingType)In.ReadInt32();
            TimeSlot = new TimeSlot() { Id = In.ReadInt32() };
            Rooms = new List<Room>(In.ReadInt32());
            for (int x = 0; x < Rooms.Capacity; x++)
                Rooms.Add(new Room() { Id = In.ReadInt32() });
            Subject = new Subject() { Id = In.ReadInt32() };
            Students = new List<Student>(In.ReadInt32());
            for (int x = 0; x < Students.Capacity; x++)
                Students.Add(new Student() { Id = In.ReadInt32() });
            Teacher = new Teacher() { Id = In.ReadInt32() };
        }
        // Obtain references to related objects
        public override bool Expand(IDataRepository Repo)
        {
            try
            {
                TimeSlot = Repo.Periods.SingleOrDefault(t => t.Id == TimeSlot.Id);
                for (int x = 0; x < Rooms.Count; x++)
                    Rooms[x] = Repo.Rooms.SingleOrDefault(r => Rooms[x].Id == r.Id);
                Subject = Repo.Subjects.SingleOrDefault(s => s.Id == Subject.Id);
                for (int x = 0; x < Students.Count; x++)
                    Students[x] = (Student)Repo.Users.SingleOrDefault(s => Students[x].Id == s.Id);
                Teacher = (Teacher)Repo.Users.SingleOrDefault(t => t.Id == Teacher.Id);
            }
            catch
            {
                return false;
            }
            return true;
        }
        // Set references to this object
        public override void Attach()
        {
            if (TimeSlot != null)
                TimeSlot.Bookings.Add(this);
            Rooms.ForEach(r => r.Bookings.Add(this));
            if (Subject != null)
                Subject.Bookings.Add(this);
            Students.ForEach(s => s.Bookings.Add(this));
            if (Teacher != null)
                Teacher.Bookings.Add(this);
        }
        // Remove references to this object
        public override void Detach()
        {
            if (TimeSlot != null)
                TimeSlot.Bookings.RemoveAll(i => i.Id == Id);
            Rooms.ForEach(r => { if (r != null) r.Bookings.RemoveAll(b => b.Id == Id); });
            if (Subject != null)
                Subject.Bookings.RemoveAll(i => i.Id == Id);
            Students.ForEach(s => { if (s != null) s.Bookings.RemoveAll(b => b.Id == Id); });
            if (Teacher != null)
                Teacher.Bookings.RemoveAll(i => i.Id == Id);
        }

        // Equality operators
        public override bool Equals(object obj)
        {
            if (!(obj is Booking)) // Type check
                return false;
            return this == (obj as Booking);
        }
        public static bool operator ==(Booking One, Booking Two)
        {
            // Check for references
            if (ReferenceEquals(One, null))
            {
                if (ReferenceEquals(Two, null))
                    return true;
                return false;
            }
            if (ReferenceEquals(Two, null))
                return false;

            // Check for references then compare fields
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
}