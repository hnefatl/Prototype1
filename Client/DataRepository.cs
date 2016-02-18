using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

using NetCore.Client;
using NetCore.Messages;
using NetCore.Messages.DataMessages;
using Data;
using Data.Models;

namespace Client
{
    // Delegate function signatures for the events described below
    public delegate void DataChangedHandler(Type ChangedType);

    // Holds a copy of the data in the database that's synchronised with the server
    public class DataRepository
        : IDisposable, IDataRepository
    {
        // Reference to the connection to the server used
        private static Connection Server { get; set; }

        // List of all the Bookings
        List<Booking> IDataRepository.Bookings { get { return Bookings.ToList(); } }
        private static ObservableCollection<Booking> _Bookings = new ObservableCollection<Booking>();
        public ObservableCollection<Booking> Bookings
        {
            get { return _Bookings; }
            set { _Bookings = value; }
        }

        // All Departments
        List<Department> IDataRepository.Departments { get { return Departments.ToList(); } }
        private static ObservableCollection<Department> _Departments = new ObservableCollection<Department>();
        public ObservableCollection<Department> Departments
        {
            get { return _Departments; }
            set { _Departments = value; }
        }

        List<Room> IDataRepository.Rooms { get { return Rooms.ToList(); } }
        private static ObservableCollection<Room> _Rooms = new ObservableCollection<Room>();
        public ObservableCollection<Room> Rooms
        {
            get { return _Rooms; }
            set { _Rooms = value; }
        }

        List<User> IDataRepository.Users { get { return Users.ToList(); } }
        private static ObservableCollection<User> _Users = new ObservableCollection<User>();
        public ObservableCollection<User> Users
        {
            get { return _Users; }
            set { _Users = value; }
        }

        List<Subject> IDataRepository.Subjects { get { return Subjects.ToList(); } }
        private static ObservableCollection<Subject> _Subjects = new ObservableCollection<Subject>();
        public ObservableCollection<Subject> Subjects
        {
            get { return _Subjects; }
            set { _Subjects = value; }
        }

        List<TimeSlot> IDataRepository.Periods { get { return Periods.ToList(); } }
        private static ObservableCollection<TimeSlot> _Periods = new ObservableCollection<TimeSlot>();
        public ObservableCollection<TimeSlot> Periods
        {
            get { return _Periods; }
            set { _Periods = value; }
        }

        List<Class> IDataRepository.Classes { get { return Classes.ToList(); } }
        private static ObservableCollection<Class> _Classes = new ObservableCollection<Class>();
        public ObservableCollection<Class> Classes
        {
            get { return _Classes; }
            set { _Classes = value; }
        }

        // Holds references to the tables by loosely typed IList collections.
        // Reduces code bloat later on
        private static readonly Dictionary<Type, IList> Tables = new Dictionary<Type, IList>()
        {
            { typeof(Booking), _Bookings },
            { typeof(Department), _Departments },
            { typeof(Room), _Rooms },
            { typeof(User), _Users },
            { typeof(Subject), _Subjects },
            { typeof(TimeSlot), _Periods },
            { typeof(Class), _Classes },
        };

        // Internal locking object for thread safety
        private static object Lock = new object();

        // Whether to inform the server of model changes
        private static bool ReportModelChanges { get; set; }

        // Signals between threads indicating the completion of certain tasks
        private static ManualResetEvent InitialisedEvent { get; set; }
        private static ManualResetEvent UserEvent { get; set; }

        // Reference to the current User and Room, set when an approriate message is received
        private static User CurrentUser { get; set; }
        private static Room CurrentRoom { get; set; }

        // Fired when any colelction of items is changed
        public static event DataChangedHandler DataChanged = delegate { };

        // Whether this instance will check for thread safety before accessing the data
        private bool LockData;

        public DataRepository(bool LockData = true)
        {
            // Optionally allow one DataRepository to be instantiated at a time.
            // Block until all other ones are Disposed.
            this.LockData = LockData;
            if (LockData)
                Monitor.Enter(Lock);
        }

        // Initialise the database model from the server
        public static Tuple<User, Room> Initialise(Connection Server, ConnectMessage Msg)
        {
            try
            {
                // Thread safe
                Monitor.Enter(Lock);

                // Reset the signal that's set when the data's received
                if (InitialisedEvent == null)
                    InitialisedEvent = new ManualResetEvent(false);
                InitialisedEvent.Reset();

                // Reset the signal that's set when the user information's received
                if (UserEvent == null)
                    UserEvent = new ManualResetEvent(false);
                UserEvent.Reset();

                // Set the current server
                DataRepository.Server = Server;

                // Hook up network events
                Server.MessageReceived += MessageReceived;
                Server.Disconnect += Disconnected;

                // Hook up data changed events
                _Bookings.CollectionChanged += Data_CollectionChanged;
                _Departments.CollectionChanged += Data_CollectionChanged;
                _Rooms.CollectionChanged += Data_CollectionChanged;
                _Users.CollectionChanged += Data_CollectionChanged;
                _Subjects.CollectionChanged += Data_CollectionChanged;
                _Periods.CollectionChanged += Data_CollectionChanged;
                _Classes.CollectionChanged += Data_CollectionChanged;

                // Send the connection message
                Server.Send(Msg);
            }
            catch
            {
                return null;
            }
            finally
            {
                // Release the lock
                Monitor.Exit(Lock);
            }

            try
            {
                // Wait for both signals to fire, signalling
                InitialisedEvent.WaitOne();
                UserEvent.WaitOne();
            }
            catch
            {
                // Disconnected during initialise
                return null;
            }

            // Return the User and their Room (grouped together for easy return value)
            return new Tuple<User, Room>(CurrentUser, CurrentRoom);
        }

        // Unlock the object on disposal
        public void Dispose()
        {
            if (LockData)
                Monitor.Exit(Lock);
        }

        // Take a frame of the information in the database model
        public static DataSnapshot TakeSnapshot(bool Lock = true)
        {
            DataSnapshot Frame = new DataSnapshot();
            using (DataRepository Repo = new DataRepository(Lock))
            {
                Frame.Bookings = Repo.Bookings.ToList();
                Frame.Departments = Repo.Departments.ToList();
                Frame.Periods = Repo.Periods.ToList();
                Frame.Rooms = Repo.Rooms.ToList();
                Frame.Users = Repo.Users.ToList();
                Frame.Subjects = Repo.Subjects.ToList();
                Frame.Classes = Repo.Classes.ToList();
            }
            return Frame;
        }

        // Load in a snapshot to the database model
        public static void LoadSnapshot(DataSnapshot Frame, bool Lock)
        {
            using (DataRepository Repo = new DataRepository(Lock))
            {
                Repo.Bookings.Clear();
                Frame.Bookings.ForEach(b => Repo.Bookings.Add(b));

                Repo.Departments.Clear();
                Frame.Departments.ForEach(d => Repo.Departments.Add(d));

                Repo.Periods.Clear();
                Frame.Periods.ForEach(p => Repo.Periods.Add(p));

                Repo.Rooms.Clear();
                Frame.Rooms.ForEach(t => Repo.Rooms.Add(t));

                Repo.Users.Clear();
                Frame.Users.ForEach(u => Repo.Users.Add(u));

                Repo.Subjects.Clear();
                Frame.Subjects.ForEach(s => Repo.Subjects.Add(s));

                Repo.Classes.Clear();
                Frame.Classes.ForEach(c => Repo.Classes.Add(c));

                // Run through all lists and expand the items within them
                foreach (IList Table in Tables.Values)
                {
                    foreach (DataModel d in Table)
                    {
                        d.Expand(Repo);
                    }
                }
            }
        }

        // Handler for when the server receives a message
        private static void MessageReceived(Connection Sender, Message Msg)
        {
            bool Locked = true;
            Monitor.Enter(Lock);

            // Don't echo chages back to the server - changes made in this
            // function have been sent to us by the server
            ReportModelChanges = false;

            if (Msg is InitialiseMessage)
            {
                // Initialisation of the client - load in the data
                LoadSnapshot((Msg as InitialiseMessage).Snapshot, false);

                // Signal that we've received the initial data
                InitialisedEvent.Set();
            }
            else if (Msg is UserInformationMessage)
            {
                // Information on the User and their Room

                UserInformationMessage m = (Msg as UserInformationMessage);
                User User = m.User;
                Room Room = m.Room;

                if (User == null)
                    throw new ArgumentNullException("Received a null user.");
                if (Room == null)
                    throw new ArgumentNullException("Received a null room.");

                // Acquire references to the actual user/room
                DataSnapshot Frame = TakeSnapshot(false);
                CurrentUser = Frame.Users.SingleOrDefault(u => u.Id == User.Id);
                CurrentRoom = Frame.Rooms.SingleOrDefault(r => r.Id == Room.Id);

                // Signal that we've received the user data
                UserEvent.Set();
            }
            else if (Msg is DataMessage)
            {
                DataMessage Data = (DataMessage)Msg;

                using (DataRepository Repo = new DataRepository(false))
                    Data.Item.Expand(Repo);
                
                if (!Data.Delete)
                    Data.Item.Attach();
                else
                    Data.Item.Detach();

                IList Table = Tables[Data.Item.GetType()];
                
                int Index = -1;
                for (int x = 0; x < Table.Count; x++)
                {
                    if (((DataModel)Table[x]).Id == Data.Item.Id)
                    {
                        Index = x;
                        break;
                    }
                }

                if (!Data.Delete)
                {
                    if (Index < 0)
                        Table.Add((Booking)Data.Item);
                    else
                        Table[Index] = (Booking)Data.Item;
                }
                else
                    Table.RemoveAt(Index);

                Monitor.Exit(Lock);
                Locked = false;

                DataChanged(Data.Item.GetType());
            }

            ReportModelChanges = true; // Continue reporting changes

            if (Locked) // Unlock if necessary
                Monitor.Exit(Lock);
        }

        // On the server disconnecting
        private static void Disconnected(Connection Sender, DisconnectMessage Message)
        {
            Server.MessageReceived -= MessageReceived;
            Server.Disconnect -= Disconnected;

            InitialisedEvent.Dispose();
            InitialisedEvent = null;
            UserEvent.Dispose();
            UserEvent = null;
        }

        // On a collection changing
        private static void Data_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (ReportModelChanges)
            {
                lock (Lock)
                {
                    ReportModelChanges = false;
                    if (e.NewItems != null)
                    {
                        foreach (DataModel d in e.NewItems)
                        {
                            ((IList)sender).Remove(d);
                            Server.Send(new DataMessage(d, false));
                        }
                    }
                    if (e.OldItems != null)
                    {
                        foreach (DataModel d in e.OldItems)
                        {
                            ((IList)sender).Add(d);
                            Server.Send(new DataMessage(d, true));
                        }
                    }
                    ReportModelChanges = true;
                }
            }
        }

        public void OnDataChanged(Type ChangedType)
        {
            if (DataChanged != null)
                DataChanged(ChangedType);
        }
    }
}
