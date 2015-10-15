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
        public override string InformalName { get { return FirstName + " " + LastName; } }
        public override string FormalName { get { return InformalName; } }

        public override AccessMode Access { get { return AccessMode.Student; } }

        public int Year { get; set; } // 13
        public string Form { get; set; } // WT

        [NotMapped]
        public string FullForm { get { return Year + Form; } }

        public virtual List<Booking> Bookings { get; set; }
        public virtual List<Class> Classes { get; set; }

        public Student()
        {
            Bookings = new List<Booking>();
            Classes = new List<Class>();

            Form = string.Empty;
        }

        public override void Serialise(IWriter Out)
        {
            base.Serialise(Out);

            Out.Write(Year);
            Out.Write(Form);
            Out.Write(Bookings.Count);
            Bookings.ForEach(b => Out.Write(b.Id));
            Out.Write(Classes.Count);
            Classes.ForEach(c => Out.Write(c.Id));
        }
        public override void Deserialise(IReader In)
        {
            base.Deserialise(In);

            Year = In.ReadInt32();
            Form = In.ReadString();
            Bookings = Enumerable.Repeat(new Booking(), In.ReadInt32()).ToList();
            Bookings.ForEach(b => b.Id = In.ReadInt32());

            Classes = Enumerable.Repeat(new Class(), In.ReadInt32()).ToList();
            Classes.ForEach(c => c.Id = In.ReadInt32());
        }
        public bool Expand(IDataRepository Repo)
        {
            try
            {
                Bookings.ForEach(b => b = Repo.Bookings.Where(b2 => b2.Id == b.Id).Single());
                Classes.ForEach(c1 => c1 = Repo.Classes.Where(c2 => c2.Id == c1.Id).Single());
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
