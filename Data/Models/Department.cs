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
    [Table("Departments")]
    public class Department
        : DataModel
    {
        public string Name { get; set; }

        public virtual List<Teacher> Teachers { get; set; }

        public Department()
        {
            Teachers = new List<Teacher>();

            Name = string.Empty;
        }

        public override bool Conflicts(List<DataModel> Others)
        {
            return Others.Cast<Department>().Any(d => d.Id != Id && d.Name == Name);
        }

        public override void Update(DataModel Other)
        {
            Department d = (Department)Other;

            Name = d.Name;
            Teachers = d.Teachers;
        }

        public override void Serialise(IWriter Out)
        {
            base.Serialise(Out);

            Out.Write(Name);
            Out.Write(Teachers.Count);
            Teachers.ForEach(t => Out.Write(t.Id));
        }
        protected override void Deserialise(IReader In)
        {
            base.Deserialise(In);

            Name = In.ReadString();
            Teachers = Enumerable.Repeat(new Teacher(), In.ReadInt32()).ToList();
            Teachers.ForEach(t => t.Id = In.ReadInt32());
        }
        public override bool Expand(IDataRepository Repo)
        {
            try
            {
                for (int x = 0; x < Teachers.Count; x++)
                    Teachers[x] = Repo.Users.OfType<Teacher>().Single(t => t.Id == Teachers[x].Id);
            }
            catch
            {
                return false;
            }
            return true;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
