using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Data.Models;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

using NetCore.Client;
using NetCore.Messages;
using NetCore.Messages.DataMessages;
using Data;

namespace Client
{
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

        List<Subject> IDataRepository.Subjects { get { return Subjects.ToList(); } }
        private static ObservableCollection<Subject> _Subjects = new ObservableCollection<Subject>();
        public ObservableCollection<Subject> Subjects
        {
            get { return _Subjects; }
            set { _Subjects = value; }
        }

        List<Teacher> IDataRepository.Teachers { get { return Teachers.ToList(); } }
        private static ObservableCollection<Teacher> _Teachers = new ObservableCollection<Teacher>();
        public ObservableCollection<Teacher> Teachers
        {
            get { return _Teachers; }
            set { _Teachers = value; }
        }

        List<TimeSlot> IDataRepository.Periods { get { return Periods.ToList(); } }
        private static ObservableCollection<TimeSlot> _Periods = new ObservableCollection<TimeSlot>();
        public ObservableCollection<TimeSlot> Periods
        {
            get { return _Periods; }
            set { _Periods = value; }
        }

        private static object Lock = new object();
        private static bool ReportModelChanges { get; set; } // Whether to inform the server of model changes

        private static ManualResetEvent InitialisedEvent { get; set; }

        private bool LockData;

        public DataRepository(bool LockData = true)
        {
            this.LockData = LockData;
            if (LockData)
                Monitor.Enter(Lock); // Only allow one DataRepository to be instantiated at a time. Block until all other ones are Disposed.
        }
        public static bool Initialise(Connection Server)
        {
            try
            {
                Monitor.Enter(Lock);

                if (InitialisedEvent == null)
                    InitialisedEvent = new ManualResetEvent(false);
                InitialisedEvent.Reset();

                DataRepository.Server = Server;

                Server.MessageReceived += MessageReceived;
                Server.Disconnect += Disconnected;

                _Bookings.CollectionChanged += Bookings_CollectionChanged;
                _Departments.CollectionChanged += Departments_CollectionChanged;
                _Rooms.CollectionChanged += Rooms_CollectionChanged;
                _Students.CollectionChanged += Students_CollectionChanged;
                _Subjects.CollectionChanged += Subjects_CollectionChanged;
                _Teachers.CollectionChanged += Teachers_CollectionChanged;
                _Periods.CollectionChanged += Periods_CollectionChanged;
            }
            catch
            {
                return false;
            }
            finally
            {
                Monitor.Exit(Lock);
            }

            try
            {
                InitialisedEvent.WaitOne();
            }
            catch
            {
                // Disconnected during initialise
                return false;
            }

            return true;
        }

        public void Dispose()
        {
            if (LockData)
                Monitor.Exit(Lock);
        }

        public static DataSnapshot TakeSnapshot()
        {
            DataSnapshot Frame = new DataSnapshot();
            using (DataRepository Repo = new DataRepository())
            {
                Frame.Bookings = Repo.Bookings.ToList();
                Frame.Departments = Repo.Departments.ToList();
                Frame.Periods = Repo.Periods.ToList();
                Frame.Rooms = Repo.Rooms.ToList();
                Frame.Students = Repo.Students.ToList();
                Frame.Subjects = Repo.Subjects.ToList();
                Frame.Teachers = Repo.Teachers.ToList();
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
            }
        }

        private static void MessageReceived(Connection Sender, Message Msg)
        {
            lock (Lock)
            {
                ReportModelChanges = false; // We've just got this message from the sever, don't echo it back

                if (Msg is InitialiseMessage)
                {
                    LoadSnapshot((Msg as InitialiseMessage).Snapshot, false);
                    ReportModelChanges = true;
                    InitialisedEvent.Set();
                }
                else if (Msg is NewBookingMessage)
                    _Bookings.Add((Msg as NewBookingMessage).Booking);



                ReportModelChanges = true; // Continue reporting
            }
        }
        private static void Disconnected(Connection Sender, DisconnectMessage Message)
        {
            Server.MessageReceived -= MessageReceived;
            Server.Disconnect -= Disconnected;

            InitialisedEvent.Dispose();
            InitialisedEvent = null;
        }

        private static void Bookings_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (ReportModelChanges)
            {
                if (e.NewItems != null)
                    foreach (Booking b in e.NewItems)
                        Server.Send(new NewBookingMessage(b));
                //if (e.OldItems != null)
                //    foreach (Booking b in e.OldItems)
                //        Server.Send(new NewBookingMessage(b));
            }
        }
        private static void Departments_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            //if (e.NewItems != null)
            //    foreach (Department d in e.NewItems)
            //        Server.Send(new NewDepartmentMessage(d));
            //if (e.OldItems != null)
            //    foreach (Department d in e.OldItems)
            //        Server.Send(new NewDepartmentMessage(d));
        }
        private static void Rooms_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            //if (e.NewItems != null)
            //    foreach (Room r in e.NewItems)
            //        Server.Send(new NewRoomMessage(r));
            //if (e.OldItems != null)
            //    foreach (Room r in e.OldItems)
            //        Server.Send(new NewRoomMessage(r));
        }
        private static void Students_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            //if (e.NewItems != null)
            //    foreach (Student s in e.NewItems)
            //        Server.Send(new NewStudentMessage(s));
            //if (e.OldItems != null)
            //    foreach (Student s in e.OldItems)
            //        Server.Send(new NewStudentMessage(s));
        }
        private static void Subjects_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            //if (e.NewItems != null)
            //    foreach (Subject s in e.NewItems)
            //        Server.Send(new NewSubjectMessage(s));
            //if (e.OldItems != null)
            //    foreach (Subject s in e.OldItems)
            //        Server.Send(new NewSubjectMessage(s));
        }
        private static void Teachers_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            //if (e.NewItems != null)
            //    foreach (Teacher t in e.NewItems)
            //        Server.Send(new NewTeacherMessage(t));
            //if (e.OldItems != null)
            //    foreach (Teacher t in e.OldItems)
            //        Server.Send(new NewTeacherMessage(t));
        }
        private static void Periods_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            //if (e.NewItems != null)
            //    foreach (TimeSlot p in e.NewItems)
            //        Server.Send(new NewPeriodMessage(p));
            //if (e.OldItems != null)
            //    foreach (TimeSlot p in e.OldItems)
            //        Server.Send(new NewPeriodMessage(p));
        }
    }
}
