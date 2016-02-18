using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations.Schema;

using Shared;

namespace Data.Models
{
    // Used to easily select multiple students from a pre-set list
    // Links multiple students to a class name, with an owning teacher
    [Table("Classes")]
    public class Class
        : DataModel
    {
        // Name of the class, used for selections
        public string ClassName { get; set; }

        // Teacher that's responsible for the class
        public virtual Teacher Owner { get; set; }
        // Students included in the class
        public virtual List<Student> Students { get; set; }

        public Class()
        {
            ClassName = string.Empty;
            Students = new List<Student>();
        }

        public override bool Conflicts(List<DataModel> Others)
        {
            return Others.Cast<Class>().Any(c => c.Id != Id && c.ClassName == ClassName);
        }

        public override void Update(DataModel Other)
        {
            Class c = (Class)Other;

            ClassName = c.ClassName;
            Owner = c.Owner;
            Students.Clear();
            Students.AddRange(c.Students);
        }

        // Output properties and IDs
        public override void Serialise(Writer Out)
        {
            base.Serialise(Out);

            Out.Write(ClassName);
            Out.Write(Owner.Id);
            Out.Write(Students.Count);
            Students.ForEach(s => Out.Write(s.Id));
        }
        // Input properties and IDs
        protected override void Deserialise(Reader In)
        {
            base.Deserialise(In);

            ClassName = In.ReadString();
            Owner = new Teacher() { Id = In.ReadInt32() };
            Students = new List<Student>(In.ReadInt32());
            for (int x = 0; x < Students.Capacity; x++)
                Students.Add(new Student() { Id = In.ReadInt32() });
        }
        // Acquire references
        public override bool Expand(IDataRepository Repo)
        {
            try
            {
                Owner = (Teacher)Repo.Users.SingleOrDefault(t => t.Id == Owner.Id);
                for (int x = 0; x < Students.Count; x++)
                    Students[x] = Repo.Users.OfType<Student>().SingleOrDefault(s => s.Id == Students[x].Id);
            }
            catch
            {
                return false;
            }
            return true;
        }
        // Set references to this
        public override void Attach()
        {
            if (Owner != null)
                Owner.Classes.Add(this);
            Students.ForEach(s => s.Classes.Add(this));
        }
        // Remove references to this
        public override void Detach()
        {
            if (Owner != null)
                Owner.Classes.RemoveAll(i => i.Id == Id);
            Students.ForEach(s => { if (s != null) s.Classes.RemoveAll(i => i.Id == Id); });
        }

        public override string ToString()
        {
            return ClassName;
        }
    }
}
