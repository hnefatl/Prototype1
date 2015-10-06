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
    /// A Booking is a effectively a single lesson.
    /// </summary>
    [Table("Bookings")]
    public class Booking
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
        /// <summary>
        /// The students attending this booking
        /// </summary>
        /// 
        public virtual List<Student> Students { get; set; }
        public virtual Teacher Teacher { get; set; }
        public virtual List<Teacher> Assistants { get; set; }
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