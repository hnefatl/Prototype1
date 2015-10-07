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
                return DateTime.FromBinary(Ticks);
            }
            set
            {
                Ticks = value.ToBinary();
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
        public virtual IList<Room> Rooms { get; set; }
        /// <summary>
        /// The subject of this booking
        /// </summary>
        public virtual Subject Subject { get; set; }

        public virtual IList<Student> Students { get; set; }
        public virtual Teacher Teacher { get; set; }

        public Booking()
        {
            TimeSlot = new TimeSlot();
            Rooms = new List<Room>();
            Subject = new Subject();
            Students = new List<Student>();
            Teacher = new Teacher();
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
            Rooms = Enumerable.Repeat(new Room(), In.ReadInt32()).ToList();
            foreach (Room r in Rooms)
                r.Id = In.ReadInt32();
            Subject.Id = In.ReadInt32();
            Students = Enumerable.Repeat(new Student(), In.ReadInt32()).ToList();
            foreach (Student s in Students)
                s.Id = In.ReadInt32();
            Teacher.Id = In.ReadInt32();
        }

        public bool Expand(IDataRepository Repo)
        {
            try
            {
                TimeSlot = Repo.Periods.Where(t => t.Id == TimeSlot.Id).Single();
                Rooms.ForEach(r => r = Repo.Rooms.Where(r2 => r.Id == r2.Id).Single());
                Subject = Repo.Subjects.Where(s => s.Id == Subject.Id).Single();
                Students.ForEach(s => s = Repo.Students.Where(s2 => s.Id == s2.Id).Single());
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