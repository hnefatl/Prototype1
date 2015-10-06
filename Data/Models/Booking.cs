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
        : ISerialisable
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
        public virtual List<Room> Rooms { get; set; }
        /// <summary>
        /// The subject of this booking
        /// </summary>
        public virtual Subject Subject { get; set; }

        public virtual List<Student> Students { get; set; }
        public virtual Teacher Teacher { get; set; }
        public virtual List<Teacher> Assistants { get; set; }

        public Booking()
        {
            TimeSlot = new TimeSlot();
            Rooms = new List<Room>();
            Subject = new Subject();
            Students = new List<Student>();
            Teacher = new Teacher();
            Assistants = new List<Teacher>();
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
            Out.Write(Assistants.Count);
            foreach (Teacher a in Assistants)
                Out.Write(a.Id);
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
            Assistants = Enumerable.Repeat(new Teacher(), In.ReadInt32()).ToList();
            foreach (Teacher a in Assistants)
                a.Id = In.ReadInt32();
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