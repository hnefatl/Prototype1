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
        :ISerialisable, IExpandsData
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string ClassName { get; set; }

        public virtual List<Student> Students { get; set; }

        public Class()
        {
            Students = new List<Student>();
            ClassName = string.Empty;
        }

        public void Serialise(IWriter Out)
        {
            Out.Write(Id);
            Out.Write(ClassName);
            Students.ForEach(s => Out.Write(s.Id));
        }
        public void Deserialise(IReader In)
        {
            Id = In.ReadInt32();
            ClassName = In.ReadString();
            Students = Enumerable.Repeat(new Student(), In.ReadInt32()).ToList();
            Students.ForEach(s => s.Id = In.ReadInt32());
        }
        public bool Expand(IDataRepository Repo)
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
