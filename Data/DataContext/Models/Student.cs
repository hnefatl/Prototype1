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
    /// Contains all the students and relevant info
    /// </summary>
    [Table("Students")]
    public class Student
        : User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        public override AccessMode Access { get { return AccessMode.Student; } }

        public int Year { get; set; }
        public string Form { get; set; }

        public virtual List<Booking> Bookings { get; set; }

        public override string ToString()
        {
            return FirstName + " " + LastName;
        }
    }
}
