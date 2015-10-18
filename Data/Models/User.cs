using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Shared;

namespace Data.Models
{
    public enum AccessMode
    {
        Student,
        Teacher,
        Admin,
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

        public abstract AccessMode Access { get; }

        public virtual bool IsStudent { get { return Access == AccessMode.Student; } }
        public virtual bool IsTeacher { get { return Access == AccessMode.Teacher; } }
        public virtual bool IsAdmin { get { return Access == AccessMode.Admin; } }

        public User()
        {
            FirstName = string.Empty;
            LastName = string.Empty;
        }

        public override void Serialise(IWriter Out)
        {
            base.Serialise(Out);

            Out.Write(FirstName);
            Out.Write(LastName);
            Out.Write(LogonName);
        }
        protected override void Deserialise(IReader In)
        {
            base.Deserialise(In);

            FirstName = In.ReadString();
            LastName = In.ReadString();
            LogonName = In.ReadString();
        }
    }
}
