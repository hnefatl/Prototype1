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
    [Table("Teachers")]
    public class Teacher
        : User
    {
        public string Title { get; set; }
        public string Email { get; set; }

        public override string InformalName { get { return FirstName + " " + LastName; } }
        public override string FormalName { get { return Title + " " + LastName; } }

        public override AccessMode Access { get; set; }

        public override UserType Discriminator { get { return UserType.Teacher; } }

        public virtual Department Department { get; set; }
        public virtual List<Class> Classes { get; set; }

        public virtual List<Booking> InternalBookings { get; set; }
        public override List<Booking> Bookings { get { return InternalBookings; } set { InternalBookings = value; } }

        public Teacher()
        {
            Classes = new List<Class>();

            Access = AccessMode.Teacher;

            Title = string.Empty;
            Email = string.Empty;
        }

        public override bool Conflicts(List<DataModel> Others)
        {
            return base.Conflicts(Others) || Others.OfType<Teacher>().Any(t => t.Id != Id && t.Title != Title && t.Email != Email);
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
        }

        public override void Serialise(IWriter Out)
        {
            base.Serialise(Out);

            Out.Write(Title);
            Out.Write(Department.Id);
            Out.Write(Classes.Count);
            Classes.ForEach(c => Out.Write(c.Id));
            Out.Write(Email);
        }
        protected override void Deserialise(IReader In)
        {
            base.Deserialise(In);

            Title = In.ReadString();
            Department = new Department() { Id = In.ReadInt32() };
            Classes = Enumerable.Repeat(new Class(), In.ReadInt32()).ToList();
            Classes.ForEach(c => c.Id = In.ReadInt32());
            Email = In.ReadString();
        }
        public override bool Expand(IDataRepository Repo)
        {
            base.Expand(Repo);

            try
            {
                Department = Repo.Departments.SingleOrDefault(d => d.Id == Department.Id);
                for (int x = 0; x < Classes.Count; x++)
                    Classes[x] = Repo.Classes.SingleOrDefault(c => c.Id == Classes[x].Id);
            }
            catch
            {
                return false;
            }
            return true;
        }
        public override void Attach()
        {
            Bookings.ForEach(b => b.Teacher = this);
            if (Department != null)
                Department.Teachers.Add(this);
            Classes.ForEach(c => c.Owner = this);
        }
        public override void Detach()
        {
            Bookings.ForEach(b => { if (b != null) b.Teacher = null; });
            if (Department != null)
                Department.Teachers.RemoveAll(i => i.Id == Id);
            Classes.ForEach(c => { if (c != null) c.Owner = null; });
        }

        public override string ToString()
        {
            return InformalName;
        }
    }
}
