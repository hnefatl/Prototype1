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
        
        public override string InformalName { get { return FirstName + " " + LastName; } }
        public override string FormalName { get { return Title + " " + LastName; } }

        public override AccessMode Access { get; set; }

        public override UserType Discriminator { get { return UserType.Teacher; } }

        public virtual Department Department { get; set; }

        public string Email { get; set; }
        public bool Admin { get; set; }

        public Teacher()
        {
            Department = new Department();

            Access = AccessMode.Teacher;

            Title = string.Empty;
            Email = string.Empty;
        }

        public override void Serialise(IWriter Out)
        {
            base.Serialise(Out);

            Out.Write(Title);
            Out.Write(Department.Id);
            Out.Write(Email);
            Out.Write(Admin);

            Out.Write(Bookings.Count);
            Bookings.ForEach(b => Out.Write(b.Id));
        }
        protected override void Deserialise(IReader In)
        {
            base.Deserialise(In);

            Title = In.ReadString();
            Department.Id = In.ReadInt32();
            Email = In.ReadString();
            Admin = In.ReadBool();

            Bookings = Enumerable.Repeat(new Booking(), In.ReadInt32()).ToList();
            Bookings.ForEach(b => b.Id = In.ReadInt32());
        }
        public override bool Expand(IDataRepository Repo)
        {
            try
            {
                Department = Repo.Departments.Where(d => d.Id == Department.Id).Single();
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
            return InformalName;
        }
    }
}
