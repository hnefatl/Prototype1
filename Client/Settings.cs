using System;
using System.Collections.Generic;
using System.IO;

namespace Client
{
    public sealed class Settings
    {
        // Default path to the Settings.txt file
        private static string _Path = "Settings.txt";
        public static string Path { get { return _Path; } set { _Path = value; } }

        private static Dictionary<string, object> Inner { get; set; }
        
        // Returns the value of a setting given the key
        public static object Get(string Setting)
        {
            return Inner[Setting];
        }
        // Returns a strongly typed value of a setting given the key
        public static T Get<T>(string Setting)
        {
            return (T)Convert.ChangeType(Get(Setting), typeof(T));
        }

        // Sets the value mapped to by a key
        public static void Set(string Setting, object Value)
        {
            Inner[Setting] = Value;
        }

        // Adds a new key-value mapping
        public static void Add(string Setting, object Value)
        {
            Inner.Add(Setting, Value);
        }
        // Remove an existing key-value mapping
        public static void Remove(string Setting)
        {
            Inner.Remove(Setting);
        }
        // Remove all mappings
        public static void Clear()
        {
            Inner.Clear();
        }

        // Checks if the key exists already
        public static bool Contains(string Setting)
        {
            return Inner.ContainsKey(Setting);
        }

        // Loads in the settings from the file, returning success/failure
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
        // Save the settings to the file, returning success/failure
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
