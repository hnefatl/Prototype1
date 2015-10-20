using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Data.DataContext.Models
{
    public enum AccessMode
    {
        Student,
        Teacher,
        Admin,
    }

    public abstract class User
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public abstract AccessMode Access { get; }

        public virtual bool IsStudent { get { return Access == AccessMode.Student; } }
        public virtual bool IsTeacher { get { return Access == AccessMode.Teacher; } }
        public virtual bool IsAdmin { get { return Access == AccessMode.Admin; } }
    }
}
