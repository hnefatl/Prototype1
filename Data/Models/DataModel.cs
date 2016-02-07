using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Shared;

namespace Data.Models
{
    public abstract class DataModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // Returns true if the current object is already contained in the given list
        public abstract bool Conflicts(List<DataModel> Others);

        // Serialises the object to a Writer
        public virtual void Serialise(Writer Out)
        {
            Out.Write(GetType().FullName);
            Out.Write(Id);
        }
        // Deserialises the object from a Reader
        protected virtual void Deserialise(Reader In)
        {
            Id = In.ReadInt32();
        }

        // Edits this object's properties to reflect the argument's
        public abstract void Update(DataModel Other);

        // Acquires references to all related entities in the database from a repository
        public abstract bool Expand(IDataRepository Repo);
        // Adds references to this item to all related entities in the database (flipside to Expand)
        public abstract void Attach();
        // Removes references to this item from related entities in the database (before deletion)
        public abstract void Detach();

        // Deserialise an unknown type of DataModel from a Reader
        public static DataModel DeserialiseExternal(Reader In)
        {
            string TypeName = In.ReadString(); // Read the full type name of the sent object

            // Test all subclasses of DataModel against the type name
            foreach (Type t in Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsSubclassOf(typeof(DataModel))))
            {
                if (t.FullName == TypeName)
                {
                    // Create the object, let it deserialise from the stream, then return it
                    DataModel m = (DataModel)Activator.CreateInstance(t);
                    m.Deserialise(In);
                    return m;
                }
            }

            // If no matching type found, it's possible a client is using an out-of-date assembly
            throw new Exception("Failed to find type name - suggests out of date Data.dll");
        }

        // Deserialise a compile-time-known type of DataModel
        public static T DeserialiseExternal<T>(Reader In) where T : DataModel
        {
            try
            {
                // If T is abstract, we can’t use this method, so use the first one
                if (typeof(T).IsAbstract)
                    return (T)DeserialiseExternal(In);

                string TypeName = In.ReadString(); // Read the type name but ignore it

                // Create the object, deserialise and return
                T Result = Activator.CreateInstance<T>();
                Result.Deserialise(In);
                return Result;
            }
            catch
            {
                // If something went wrong, then deserialisation failed
                throw new Exception("Received type isn't of type expected.");
            }
        }
    }
}
