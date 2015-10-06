using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        : ISerialisable
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public abstract AccessMode Access { get; }

        public virtual bool IsStudent { get { return Access == AccessMode.Student; } }
        public virtual bool IsTeacher { get { return Access == AccessMode.Teacher; } }
        public virtual bool IsAdmin { get { return Access == AccessMode.Admin; } }

        public User()
        {
            FirstName = string.Empty;
            LastName = string.Empty;
        }

        public virtual void Serialise(IWriter Out)
        {
            Out.Write(FirstName);
            Out.Write(LastName);
        }
        public virtual void Deserialise(IReader In)
        {
            FirstName = In.ReadString();
            LastName = In.ReadString();
        }
    }
}
