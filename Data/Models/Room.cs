using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations.Schema;

using Shared;

namespace Data.Models
{
    // Rooms are the area that can be booked
    [Table("Rooms")]
    public class Room
        : DataModel
    {
        // Recognisable name of the room (eg D6, D12, Library)
        public string RoomName { get; set; }

        // Number of "standard seats". Usually just number of available desks
        public int StandardSeats { get; set; }

        // Number of "special seats", for example a computer, workbench etc
        public int SpecialSeats { get; set; }

        // Type of "Special Seat", so eg "Computer", "Workbench"
        public string SpecialSeatType { get; set; }

        // Bookings using this room
        public virtual List<Booking> Bookings { get; set; }

        public virtual Department Department { get; set; }

        protected List<string> _ComputerNames = new List<string>();
        [NotMapped] // The list of names of computers in a room (eg D12C2)
        public List<string> ComputerNames
        {
            get { return _ComputerNames; }
            set
            {
                // Can't have a room containing the character used to delimit computer names
                if (value.Any(s => s.Contains(ComputerNameSeperator)))
                    throw new ArgumentException("Computer name cannot contain '" + ComputerNameSeperator + "'.");
                _ComputerNames = value;
            }
        }
        // Can't store a List<string> in the DB, so store a string formed by joining all
        // the names, delimiting with a seperator. Gettings/Setting this property creates
        // the string by working on the list. This is the actual property stored in the DB
        public string ComputerNamesJoined
        {
            get { return string.Join("" + ComputerNameSeperator, ComputerNames); }
            set { ComputerNames = value.Split(ComputerNameSeperator).ToList(); }
        }

        // Character used to delimit computer names in the joined string
        public const char ComputerNameSeperator = '|';

        public Room()
        {
            Bookings = new List<Booking>();
            ComputerNames = new List<string>();

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
            Bookings.Clear();
            Bookings.AddRange(r.Bookings);
            Department = r.Department;
        }

        // Serialise properties and IDs
        public override void Serialise(Writer Out)
        {
            base.Serialise(Out);

            Out.Write(RoomName);
            Out.Write(StandardSeats);
            Out.Write(SpecialSeats);
            Out.Write(SpecialSeatType);
            Out.Write(ComputerNamesJoined);

            Out.Write(Bookings.Count);
            Bookings.ForEach(b => Out.Write(b.Id));
            Out.Write(Department.Id);
        }
        // Deserialise properties and IDs
        protected override void Deserialise(Reader In)
        {
            base.Deserialise(In);

            RoomName = In.ReadString();
            StandardSeats = In.ReadInt32();
            SpecialSeats = In.ReadInt32();
            SpecialSeatType = In.ReadString();
            ComputerNamesJoined = In.ReadString();

            Bookings = Enumerable.Repeat(new Booking(), In.ReadInt32()).ToList();
            Bookings.ForEach(b => b.Id = In.ReadInt32());
            Department = new Department() { Id = In.ReadInt32() };
        }
        // Obtain references to related items
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
        // Set references to this item
        public override void Attach()
        {
            Bookings.ForEach(b => b.Rooms.Add(this));
            if (Department != null)
                Department.Rooms.Add(this);
        }
        // Remove references to this item
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
