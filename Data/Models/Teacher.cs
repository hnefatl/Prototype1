using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations.Schema;

using Shared;

namespace Data.Models
{
    // Contains all teachers and properties
    [Table("Teachers")]
    public class Teacher
        : User
    {
        public string Title { get; set; } // Eg. Mr, Mrs
        public string Email { get; set; } // Eg. email@gmail.com

        public override string InformalName { get { return FirstName + " " + LastName; } }
        public override string FormalName { get { return Title + " " + LastName; } }

        public override UserType Discriminator { get { return UserType.Teacher; } }

        public virtual Department Department { get; set; }

        // All bookings the user's involved in
        public virtual List<Booking> Bookings { get; set; }
        // All classes the user's involved in
        public virtual List<Class> Classes { get; set; }

        public Teacher()
        {
            Access = AccessMode.Teacher;

            Title = string.Empty;
            Email = string.Empty;

            Bookings = new List<Booking>();
            Classes = new List<Class>();
        }

        public override bool Conflicts(List<DataModel> Others)
        {
            // Return true if the base conflicts or the properties introduced in this class conflict
            return base.Conflicts(Others) || Others.OfType<Teacher>().Any(t => t.Id != Id && t.Title == Title && t.Email == Email);
        }

        public override void Update(DataModel Other)
        {
            base.Update(Other);

            Teacher t = (Teacher)Other;
            Title = t.Title;
            Email = t.Email;

            Department = t.Department;
            Classes.Clear();
            Classes.AddRange(t.Classes);
            Bookings.Clear();
            Bookings.AddRange(t.Bookings);
        }

        public override void Serialise(Writer Out)
        {
            // Output base class properties followed by this class's properties
            base.Serialise(Out);

            Out.Write(Title);
            Out.Write(Department.Id);
            Out.Write(Classes.Count);
            Classes.ForEach(c => Out.Write(c.Id));
            Out.Write(Email);

            Out.Write(Bookings.Count);
            Bookings.ForEach(b => Out.Write(b.Id));
        }
        protected override void Deserialise(Reader In)
        {
            // Deserialise base class then this class
            base.Deserialise(In);

            Title = In.ReadString();
            Department = new Department() { Id = In.ReadInt32() };
            Classes = Enumerable.Repeat(new Class(), In.ReadInt32()).ToList();
            Classes.ForEach(c => c.Id = In.ReadInt32());
            Email = In.ReadString();

            Bookings = Enumerable.Repeat(new Booking(), In.ReadInt32()).ToList();
            Bookings.ForEach(b => b.Id = In.ReadInt32());
        }
        // Obtain IDs of related objects
        public override bool Expand(IDataRepository Repo)
        {
            try
            {
                Department = Repo.Departments.SingleOrDefault(d => d.Id == Department.Id);
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
        // Branch out references
        public override void Attach()
        {
            Bookings.ForEach(b => b.Teacher = this);
            if (Department != null)
                Department.Teachers.Add(this);
            Classes.ForEach(c => c.Owner = this);
        }
        // Remove references
        public override void Detach()
        {
            Bookings.ForEach(b => { if (b != null) b.Teacher = null; });
            if (Department != null)
                Department.Teachers.RemoveAll(i => i.Id == Id);
            Classes.ForEach(c => { if (c != null) c.Owner = null; });
        }
    }
}
