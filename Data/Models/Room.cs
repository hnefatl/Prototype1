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
        : ISerialisable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

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
        public string SpecialSeatsType { get; set; }

        /// <summary>
        /// Bookings using this room
        /// </summary>
        public virtual List<Booking> Bookings { get; set; }

        public Room()
        {
            Bookings = new List<Booking>();

            RoomName = string.Empty;
            SpecialSeatsType = string.Empty;
        }

        public void Serialise(IWriter Out)
        {
            Out.Write(Id);
            Out.Write(RoomName);
            Out.Write(StandardSeats);
            Out.Write(SpecialSeats);
            Out.Write(SpecialSeatsType);

            Out.Write(Bookings.Count);
            Bookings.ForEach(b => Out.Write(b.Id));
        }
        public void Deserialise(IReader In)
        {
            Id = In.ReadInt32();
            RoomName = In.ReadString();
            StandardSeats = In.ReadInt32();
            SpecialSeats = In.ReadInt32();
            SpecialSeatsType = In.ReadString();

            Bookings = Enumerable.Repeat(new Booking(), In.ReadInt32()).ToList();
            Bookings.ForEach(b => b.Id = In.ReadInt32());
        }

        public override string ToString()
        {
            return RoomName;
        }
    }
}
