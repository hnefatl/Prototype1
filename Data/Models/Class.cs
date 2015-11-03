using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Shared;

namespace Data.Models
{
    [Table("Classes")]
    public class Class
        : DataModel
    {
        public string ClassName { get; set; }

        public virtual Teacher Owner { get; set; }
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
            Students = c.Students;
        }

        public override void Serialise(IWriter Out)
        {
            base.Serialise(Out);

            Out.Write(ClassName);
            Out.Write(Owner.Id);
            Out.Write(Students.Count);
            Students.ForEach(s => Out.Write(s.Id));
        }
        protected override void Deserialise(IReader In)
        {
            base.Deserialise(In);

            ClassName = In.ReadString();
            Owner = new Teacher() { Id = In.ReadInt32() };
            Students = Enumerable.Repeat(new Student(), In.ReadInt32()).ToList();
            Students.ForEach(s => s.Id = In.ReadInt32());
        }
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
        public override void Detach()
        {
            if (Owner != null)
                Owner.Classes.RemoveAll(i => i.Id == Id);
            Students.ForEach(s => { if (s != null) s.Classes.RemoveAll(i => i.Id == Id); });
        }
    }
}
