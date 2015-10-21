using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data.Entity;

using NetCore.Client;
using NetCore.Messages;
using NetCore.Messages.DataMessages;
using Data;
using Data.Models;

namespace Client
{
    public delegate void UserChangeHandler(User NewUser);
    public delegate void DataChangedHandler();
    public class DataRepository
        : IDisposable, IDataRepository
    {
        private static Connection Server { get; set; }

        List<Booking> IDataRepository.Bookings { get { return Bookings.ToList(); } }
        private static ObservableCollection<Booking> _Bookings = new ObservableCollection<Booking>();
        public ObservableCollection<Booking> Bookings
        {
            get { return _Bookings; }
            set { _Bookings = value; }
        }

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

        List<Student> IDataRepository.Students { get { return Students.ToList(); } }
        private static ObservableCollection<Student> _Students = new ObservableCollection<Student>();
        public ObservableCollection<Student> Students
        {
            get { return _Students; }
            set { _Students = value; }
        }

        List<Teacher> IDataRepository.Teachers { get { return Teachers.ToList(); } }
        private static ObservableCollection<Teacher> _Teachers = new ObservableCollection<Teacher>();
        public ObservableCollection<Teacher> Teachers
        {
            get { return _Teachers; }
            set { _Teachers = value; }
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

        private static object Lock = new object();
        private static bool ReportModelChanges { get; set; } // Whether to inform the server of model changes

        private static ManualResetEvent InitialisedEvent { get; set; }
        private static ManualResetEvent UserEvent { get; set; }
        private static User CurrentUser { get; set; }

        public static event UserChangeHandler UserChange = delegate { };
        protected static void OnUserChange(User New)
        {
            if (UserChange != null)
                UserChange(New);
        }

        public static event DataChangedHandler DataChanged = delegate { };

        private bool LockData;

        public DataRepository(bool LockData = true)
        {
            this.LockData = LockData;
            if (LockData)
                Monitor.Enter(Lock); // Only allow one DataRepository to be instantiated at a time. Block until all other ones are Disposed.
        }
        public static User Initialise(Connection Server, ConnectMessage Msg)
        {
            try
            {
                Monitor.Enter(Lock);

                Server.Send(Msg);

                if (InitialisedEvent == null)
                    InitialisedEvent = new ManualResetEvent(false);
                InitialisedEvent.Reset();
                if (UserEvent == null)
                    UserEvent = new ManualResetEvent(false);
                UserEvent.Reset();

                DataRepository.Server = Server;

                Server.MessageReceived += MessageReceived;
                Server.Disconnect += Disconnected;

                _Bookings.CollectionChanged += Data_CollectionChanged;
                _Departments.CollectionChanged += Data_CollectionChanged;
                _Rooms.CollectionChanged += Data_CollectionChanged;
                _Students.CollectionChanged += Data_CollectionChanged;
                _Subjects.CollectionChanged += Data_CollectionChanged;
                _Teachers.CollectionChanged += Data_CollectionChanged;
                _Periods.CollectionChanged += Data_CollectionChanged;
                _Classes.CollectionChanged += Data_CollectionChanged;
            }
            catch
            {
                return null;
            }
            finally
            {
                Monitor.Exit(Lock);
            }

            try
            {
                InitialisedEvent.WaitOne();
                UserEvent.WaitOne();
            }
            catch
            {
                // Disconnected during initialise
                return null;
            }

            return CurrentUser;
        }

        public void Dispose()
        {
            if (LockData)
                Monitor.Exit(Lock);
        }

        public static DataSnapshot TakeSnapshot(bool Lock = true)
        {
            DataSnapshot Frame = new DataSnapshot();
            using (DataRepository Repo = new DataRepository(Lock))
            {
                Frame.Bookings = Repo.Bookings.ToList();
                Frame.Departments = Repo.Departments.ToList();
                Frame.Periods = Repo.Periods.ToList();
                Frame.Rooms = Repo.Rooms.ToList();
                Frame.Students = Repo.Students.ToList();
                Frame.Subjects = Repo.Subjects.ToList();
                Frame.Teachers = Repo.Teachers.ToList();
                Frame.Classes = Repo.Classes.ToList();
            }
            return Frame;
        }
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

                Repo.Students.Clear();
                Frame.Students.ForEach(s => Repo.Students.Add(s));

                Repo.Subjects.Clear();
                Frame.Subjects.ForEach(s => Repo.Subjects.Add(s));

                Repo.Teachers.Clear();
                Frame.Teachers.ForEach(t => Repo.Teachers.Add(t));

                Repo.Classes.Clear();
                Frame.Classes.ForEach(c => Repo.Classes.Add(c));

                foreach (Booking b in Repo.Bookings)
                    b.Expand(Repo);
                foreach (Department d in Repo.Departments)
                    d.Expand(Repo);
                foreach (TimeSlot t in Repo.Periods)
                    t.Expand(Repo);
                foreach (Room r in Repo.Rooms)
                    r.Expand(Repo);
                foreach (Student s in Repo.Students)
                    s.Expand(Repo);
                foreach (Subject s in Repo.Subjects)
                    s.Expand(Repo);
                foreach (Teacher t in Repo.Teachers)
                    t.Expand(Repo);
                foreach (Class c in Repo.Classes)
                    c.Expand(Repo);
            }
        }

        private static void MessageReceived(Connection Sender, Message Msg)
        {
            Monitor.Enter(Lock);
            bool Unlocked = false;


            ReportModelChanges = false; // We've just got this message from the sever, don't echo the results back

            if (Msg is InitialiseMessage)
            {
                LoadSnapshot((Msg as InitialiseMessage).Snapshot, false);
                InitialisedEvent.Set();
            }
            else if (Msg is UserInformationMessage)
            {
                OnUserChange((Msg as UserInformationMessage).User);
                User u = (Msg as UserInformationMessage).User;

                if (u == null)
                    throw new ArgumentNullException("Received a null user.");

                DataSnapshot Frame = TakeSnapshot(false);
                if (u is Student)
                    CurrentUser = Frame.Students.Where(s => s.Id == u.Id).SingleOrDefault();
                else
                    CurrentUser = Frame.Teachers.Where(t => t.Id == u.Id).SingleOrDefault();

                UserEvent.Set();
            }
            else if (Msg is DataMessage)
            {
                DataMessage Data = (DataMessage)Msg;

                Monitor.Exit(Lock);
                Unlocked = true;

                if (Data.Item is Booking)
                {
                    if (!Data.Delete)
                    {
                        using (DataRepository Repo = new DataRepository(false))
                            Data.Item.Expand(Repo);
                        _Bookings.Add((Booking)Data.Item);
                    }
                    else
                        _Bookings.Remove(_Bookings.Where(b => b.Id == Data.Item.Id).Single());
                }
            }

            ReportModelChanges = true; // Continue reporting

            if (!Unlocked)
                Monitor.Exit(Lock);
        }
        private static void Disconnected(Connection Sender, DisconnectMessage Message)
        {
            Server.MessageReceived -= MessageReceived;
            Server.Disconnect -= Disconnected;

            InitialisedEvent.Dispose();
            InitialisedEvent = null;
        }

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
                            ((System.Collections.IList)sender).Remove(d);
                            Server.Send(new DataMessage(d, false));
                        }
                    }
                    if (e.OldItems != null)
                    {
                        foreach (DataModel d in e.OldItems)
                        {
                            ((System.Collections.IList)sender).Add(d);
                            Server.Send(new DataMessage(d, true));
                        }
                    }
                    ReportModelChanges = true;
                }
            }

            DataChanged();
        }
    }
}
