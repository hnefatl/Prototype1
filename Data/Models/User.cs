using System;
using System.Collections.Generic;
using System.Linq;
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
    public enum UserType // Internal discriminator for the database between Students and Teachers
    {
        Student,
        Teacher,
    }

    public abstract class User
        : DataModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string LogonName { get; set; } // User's school username

        [NotMapped] // InformalName isn't stored in the DB, but constructed from First and Last names
        public abstract string InformalName { get; }
        [NotMapped] // Again, not stored in the DB but constructed
        public abstract string FormalName { get; }

        // Access level in the system (used for all authentication checks)
        public virtual AccessMode Access { get; set; }

        // Used to differentiate between Students and Teachers while storing both in the same table.
        public abstract UserType Discriminator { get; }

        [NotMapped] // Helpful unmapped properties, good for UI bindings
        public virtual bool IsStudent { get { return Access == AccessMode.Student; } }
        [NotMapped]
        public virtual bool IsTeacher { get { return Access == AccessMode.Teacher; } }
        [NotMapped]
        public virtual bool IsAdmin { get { return Access == AccessMode.Admin; } }

        public User()
        {
            FirstName = string.Empty;
            LastName = string.Empty;
        }

        public override bool Conflicts(List<DataModel> Others)
        {
            // Conflicts occur if the same ID and same LogonName are used elsewhere
            return Others.Cast<User>().Any(u => u.Id != Id && u.LogonName == LogonName);
        }

        // Update this entry's data to match those provided
        public override void Update(DataModel Other)
        {
            User u = (User)Other;

            FirstName = u.FirstName;
            LastName = u.LastName;
            LogonName = u.LogonName;
            Access = u.Access;
        }

        // Serialise all required properties
        public override void Serialise(Writer Out)
        {
            base.Serialise(Out);

            Out.Write((int)Access);
            Out.Write(FirstName);
            Out.Write(LastName);
            Out.Write(LogonName);
        }
        // Deserialise all properties
        protected override void Deserialise(Reader In)
        {
            base.Deserialise(In);

            Access = (AccessMode)In.ReadInt32();
            FirstName = In.ReadString();
            LastName = In.ReadString();
            LogonName = In.ReadString();
        }

        public override string ToString()
        {
            return InformalName;
        }
    }
}
