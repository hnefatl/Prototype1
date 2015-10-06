using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace Client
{
    public partial class App
        : Application
    {
        public App() // Startup code
        {
            if (!Settings.Load())
                Environment.Exit(-1); // Fatal error

            Settings.Clear();
            Settings.Add("ServerAddress", "127.0.0.1");
            Settings.Add("ServerPort", "34652");
            Settings.Save();
        }
    }
}
