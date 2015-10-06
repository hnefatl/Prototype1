using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Client
{
    //////////////////////////////////////////////////////////////////////////////////////////////
    // TODO: Ask client how to store client settings - file? Command-Line arguments on startup? //
    //////////////////////////////////////////////////////////////////////////////////////////////

    public sealed class Settings
    {
        public static string Path { get { return "Settings.txt"; } }

        private static Dictionary<string, object> Inner { get; set; }

        // Static indexers don't work -.-
        public static object Get(string Setting)
        {
            return Inner[Setting];
        }
        public static T Get<T>(string Setting)
        {
            return (T)Convert.ChangeType(Get(Setting), typeof(T));
        }
        public static void Set(string Setting, object Value)
        {
            if (!Value.GetType().IsSerializable)
                throw new SerializationException("Parameter Value of type " + Value.GetType().ToString() + " is not serialisable.");

            Inner[Setting] = Value;
        }
        public static void Set<T>(string Setting, T Value)
        {
            Set(Setting, Value);
        }

        public static void Add(string Setting, object Value)
        {
            Inner.Add(Setting, Value);
        }
        public static void Remove(string Setting)
        {
            Inner.Remove(Setting);
        }
        public static void Clear()
        {
            Inner.Clear();
        }

        public static bool Contains(string Setting)
        {
            return Inner.ContainsKey(Setting);
        }

        public static bool Load()
        {
            try
            {
                Inner = new Dictionary<string, object>();
                BinaryFormatter Reader = new BinaryFormatter();
                using (FileStream File = new FileStream(Path, FileMode.Open))
                {
                    while (File.Position != File.Length)
                        Inner.Add(Convert.ToString(Reader.Deserialize(File)), Reader.Deserialize(File));
                }
            }
            catch
            {
                return false;
            }
            return true;
        }
        public static bool Save()
        {
            try
            {
                BinaryFormatter Writer = new BinaryFormatter();
                using (FileStream File = new FileStream(Path, FileMode.Create))
                {
                    foreach (KeyValuePair<string, object> Setting in Inner)
                    {
                        Writer.Serialize(File, Setting.Key);
                        Writer.Serialize(File, Setting.Value);
                    }
                }
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}
