using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.DataContext.Models
{
    [Table("Configs")]
    public class Config
    {
        [Key]
        public string Key { get; set; }
        public string Value { get; set; }

        public override string ToString()
        {
            return Key + ": " + Value;
        }
    }
}
