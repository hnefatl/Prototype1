using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.DataContext.Models
{
    /// <summary>
    /// Rooms are the area that can be booked, and contains some useful info
    /// </summary>
    [Table("Rooms")]
    public class Room
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

        public override string ToString()
        {
            return RoomName;
        }
    }
}
