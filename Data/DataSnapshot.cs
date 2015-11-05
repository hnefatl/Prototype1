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
        public List<User> Users { get; set; }
        public List<Subject> Subjects { get; set; }
        public List<TimeSlot> Periods { get; set; }
        public List<Class> Classes { get; set; }

        public DataSnapshot()
        {
            Bookings = new List<Booking>();
            Departments = new List<Department>();
            Rooms = new List<Room>();
            Users = new List<User>();
            Subjects = new List<Subject>();
            Periods = new List<TimeSlot>();
            Classes = new List<Class>();
        }

        public void Serialise(Writer Out)
        {
            Out.Write(Bookings.Count);
            Bookings.ForEach(b => b.Serialise(Out));

            Out.Write(Departments.Count);
            Departments.ForEach(d => d.Serialise(Out));

            Out.Write(Periods.Count);
            Periods.ForEach(p => p.Serialise(Out));

            Out.Write(Rooms.Count);
            Rooms.ForEach(r => r.Serialise(Out));

            Out.Write(Users.Count);
            Users.ForEach(t => t.Serialise(Out));

            Out.Write(Subjects.Count);
            Subjects.ForEach(s => s.Serialise(Out));

            Out.Write(Classes.Count);
            Classes.ForEach(c => c.Serialise(Out));
        }
        public void Deserialise(Reader In)
        {
            Bookings = new List<Booking>(In.ReadInt32());
            for (int x = 0; x < Bookings.Capacity; x++)
                Bookings.Add((Booking)DataModel.DeserialiseExternal(In));

            Departments = new List<Department>(In.ReadInt32());
            for (int x = 0; x < Departments.Capacity; x++)
                Departments.Add((Department)DataModel.DeserialiseExternal(In));

            Periods = new List<TimeSlot>(In.ReadInt32());
            for (int x = 0; x < Periods.Capacity; x++)
                Periods.Add((TimeSlot)DataModel.DeserialiseExternal(In));

            Rooms = new List<Room>(In.ReadInt32());
            for (int x = 0; x < Rooms.Capacity; x++)
                Rooms.Add((Room)DataModel.DeserialiseExternal(In));

            Users = new List<User>(In.ReadInt32());
            for (int x = 0; x < Users.Capacity; x++)
                Users.Add((User)DataModel.DeserialiseExternal(In));

            Subjects = new List<Subject>(In.ReadInt32());
            for (int x = 0; x < Subjects.Capacity; x++)
                Subjects.Add((Subject)DataModel.DeserialiseExternal(In));
            
            Classes = new List<Class>(In.ReadInt32());
            for (int x = 0; x < Classes.Capacity; x++)
                Classes.Add((Class)DataModel.DeserialiseExternal(In));
        }
    }
}
