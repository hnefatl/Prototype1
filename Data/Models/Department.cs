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
        public virtual List<Room> Rooms { get; set; }

        public Department()
        {
            Teachers = new List<Teacher>();
            Rooms = new List<Room>();

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
            Rooms = d.Rooms;
        }

        public override void Serialise(IWriter Out)
        {
            base.Serialise(Out);

            Out.Write(Name);
            Out.Write(Teachers.Count);
            Teachers.ForEach(t => Out.Write(t.Id));
            Out.Write(Rooms.Count);
            Rooms.ForEach(r => Out.Write(r.Id));
        }
        protected override void Deserialise(IReader In)
        {
            base.Deserialise(In);

            Name = In.ReadString();
            Teachers = Enumerable.Repeat(new Teacher(), In.ReadInt32()).ToList();
            Teachers.ForEach(t => t.Id = In.ReadInt32());
            Rooms = Enumerable.Repeat(new Room(), In.ReadInt32()).ToList();
            Rooms.ForEach(r => r.Id = In.ReadInt32());
        }
        public override bool Expand(IDataRepository Repo)
        {
            try
            {
                for (int x = 0; x < Teachers.Count; x++)
                    Teachers[x] = Repo.Users.OfType<Teacher>().Single(t => t.Id == Teachers[x].Id);
                for (int x = 0; x < Rooms.Count; x++)
                    Rooms[x] = Repo.Rooms.Single(r => r.Id == Rooms[x].Id);
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
