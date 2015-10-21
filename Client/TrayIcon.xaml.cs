using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using NetCore.Client;
using Data.Models;

using Client.Admin;

namespace Client
{
    public partial class TrayIcon
        : Window
    {
        public Connection Connection { get; protected set; }
        public User CurrentUser { get; protected set; }

        protected System.Windows.Forms.NotifyIcon ToolbarIcon { get; set; }
        protected System.Windows.Forms.ContextMenu Menu { get; set; }

        protected MainWindow MainWindow { get; set; }
        protected bool MainWindowShown { get; set; }

        protected AdminWindow AdminWindow { get; set; }
        protected bool AdminWindowShown { get; set; }

        public TrayIcon(Connection Connection, User CurrentUser)
        {
            InitializeComponent();

            this.Connection = Connection;
            this.CurrentUser = CurrentUser;

            Connection.Disconnect += Connection_Disconnect;

            MainWindowShown = false;
            AdminWindowShown = false;

            ToolbarIcon = new System.Windows.Forms.NotifyIcon();
            ToolbarIcon.Click += ToolbarIcon_Click;
            ToolbarIcon.Icon = Properties.Resources.ToolbarIcon;
            ToolbarIcon.Visible = true;

            if (CurrentUser.Access == AccessMode.Admin)
            {
                Menu = new System.Windows.Forms.ContextMenu();
                Menu.MenuItems.Add(new System.Windows.Forms.MenuItem("Standard View", ToolbarIcon_Click));
                Menu.MenuItems.Add(new System.Windows.Forms.MenuItem("Admin View", ShowAdminWindow));
                ToolbarIcon.ContextMenu = Menu;
            }
        }

        private void ToolbarIcon_Click(object sender, EventArgs e)
        {
            if (MainWindow != null && !MainWindow.Dispatcher.CheckAccess())
                MainWindow.Dispatcher.Invoke((Action<object, EventArgs>)ToolbarIcon_Click, sender, e);

            if (!MainWindowShown)
            {
                MainWindow = new MainWindow(Connection, CurrentUser);
                MainWindow.Closed += (s, o) => MainWindowShown = false;
                MainWindow.Show();
            }
            else
                MainWindow.Show();
        }
        private void ShowAdminWindow(object sender, EventArgs e)
        {
            if (AdminWindow != null && !AdminWindow.Dispatcher.CheckAccess())
                AdminWindow.Dispatcher.Invoke((Action<object, EventArgs>)ShowAdminWindow, sender, e);

            if (!AdminWindowShown)
            {
                AdminWindow = new AdminWindow(Connection, CurrentUser);
                AdminWindow.Closed += (s, o) => AdminWindowShown = false;
                AdminWindow.Show();
            }
            else
                AdminWindow.Show();
        }
        
        private void Connection_Disconnect(Connection Sender, NetCore.Messages.DisconnectMessage Message)
        {
            if (MainWindowShown)
                MainWindow.Dispatcher.Invoke((Action)Close);
            if (AdminWindowShown)
                AdminWindow.Dispatcher.Invoke((Action)Close);
        }
    }
}
