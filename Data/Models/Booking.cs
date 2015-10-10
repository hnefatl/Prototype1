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
        : ISerialisable, IExpandsData
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }


        public long Ticks { get; set; }
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
        {
            TimeSlot = new TimeSlot();
            Rooms = new List<Room>();
            Subject = new Subject();
            Students = new List<Student>();
            Teacher = new Teacher();
        }

        public bool MatchesDay(DateTime Day)
        {
            return (BookingType == BookingType.Single && Day.Date == Date) ||
                    (BookingType == BookingType.Weekly && (Day.Date - Date).Days % 7 == 0) ||
                    (BookingType == BookingType.Fortnightly && (Day.Date - Date).Days % 14 == 0) ||
                    (BookingType == BookingType.Monthly && (Day.Date - Date).Days % 31 == 0);
        }

        public void Serialise(IWriter Out)
        {
            Out.Write(Id);
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
        public void Deserialise(IReader In)
        {
            Id = In.ReadInt32();
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
        public bool Expand(IDataRepository Repo)
        {
            try
            {
                TimeSlot = Repo.Periods.Where(t => t.Id == TimeSlot.Id).Single();
                for (int x = 0; x < Rooms.Count; x++)
                    Rooms[x] = Repo.Rooms.Where(r => Rooms[x].Id == r.Id).Single();
                Subject = Repo.Subjects.Where(s => s.Id == Subject.Id).Single();
                for (int x = 0; x < Students.Count; x++)
                    Students[x] = Repo.Students.Where(s => Students[x].Id == s.Id).Single();
                Teacher = Repo.Teachers.Where(t => t.Id == Teacher.Id).Single();
            }
            catch
            {
                return false;
            }
            return true;
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