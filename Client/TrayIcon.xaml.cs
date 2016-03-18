using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
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
        // Reference to the connection to the Server
        public Connection Connection { get; protected set; }
        // Reference to the current user logged on
        public User CurrentUser { get; protected set; }
        // Reference to the current room
        public Room CurrentRoom { get; protected set; }

        // The Winforms icon, this class is a wrapper around it
        protected System.Windows.Forms.NotifyIcon ToolbarIcon { get; set; }
        // The Winforms context menu attached to the icon
        protected System.Windows.Forms.ContextMenu Menu { get; set; }

        // The timetable window, only one allowed to be opened
        protected MainWindow MainWindow { get; set; }
        protected bool MainWindowShown { get; set; }

        // The Admin window, only one allowed to be opened
        protected AdminWindow AdminWindow { get; set; }
        protected bool AdminWindowShown { get; set; }

        // Timer used for booking checks
        protected Timer Timer { get; set; }
        // Timeslot that the program was running during in the last timer update
        protected TimeSlot LastSlot { get; set; }
        // How long the balloon message will linger for
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

            // Construct the context menu so that it contains relevant options for the user

            Menu = new System.Windows.Forms.ContextMenu();
            Menu.MenuItems.Add(new System.Windows.Forms.MenuItem("View Bookings", (s, e) => ToolbarIcon_Click(s, null)));

            // Admins can customise
            if (CurrentUser.Access == AccessMode.Admin)
                Menu.MenuItems.Add(new System.Windows.Forms.MenuItem("Customise system", (s, e) => ShowAdminWindow()));

            // Admins and teachers can exit
            if (CurrentUser.Access == AccessMode.Admin || CurrentUser.Access == AccessMode.Teacher)
                Menu.MenuItems.Add(new System.Windows.Forms.MenuItem("Exit", ExitClick));
            ToolbarIcon.ContextMenu = Menu;

            // Every 30 seconds, fire an event
            Timer = new Timer(TimeSpan.FromSeconds(30).TotalMilliseconds);
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

        // Displays a balloon message
        public void ShowBalloon(string Title, string Message, System.Windows.Forms.ToolTipIcon Icon)
        {
            ToolbarIcon.ShowBalloonTip(MessageDuration, Title, Message, Icon);
        }

        // Every 30 seconds evaluate the bookings to see if a message needs displaying
        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            DataSnapshot Frame = DataRepository.TakeSnapshot();
            // Get the current timeslot
            TimeSlot CurrentSlot = Frame.Periods.SingleOrDefault(t => t.IsCurrent(DateTime.Now));
            if (CurrentSlot == null)
                return;

            // Get any matching bookings
            Booking Booking = Frame.Bookings.SingleOrDefault(b => b.MatchesDay(DateTime.Now.Date) && b.TimeSlot == CurrentSlot && b.Rooms.Contains(CurrentRoom));
            if (Booking != null)
            {
                // If we haven't already shown this message
                if (LastSlot == null || LastSlot != CurrentSlot)
                {
                    // Display the message
                    LastSlot = CurrentSlot;
                    ToolbarIcon.Visible = true;
                    ToolbarIcon.ShowBalloonTip(MessageDuration, "Scheduled booking", "A lesson is taking place in this room this period (" + CurrentSlot.Name + ").\n" +
                        "Teacher: " + Booking.Teacher.FormalName + "\n" +
                        "Subject: " + Booking.Subject.SubjectName, System.Windows.Forms.ToolTipIcon.Info);
                }
            }
        }

        // Open the window if clicked
        private void ToolbarIcon_Click(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e == null || (e != null && e.Button != System.Windows.Forms.MouseButtons.Right))
                ShowMainWindow();
        }
        // Handle opening the window if it's closed, or bring it to the front if it's hidden
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
        // Same as ShowMainWindow but for the AdminWindow
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

        // Send a D/C message on exiting
        private void ExitClick(object sender, EventArgs e)
        {
            Connection.Close(DisconnectType.Expected);
            Environment.Exit(0);
        }

        // Close all windows if the server disconnects
        private void Connection_Disconnect(Connection Sender, NetCore.Messages.DisconnectMessage Message)
        {
            if (MainWindowShown)
                MainWindow.Dispatcher.Invoke((Action)Close);
            if (AdminWindowShown)
                AdminWindow.Dispatcher.Invoke((Action)Close);
        }

        // Replace the existing show method with one that shows the icon
        public new void Show()
        {
            if (!Dispatcher.CheckAccess())
                Dispatcher.Invoke((Action)Show);
            else
            {
                ToolbarIcon.Visible = true;
            }
        }
        // Hides the icon, opposite of Show
        public new void Hide()
        {
            if (!Dispatcher.CheckAccess())
                Dispatcher.Invoke((Action)Show);
            else
            {
                ToolbarIcon.Visible = false;
            }
        }
    }
}
