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
            Owner = new Teacher();
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
            Owner.Id = In.ReadInt32();
            Students = Enumerable.Repeat(new Student(), In.ReadInt32()).ToList();
            Students.ForEach(s => s.Id = In.ReadInt32());
        }
        public override bool Expand(IDataRepository Repo)
        {
            try
            {
                Owner = Repo.Teachers.Where(t => t.Id == Owner.Id).Single();
                Students.ForEach(s1 => s1 = Repo.Students.Where(s2 => s1.Id == s2.Id).Single());
            }
            catch
            {
                return false;
            }
            return true;
        }
    }
}
