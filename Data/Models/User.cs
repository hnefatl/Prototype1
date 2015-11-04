using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Shared;

namespace Data.Models
{
    public enum AccessMode // Actual access mode in the system
    {
        Student,
        Teacher,
        Admin,
    }
    public enum UserType // Internal discriminator for the database between Students and Teachers (for their respective tables)
    {
        Student,
        Teacher,
    }

    public abstract class User
        : DataModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string LogonName { get; set; }

        [NotMapped]
        public abstract string InformalName { get; }
        [NotMapped]
        public abstract string FormalName { get; }

        public virtual AccessMode Access { get; set; }

        public abstract UserType Discriminator { get; }

        [NotMapped]
        public abstract List<Booking> Bookings { get; set; }

        [NotMapped]
        public virtual bool IsStudent { get { return Access == AccessMode.Student; } }
        [NotMapped]
        public virtual bool IsTeacher { get { return Access == AccessMode.Teacher; } }
        [NotMapped]
        public virtual bool IsAdmin { get { return Access == AccessMode.Admin; } }

        public User()
        {
            FirstName = string.Empty;
            LastName = string.Empty;

            Bookings = new List<Booking>();
        }

        public override bool Conflicts(List<DataModel> Others)
        {
            return Others.Cast<User>().Any(u => u.Id != Id && u.LogonName == LogonName);
        }

        public override void Update(DataModel Other)
        {
            User u = (User)Other;

            FirstName = u.FirstName;
            LastName = u.LastName;
            LogonName = u.LogonName;
            Access = u.Access;
            Bookings.Clear();
            Bookings.AddRange(u.Bookings);
        }

        public override void Serialise(IWriter Out)
        {
            base.Serialise(Out);

            Out.Write((int)Access);
            Out.Write(FirstName);
            Out.Write(LastName);
            Out.Write(LogonName);

            Out.Write(Bookings.Count);
            Bookings.ForEach(b => Out.Write(b.Id));
        }
        protected override void Deserialise(IReader In)
        {
            base.Deserialise(In);

            Access = (AccessMode)In.ReadInt32();
            FirstName = In.ReadString();
            LastName = In.ReadString();
            LogonName = In.ReadString();

            Bookings = Enumerable.Repeat(new Booking(), In.ReadInt32()).ToList();
            Bookings.ForEach(b => b.Id = In.ReadInt32());
        }

        public override bool Expand(IDataRepository Repo)
        {
            try
            {
                for (int x = 0; x < Bookings.Count; x++)
                    Bookings[x] = Repo.Bookings.SingleOrDefault(b => b.Id == Bookings[x].Id);
            }
            catch
            {
                return false;
            }
            return true;
        }
    }
}
