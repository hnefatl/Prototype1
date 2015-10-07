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
            Bookings = new List<Booking>(In.ReadInt32());
            for (int x = 0; x < Bookings.Capacity; x++)
            {
                Bookings.Add(new Booking());
                Bookings[x].Deserialise(In);
            }

            Departments = new List<Department>(In.ReadInt32());
            for (int x = 0; x < Departments.Capacity; x++)
            {
                Departments.Add(new Department());
                Departments[x].Deserialise(In);
            }

            Periods = new List<TimeSlot>(In.ReadInt32());
            for (int x = 0; x < Periods.Capacity; x++)
            {
                Periods.Add(new TimeSlot());
                Periods[x].Deserialise(In);
            }

            Rooms = new List<Room>(In.ReadInt32());
            for (int x = 0; x < Rooms.Capacity; x++)
            {
                Rooms.Add(new Room());
                Rooms[x].Deserialise(In);
            }

            Students = new List<Student>(In.ReadInt32());
            for (int x = 0; x < Students.Capacity; x++)
            {
                Students.Add(new Student());
                Students[x].Deserialise(In);
            }

            Subjects = new List<Subject>(In.ReadInt32());
            for (int x = 0; x < Subjects.Capacity; x++)
            {
                Subjects.Add(new Subject());
                Subjects[x].Deserialise(In);
            }

            Teachers = new List<Teacher>(In.ReadInt32());
            for (int x = 0; x < Teachers.Capacity; x++)
            {
                Teachers.Add(new Teacher());
                Teachers[x].Deserialise(In);
            }
        }
    }
}
