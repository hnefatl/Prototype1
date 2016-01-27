using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Server
{
    public sealed class Settings
    {
        public static string DatabasePath { get { return _DatabasePath; } private set { _DatabasePath = value; } }
        private static string _DatabasePath = Environment.CurrentDirectory + "\\Data.mdf";

        public static int Port { get { return _Port; } private set { _Port = value; } }
        private static int _Port = 34652;

        public static string Path { get { return _Path; } set { _Path = value; } }
        private static string _Path = "Settings.txt";

        public static void Load()
        {
            try
            {
                using (Shared.TextReader In = new Shared.TextReader(File.OpenRead(Path)))
                {
                    string Key = In.ReadString();

                    if (Key == "DatabasePath")
                        DatabasePath = In.ReadString();
                    else if (Key == "Port")
                        Port = In.ReadInt32();
                }
            }
            catch { }
        }
        public static bool Save()
        {
            try
            {
                using (Shared.TextWriter Out = new Shared.TextWriter(File.OpenWrite(Path)))
                {
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
