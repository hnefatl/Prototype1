using System;
using System.IO;

namespace Server
{
    public sealed class Settings
    {
        // Default path to the database file
        public static string DatabasePath { get { return _DatabasePath; } private set { _DatabasePath = value; } }
        private static string _DatabasePath = Environment.CurrentDirectory + "\\Data.mdf";

        // Default Port to listen on
        public static int Port { get { return _Port; } private set { _Port = value; } }
        private static int _Port = 34652;

        // Default path to the Settings file
        public static string Path { get { return _Path; } set { _Path = value; } }
        private static string _Path = "Settings.txt";

        // Loads in the settings from the file
        public static void Load()
        {
            try
            {
                using (Shared.TextReader In = new Shared.TextReader(File.OpenRead(Path)))
                {
                    // Read a key on one line
                    string Key = In.ReadString();

                    // Set the appropriate variable depending on the key
                    if (Key == "DatabasePath")
                        DatabasePath = In.ReadString();
                    else if (Key == "Port")
                        Port = In.ReadInt32();
                }
            }
            catch { }
        }
        // Save the existing settings to the file
        public static bool Save()
        {
            try
            {
                using (Shared.TextWriter Out = new Shared.TextWriter(File.OpenWrite(Path)))
                {
                    // Output all the variables
                    Out.Write("DatabasePath");
                    Out.Write(DatabasePath);
                    Out.Write("Port");
                    Out.Write(Port);
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
