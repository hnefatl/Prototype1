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
    [Table("Teachers")]
    public class Teacher
        : User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        public string Title { get; set; }
        
        public override AccessMode Access { get { return AccessMode.Teacher; } }

        public virtual Department Department { get; set; }

        public string Email { get; set; }
        public string Password { get; set; }
        public bool Admin { get; set; }

        /// <summary>
        /// The bookings this teacher is teaching during
        /// </summary>
        public virtual List<Booking> TeachingBookings { get; set; }
        /// <summary>
        /// The bookings this teacher is assisting during (TA)
        /// </summary>
        public virtual List<Booking> AssistingBookings { get; set; }

        public override string ToString()
        {
            return Title + " " + LastName;
        }
    }
}
