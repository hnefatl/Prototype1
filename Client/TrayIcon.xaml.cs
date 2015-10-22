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
            ToolbarIcon.MouseClick += ToolbarIcon_Click;
            ToolbarIcon.Icon = Properties.Resources.ToolbarIcon;
            ToolbarIcon.Visible = true;

            if (CurrentUser.Access == AccessMode.Admin)
            {
                Menu = new System.Windows.Forms.ContextMenu();
                Menu.MenuItems.Add(new System.Windows.Forms.MenuItem("Standard View", (s, e) => ToolbarIcon_Click(s, null)));
                Menu.MenuItems.Add(new System.Windows.Forms.MenuItem("Admin View", (s, e) => ShowAdminWindow()));
                Menu.MenuItems.Add(new System.Windows.Forms.MenuItem("Exit", ExitClick));
                ToolbarIcon.ContextMenu = Menu;
            }
        }
        protected override void OnClosed(EventArgs e)
        {
            ToolbarIcon.Visible = false;
            Menu.Dispose();
            ToolbarIcon.Dispose();

            base.OnClosed(e);
        }


        private void ToolbarIcon_Click(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e == null || (e != null && e.Button != System.Windows.Forms.MouseButtons.Right))
                ShowMainWindow();
        }
        private void ShowMainWindow()
        {
            if (MainWindow != null && !MainWindow.Dispatcher.CheckAccess())
                MainWindow.Dispatcher.Invoke((Action)ShowMainWindow);

            if (!MainWindowShown)
            {
                MainWindowShown = true;
                MainWindow = new MainWindow(Connection, CurrentUser);
                // Something really weird happens here - calling the MainWindow constructor causes the click event to be run again.
                // Stack Trace shows it comes direct from the NotifyIcon itself, not from any accidental callbacks :/
                // Some crazy logic and flags avoids the issue, as the root cause seems to be threads updating when the new window is shown and therefore unavoidable.

                MainWindow.Closed += (s, o) => MainWindowShown = false;
                MainWindow.Show();
            }
            else
                MainWindow.Activate();
        }
        private void ShowAdminWindow()
        {
            if (AdminWindow != null && !AdminWindow.Dispatcher.CheckAccess())
                AdminWindow.Dispatcher.Invoke((Action)ShowAdminWindow);

            if (!AdminWindowShown)
            {
                AdminWindow = new AdminWindow(Connection, CurrentUser);
                AdminWindow.Closed += (s, o) => AdminWindowShown = false;
                AdminWindow.Show();
                AdminWindowShown = true;
            }
            else
                AdminWindow.Activate();
        }

        private void ExitClick(object sender, EventArgs e)
        {
            Close();
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
