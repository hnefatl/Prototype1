using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Shared;
using Data;

namespace Data.Models
{
    public abstract class DataModel
        : ISerialisable, IExpandsData
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public abstract void Serialise(IWriter Out);
        public abstract void Deserialise(IReader In);

        public abstract bool Expand(IDataRepository Repo);
    }
}
