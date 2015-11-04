using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Windows.Media;

using Shared;

namespace Data.Models
{
    /// <summary>
    /// Contains all subjects (eg Maths, Computing)
    /// </summary>
    [Table("Subjects")]
    public class Subject
        : DataModel
    {
        /// <summary>
        /// Friendly name of the subject (Maths, Computing)
        /// </summary>
        public string SubjectName { get; set; }

        public int Argb
        {
            get
            {
                return Helpers.ColorToInt(Colour);
            }
            set
            {
                Colour = Helpers.IntToColour(value);
            }
        }

        /// <summary>
        /// Colour used to display bookings of this subject on the timetable
        /// </summary>
        [NotMapped]
        public Color Colour { get; set; }

        /// <summary>
        /// Bookings of this subject
        /// </summary>
        public virtual List<Booking> Bookings { get; set; }

        public Subject()
        {
            Bookings = new List<Booking>();

            SubjectName = string.Empty;
        }

        public override bool Conflicts(List<DataModel> Others)
        {
            return Others.Cast<Subject>().Any(s => s.Id != Id && s.SubjectName == SubjectName);
        }

        public override void Update(DataModel Other)
        {
            Subject s = (Subject)Other;
            SubjectName = s.SubjectName;
            Argb = s.Argb;
            Bookings.Clear();
            Bookings.AddRange(s.Bookings);
        }

        public override void Serialise(IWriter Out)
        {
            base.Serialise(Out);

            Out.Write(SubjectName);
            Out.Write(Argb);
            Out.Write(Bookings.Count);
            Bookings.ForEach(b => Out.Write(b.Id));
        }
        protected override void Deserialise(IReader In)
        {
            base.Deserialise(In);

            SubjectName = In.ReadString();
            Argb = In.ReadInt32();
            Bookings = Enumerable.Repeat(new Booking(), In.ReadInt32()).ToList();
            Bookings.ForEach(b => b.Id = In.ReadInt32());
        }
        public override bool Expand(IDataRepository Repo)
        {
            try
            {
                for (int x = 0; x < Bookings.Count; x++)
                    Bookings[x] = Repo.Bookings.SingleOrDefault(b => b.Id == Bookings[x].Id);
            }
            catch
            {
                return false;
            }
            return true;
        }
        public override void Attach()
        {
            Bookings.ForEach(b => b.Subject = this);
        }
        public override void Detach()
        {
            Bookings.ForEach(b => { if (b != null) b.Subject = null; });
        }

        public override string ToString()
        {
            return SubjectName;
        }
    }
}
