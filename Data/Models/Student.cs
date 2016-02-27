using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations.Schema;

using Shared;

namespace Data.Models
{
    // Contains all the students and relevant info
    [Table("Students")]
    public class Student
        : User
    {
        public int Year { get; set; } // Eg. 13
        public string Form { get; set; } // Eg. WT

        [NotMapped] // Utility property to get the full form name 
        public string FullForm { get { return Year + Form; } }

        public override string InformalName { get { return FirstName + " " + LastName; } }
        public override string FormalName { get { return InformalName; } }

        public override UserType Discriminator { get { return UserType.Student; } }

        // All bookings the user's involved in
        public virtual List<Booking> Bookings { get; set; }
        // All classes the user's involved in
        public virtual List<Class> Classes { get; set; }

        public Student()
        {
            Access = AccessMode.Student;

            Form = string.Empty;

            Bookings = new List<Booking>();
            Classes = new List<Class>();
        }

        public override bool Conflicts(List<DataModel> Others)
        {
            return base.Conflicts(Others);
        }

        public override void Update(DataModel Other)
        {
            base.Update(Other);

            Student s = (Student)Other;
            Year = s.Year;
            Form = s.Form;
            Classes.Clear();
            Classes.AddRange(s.Classes);
            Bookings.Clear();
            Bookings.AddRange(s.Bookings);
        }

        // Serialise all properties to the stream
        public override void Serialise(Writer Out)
        {
            // Serialise the base class' properties
            base.Serialise(Out);

            Out.Write(Year);
            Out.Write(Form);
            Out.Write(Classes.Count);
            Classes.ForEach(c => Out.Write(c.Id));

            Out.Write(Bookings.Count);
            Bookings.ForEach(b => Out.Write(b.Id));
        }
        // Deserialise from the stream
        protected override void Deserialise(Reader In)
        {
            base.Deserialise(In);

            Year = In.ReadInt32();
            Form = In.ReadString();

            Classes = Enumerable.Repeat(new Class(), In.ReadInt32()).ToList();
            Classes.ForEach(c => c.Id = In.ReadInt32());

            Bookings = Enumerable.Repeat(new Booking(), In.ReadInt32()).ToList();
            Bookings.ForEach(b => b.Id = In.ReadInt32());
        }
        // Obtain references to related entities
        public override bool Expand(IDataRepository Repo)
        {
            try
            {
                for (int x = 0; x < Classes.Count; x++)
                    Classes[x] = Repo.Classes.SingleOrDefault(c => c.Id == Classes[x].Id);
                for (int x = 0; x < Bookings.Count; x++)
                    Bookings[x] = Repo.Bookings.SingleOrDefault(b => b.Id == Bookings[x].Id);
            }
            catch
            {
                return false;
            }
            return true;
        }
        // Set references to this from other related objects
        public override void Attach()
        {
            Bookings.ForEach(b => b.Students.Add(this));
            Classes.ForEach(c => c.Students.Add(this));
        }
        // Remove references before deletion
        public override void Detach()
        {
            Bookings.ForEach(b => { if (b != null) b.Students.RemoveAll(i => i.Id == Id); });
            Classes.ForEach(c => { if (c != null) c.Students.RemoveAll(i => i.Id == Id); });
        }
    }
}
