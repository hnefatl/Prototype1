using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.ComponentModel.DataAnnotations.Schema;
using System.Windows.Media;

using Shared;

namespace Data.Models
{
    // Contains all subjects (eg Maths, Computing)
    [Table("Subjects")]
    public class Subject
        : DataModel
    {
        // Friendly name of the subject (Maths, Computing)
        public string SubjectName { get; set; }

        // Store the integer equivalent of the colour
        public int Argb
        {
            get { return Helpers.ColorToInt(Colour); }
            set { Colour = Helpers.IntToColour(value); }
        }
        
        // Colour used to display bookings of this subject on the timetable
        [NotMapped]
        public Color Colour { get; set; }
        
        // Bookings of this subject
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

        // Write properties and IDs
        public override void Serialise(Writer Out)
        {
            base.Serialise(Out);

            Out.Write(SubjectName);
            Out.Write(Argb);
            Out.Write(Bookings.Count);
            Bookings.ForEach(b => Out.Write(b.Id));
        }
        // Read properties and IDs
        protected override void Deserialise(Reader In)
        {
            base.Deserialise(In);

            SubjectName = In.ReadString();
            Argb = In.ReadInt32();
            Bookings = Enumerable.Repeat(new Booking(), In.ReadInt32()).ToList();
            Bookings.ForEach(b => b.Id = In.ReadInt32());
        }
        // Obtain references to related entities
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
        // Set references to this object
        public override void Attach()
        {
            Bookings.ForEach(b => b.Subject = this);
        }
        // Remove references to this object
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
