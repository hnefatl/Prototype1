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
    public sealed class Settings
    {
        private static string _Path = "Settings.txt";
        public static string Path { get { return _Path; } set { _Path = value; } }

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

                using (Shared.TextReader In = new Shared.TextReader(File.OpenRead(Path)))
                {
                    while (true)
                    {
                        string Key = In.ReadString();
                        string Value = In.ReadString();
                        if (Key == null || Value == null)
                            break;

                        Inner.Add(Key, Value);
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
                using (Shared.TextWriter Out = new Shared.TextWriter(File.OpenWrite(Path)))
                {
                    foreach (KeyValuePair<string, object> Setting in Inner)
                    {
                        Out.Write(Setting.Key);
                        Out.Write(Setting.Value);
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
