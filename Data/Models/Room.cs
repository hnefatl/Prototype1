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
    /// Rooms are the area that can be booked, and contains some useful info
    /// </summary>
    [Table("Rooms")]
    public class Room
        : DataModel
    {
        /// <summary>
        /// Recognisable name of the room (eg D6, D12, Library)
        /// </summary>
        public string RoomName { get; set; }
        /// <summary>
        /// Number of "standard seats" that don't consume specific resources. Usually just number of available desks.
        /// </summary>
        public int StandardSeats { get; set; }
        /// <summary>
        /// Number of "special seats" that consume specific resources, for example a computer, workbench etc.
        /// </summary>
        public int SpecialSeats { get; set; }

        /// <summary>
        /// Type of "Special Seat", so eg "Computer", "Workbench".
        /// </summary>
        public string SpecialSeatType { get; set; }

        /// <summary>
        /// Bookings using this room
        /// </summary>
        public virtual List<Booking> Bookings { get; set; }

        public virtual Department Department { get; set; }

        public Room()
        {
            Bookings = new List<Booking>();

            RoomName = string.Empty;
            SpecialSeatType = string.Empty;
        }

        public override bool Conflicts(List<DataModel> Others)
        {
            return Others.Cast<Room>().Any(r => r.Id != Id && r.RoomName == RoomName);
        }

        public override void Update(DataModel Other)
        {
            Room r = (Room)Other;

            RoomName = r.RoomName;
            StandardSeats = r.StandardSeats;
            SpecialSeatType = r.SpecialSeatType;
            SpecialSeats = r.SpecialSeats;
            Bookings = r.Bookings;
            Department = r.Department;
        }

        public override void Serialise(IWriter Out)
        {
            base.Serialise(Out);

            Out.Write(RoomName);
            Out.Write(StandardSeats);
            Out.Write(SpecialSeats);
            Out.Write(SpecialSeatType);

            Out.Write(Bookings.Count);
            Bookings.ForEach(b => Out.Write(b.Id));
            Out.Write(Department.Id);
        }
        protected override void Deserialise(IReader In)
        {
            base.Deserialise(In);

            RoomName = In.ReadString();
            StandardSeats = In.ReadInt32();
            SpecialSeats = In.ReadInt32();
            SpecialSeatType = In.ReadString();

            Bookings = Enumerable.Repeat(new Booking(), In.ReadInt32()).ToList();
            Bookings.ForEach(b => b.Id = In.ReadInt32());
            Department = new Department() { Id = In.ReadInt32() };
        }
        public override bool Expand(IDataRepository Repo)
        {
            try
            {
                for (int x = 0; x < Bookings.Count; x++)
                    Bookings[x] = Repo.Bookings.SingleOrDefault(b => b.Id == Bookings[x].Id);
                Department = Repo.Departments.SingleOrDefault(d => d.Id == Department.Id);
            }
            catch
            {
                return false;
            }
            return true;
        }
        public override void Detach()
        {
            Bookings.ForEach(b => { if (b != null) b.Rooms.RemoveAll(i => i.Id == Id); });
            if (Department != null)
                Department.Rooms.RemoveAll(i => i.Id == Id);
        }

        public override string ToString()
        {
            return RoomName;
        }
    }
}
