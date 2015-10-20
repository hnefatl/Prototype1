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

        public virtual List<Student> Students { get; set; }

        public Class()
        {
            Students = new List<Student>();
            ClassName = string.Empty;
        }

        public override void Serialise(IWriter Out)
        {
            base.Serialise(Out);
            
            Out.Write(ClassName);
            Out.Write(Students.Count);
            Students.ForEach(s => Out.Write(s.Id));
        }
        protected override void Deserialise(IReader In)
        {
            base.Deserialise(In);
            
            ClassName = In.ReadString();
            Students = Enumerable.Repeat(new Student(), In.ReadInt32()).ToList();
            Students.ForEach(s => s.Id = In.ReadInt32());
        }
        public override bool Expand(IDataRepository Repo)
        {
            try
            {
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
