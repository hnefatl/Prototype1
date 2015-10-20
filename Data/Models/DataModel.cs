using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Shared;
using Data;

namespace Data.Models
{
    public abstract class DataModel
        : IExpandsData
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        public virtual void Serialise(IWriter Out)
        {
            Out.Write(GetType().FullName);
            Out.Write(Id);
        }
        protected virtual void Deserialise(IReader In)
        {
            Id = In.ReadInt32();
        }

        public abstract bool Expand(IDataRepository Repo);

        public static DataModel DeserialiseExternal(IReader In)
        {
            string TypeName = In.ReadString();

            foreach(Type t in Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsSubclassOf(typeof(DataModel))))
            {
                if(t.FullName == TypeName)
                {
                    DataModel m = (DataModel)Activator.CreateInstance(t);
                    m.Deserialise(In);
                    return m;
                }
            }
            return null;
        }
    }
}
