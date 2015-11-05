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

        public abstract bool Conflicts(List<DataModel> Others);

        public virtual void Serialise(Writer Out)
        {
            Out.Write(GetType().FullName);
            Out.Write(Id);
        }
        protected virtual void Deserialise(Reader In)
        {
            Id = In.ReadInt32();
        }

        public abstract void Update(DataModel Other);

        public abstract bool Expand(IDataRepository Repo);
        public abstract void Attach();
        public abstract void Detach();

        public static DataModel DeserialiseExternal(Reader In)
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
