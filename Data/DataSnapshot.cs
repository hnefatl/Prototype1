using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Data.Models;
using Shared;

namespace Data
{
    public class DataSnapshot
        : ISerialisable
    {
        public List<Booking> Bookings { get; set; }
        public List<Department> Departments { get; set; }
        public List<Room> Rooms { get; set; }
        public List<Student> Students { get; set; }
        public List<Subject> Subjects { get; set; }
        public List<Teacher> Teachers { get; set; }
        public List<TimeSlot> Periods { get; set; }

        public DataSnapshot()
        {
            Bookings = new List<Booking>();
            Departments = new List<Department>();
            Rooms = new List<Room>();
            Students = new List<Student>();
            Subjects = new List<Subject>();
            Teachers = new List<Teacher>();
            Periods = new List<TimeSlot>();
        }

        public void Serialise(IWriter Out)
        {
            Out.Write(Bookings.Count);
            Bookings.ForEach(b => b.Serialise(Out));

            Out.Write(Departments.Count);
            Departments.ForEach(d => d.Serialise(Out));

            Out.Write(Periods.Count);
            Periods.ForEach(p => p.Serialise(Out));

            Out.Write(Rooms.Count);
            Rooms.ForEach(r => r.Serialise(Out));

            Out.Write(Students.Count);
            Students.ForEach(s => s.Serialise(Out));

            Out.Write(Subjects.Count);
            Subjects.ForEach(s => s.Serialise(Out));

            Out.Write(Teachers.Count);
            Teachers.ForEach(t => t.Serialise(Out));
        }
        public void Deserialise(IReader In)
        {
            Bookings = Enumerable.Repeat(new Booking(), In.ReadInt32()).ToList();
            Bookings.ForEach(b => b.Deserialise(In));

            Departments = Enumerable.Repeat(new Department(), In.ReadInt32()).ToList();
            Departments.ForEach(d => d.Deserialise(In));

            Periods = Enumerable.Repeat(new TimeSlot(), In.ReadInt32()).ToList();
            Periods.ForEach(p => p.Deserialise(In));

            Rooms = Enumerable.Repeat(new Room(), In.ReadInt32()).ToList();
            Rooms.ForEach(r => r.Deserialise(In));

            Students = Enumerable.Repeat(new Student(), In.ReadInt32()).ToList();
            Students.ForEach(s => s.Deserialise(In));

            Subjects = Enumerable.Repeat(new Subject(), In.ReadInt32()).ToList();
            Subjects.ForEach(s => s.Deserialise(In));

            Teachers = Enumerable.Repeat(new Teacher(), In.ReadInt32()).ToList();
            Teachers.ForEach(t => t.Deserialise(In));
        }
    }
}
