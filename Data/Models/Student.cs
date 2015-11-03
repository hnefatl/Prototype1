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
        : User
    {
        public int Year { get; set; } // Eg. 13
        public string Form { get; set; } // Eg. WT

        [NotMapped]
        public string FullForm { get { return Year + Form; } }

        public override string InformalName { get { return FirstName + " " + LastName; } }
        public override string FormalName { get { return InformalName; } }

        public override UserType Discriminator { get { return UserType.Student; } }

        public virtual List<Class> Classes { get; set; }

        public virtual List<Booking> InternalBookings { get; set; }
        public override List<Booking> Bookings { get { return InternalBookings; } set { InternalBookings = value; } }

        public Student()
        {
            Classes = new List<Class>();
            InternalBookings = new List<Booking>();

            Access = AccessMode.Student;

            Form = string.Empty;
        }

        public override bool Conflicts(List<DataModel> Others)
        {
            return base.Conflicts(Others) || Others.OfType<Student>().Any(s => s.Id != Id && s.Year == Year && s.Form == Form);
        }

        public override void Update(DataModel Other)
        {
            base.Update(Other);

            Student s = (Student)Other;
            Year = s.Year;
            Form = s.Form;
            Classes = s.Classes;
        }

        public override void Serialise(IWriter Out)
        {
            base.Serialise(Out);

            Out.Write(Year);
            Out.Write(Form);
            Out.Write(Classes.Count);
            Classes.ForEach(c => Out.Write(c.Id));
        }
        protected override void Deserialise(IReader In)
        {
            base.Deserialise(In);

            Year = In.ReadInt32();
            Form = In.ReadString();

            Classes = Enumerable.Repeat(new Class(), In.ReadInt32()).ToList();
            Classes.ForEach(c => c.Id = In.ReadInt32());
        }
        public override bool Expand(IDataRepository Repo)
        {
            base.Expand(Repo);

            try
            {
                for (int x = 0; x < Classes.Count; x++)
                    Classes[x] = Repo.Classes.SingleOrDefault(c => c.Id == Classes[x].Id);
            }
            catch
            {
                return false;
            }
            return true;
        }
        public override void Detach()
        {
            Bookings.ForEach(b => { if (b != null) b.Students.RemoveAll(i => i.Id == Id); });
            Classes.ForEach(c => { if (c != null) c.Students.RemoveAll(i => i.Id == Id); });
        }

        public override string ToString()
        {
            return FirstName + " " + LastName;
        }
    }
}
