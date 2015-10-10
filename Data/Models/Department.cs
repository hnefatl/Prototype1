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
        : ISerialisable, IExpandsData
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Name { get; set; }

        public virtual List<Teacher> Teachers { get; set; }

        public Department()
        {
            Teachers = new List<Teacher>();

            Name = string.Empty;
        }

        public void Serialise(IWriter Out)
        {
            Out.Write(Id);
            Out.Write(Name);
            Out.Write(Teachers.Count);
            Teachers.ForEach(t => Out.Write(t.Id));
        }
        public void Deserialise(IReader In)
        {
            Id = In.ReadInt32();
            Name = In.ReadString();
            Teachers = Enumerable.Repeat(new Teacher(), In.ReadInt32()).ToList();
            Teachers.ForEach(t => t.Id = In.ReadInt32());
        }
        public bool Expand(IDataRepository Repo)
        {
            try
            {
                Teachers.ForEach(t => t = Repo.Teachers.Where(t2 => t.Id == t2.Id).Single());
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
