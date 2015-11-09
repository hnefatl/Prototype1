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
using System.Timers;

using NetCore.Client;
using NetCore.Messages;
using Data.Models;
using Data;
using Client.Admin;

namespace Client
{
    public partial class TrayIcon
        : Window
    {
        public Connection Connection { get; protected set; }
        public User CurrentUser { get; protected set; }
        public Room CurrentRoom { get; protected set; }

        protected System.Windows.Forms.NotifyIcon ToolbarIcon { get; set; }
        protected System.Windows.Forms.ContextMenu Menu { get; set; }

        protected MainWindow MainWindow { get; set; }
        protected bool MainWindowShown { get; set; }

        protected AdminWindow AdminWindow { get; set; }
        protected bool AdminWindowShown { get; set; }

        protected Timer Timer { get; set; }
        protected TimeSlot LastSlot { get; set; }
        protected const int MessageDuration = 5000;

        public TrayIcon(Connection Connection, User CurrentUser, Room CurrentRoom)
        {
            InitializeComponent();

            this.Connection = Connection;
            this.CurrentUser = CurrentUser;
            this.CurrentRoom = CurrentRoom;

            Connection.Disconnect += Connection_Disconnect;

            MainWindowShown = false;
            AdminWindowShown = false;

            ToolbarIcon = new System.Windows.Forms.NotifyIcon();
            ToolbarIcon.MouseClick += ToolbarIcon_Click;
            ToolbarIcon.Icon = Properties.Resources.ToolbarIcon;
            ToolbarIcon.Visible = true;

            Menu = new System.Windows.Forms.ContextMenu();
            if (CurrentUser.Access == AccessMode.Teacher)
            {
                Menu.MenuItems.Add(new System.Windows.Forms.MenuItem("View Bookings", (s, e) => ToolbarIcon_Click(s, null)));

                if (CurrentUser.Access == AccessMode.Admin)
                {
                    Menu.MenuItems.Add(new System.Windows.Forms.MenuItem("Customise system", (s, e) => ShowAdminWindow()));
                }

                Menu.MenuItems.Add(new System.Windows.Forms.MenuItem("Exit", ExitClick));
                ToolbarIcon.ContextMenu = Menu;
            }

            Timer = new Timer(TimeSpan.FromSeconds(30).TotalMilliseconds); // Every 30 seconds, fire an event
            Timer.Elapsed += Timer_Elapsed;
            Timer.Start();

            Timer_Elapsed(null, null); // Fire the timer event immediately
        }
        protected override void OnClosed(EventArgs e)
        {
            Timer.Stop();
            Timer.Dispose();

            ToolbarIcon.Visible = false;
            ToolbarIcon.Icon.Dispose();
            ToolbarIcon.Icon = null; // Actually hides the icon, otherwise it lingers for a bit
            Menu.Dispose();

            base.OnClosed(e);
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            DataSnapshot Frame = DataRepository.TakeSnapshot();
            TimeSlot CurrentSlot = Frame.Periods.SingleOrDefault(t => t.IsCurrent(DateTime.Now));
            if (CurrentSlot == null)
                return;

            Booking Booking = Frame.Bookings.SingleOrDefault(b => b.MatchesDay(DateTime.Now.Date) && b.TimeSlot == CurrentSlot && b.Rooms.Contains(CurrentRoom));
            if (Booking != null)
            {
                if (LastSlot == null || LastSlot != CurrentSlot)
                {
                    LastSlot = CurrentSlot;
                    ToolbarIcon.ShowBalloonTip(MessageDuration, "Scheduled booking", "A lesson is taking place in this room this period (" + CurrentSlot.Name + ").\n" +
                        "Teacher: " + Booking.Teacher.FormalName + "\n" +
                        "Subject: " + Booking.Subject.SubjectName, System.Windows.Forms.ToolTipIcon.Info);
                }
            }
        }

        private void ToolbarIcon_Click(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e == null || (e != null && e.Button != System.Windows.Forms.MouseButtons.Right))
                ShowMainWindow();
        }
        private void ShowMainWindow()
        {
            if (MainWindow != null && !MainWindow.Dispatcher.CheckAccess())
                MainWindow.Dispatcher.BeginInvoke((Action)ShowMainWindow);
            else
            {
                if (!MainWindowShown)
                {
                    MainWindowShown = true;
                    MainWindow = new MainWindow(Connection, CurrentUser);
                    // Something really weird happens here - calling the MainWindow constructor causes the click event to be run again.
                    // Stack Trace shows it comes direct from the NotifyIcon itself, not from any accidental callbacks :/
                    // Some logic and flags avoids the issue, as the root cause seems to be threads updating when the new window is shown and therefore unavoidable.

                    MainWindow.Closed += (s, o) => MainWindowShown = false;
                    MainWindow.Show();
                }
                else
                    MainWindow.Activate();
            }
        }
        private void ShowAdminWindow()
        {
            if (AdminWindow != null && !AdminWindow.Dispatcher.CheckAccess())
                AdminWindow.Dispatcher.BeginInvoke((Action)ShowAdminWindow);
            else
            {
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
        }

        private void ExitClick(object sender, EventArgs e)
        {
            Connection.Close(DisconnectType.Expected);
            Environment.Exit(0);
        }

        private void Connection_Disconnect(Connection Sender, NetCore.Messages.DisconnectMessage Message)
        {
            if (MainWindowShown)
                MainWindow.Dispatcher.Invoke((Action)Close);
            if (AdminWindowShown)
                AdminWindow.Dispatcher.Invoke((Action)Close);
        }

        public new void Show()
        {
            base.Show();
            ToolbarIcon.Visible = true;
        }
        public new void Hide()
        {
            base.Hide();
            ToolbarIcon.Visible = false;
        }
    }
}
