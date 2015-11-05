using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using Shared;

namespace Client
{
    //////////////////////////////////////////////////////////////////////////////////////////////
    // TODO: Ask client how to store client settings - file? Command-Line arguments on startup? //
    //////////////////////////////////////////////////////////////////////////////////////////////

    public sealed class Settings
    {
        public static string Path { get { return "Settings.txt"; } }

        private static Dictionary<string, object> Inner { get; set; }

        // Static indexers don't work :(
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
            Inner[Setting] = Value;
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
                //Inner.Add("ServerAddress", "10.105.35.97");
                //Inner.Add("ServerPort", 34652);

                using (FileStream File = new FileStream(Path, FileMode.Open))
                {
                    using (Reader Reader = GetReader(File))
                    {
                        while (File.Position != File.Length)
                            Inner.Add(Reader.ReadString(), Reader.ReadString());
                    }
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
                using (FileStream File = new FileStream(Path, FileMode.Create))
                {
                    using (Writer Writer = GetWriter(File))
                    {
                        foreach (KeyValuePair<string, object> Setting in Inner)
                        {
                            Writer.Write(Setting.Key);
                            Writer.Write(Setting.Value);
                        }
                    }
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        private static Reader GetReader(Stream Base)
        {
#if DEBUG
            return new Shared.TextReader(Base);
#else
            return new NetCore.NetReader(Base);
#endif
        }
        private static Writer GetWriter(Stream Base)
        {
#if DEBUG
            return new Shared.TextWriter(Base);
#else
            return new NetCore.NetWriter(Base);
#endif
        }
    }
}
