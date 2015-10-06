using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Windows.Media;

namespace Data.DataContext.Models
{
    /// <summary>
    /// Contains all subjects (eg Maths, Computing)
    /// </summary>
    [Table("Subjects")]
    public class Subject
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Friendly name of the subject (Maths, Computing)
        /// </summary>
        public string SubjectName { get; set; }

        public int Argb
        {
            get
            {
                return Helpers.ColorToInt(Colour);
            }
            set
            {
                Colour = Helpers.IntToColour(value);
            }
        }

        /// <summary>
        /// Colour used to display bookings of this subject on the timetable
        /// </summary>
        [NotMapped]
        public Color Colour { get; set; }

        /// <summary>
        /// Bookings of this subject
        /// </summary>
        public virtual List<Booking> Bookings { get; set; }
        /// <summary>
        /// Students taking a subject
        /// </summary>
        public virtual List<Student> Students { get; set; }

        public override string ToString()
        {
            return SubjectName;
        }
    }
}
