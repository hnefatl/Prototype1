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
    /// Contains all the students and relevant info
    /// </summary>
    [Table("Students")]
    public class Student
        : User, IExpandsData
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        public override AccessMode Access { get { return AccessMode.Student; } }

        public int Year { get; set; }
        public string Form { get; set; }

        public virtual List<Booking> Bookings { get; set; }

        public Student()
        {
            Bookings = new List<Booking>();

            Form = string.Empty;
        }

        public override void Serialise(IWriter Out)
        {
            base.Serialise(Out);

            Out.Write(Id);
            Out.Write(Year);
            Out.Write(Form);
            Out.Write(Bookings.Count);
            Bookings.ForEach(b => Out.Write(b.Id));
        }
        public override void Deserialise(IReader In)
        {
            base.Deserialise(In);

            Id = In.ReadInt32();
            Year = In.ReadInt32();
            Form = In.ReadString();
            Bookings = Enumerable.Repeat(new Booking(), In.ReadInt32()).ToList();
            Bookings.ForEach(b => b.Id = In.ReadInt32());
        }
        public bool Expand(IDataRepository Repo)
        {
            try
            {
                Bookings.ForEach(b => b = Repo.Bookings.Where(b2 => b2.Id == b.Id).Single());
            }
            catch
            {
                return false;
            }
            return true;
        }

        public override string ToString()
        {
            return FirstName + " " + LastName;
        }
    }
}
