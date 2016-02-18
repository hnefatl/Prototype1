using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Threading;

using Data.Models;
using Data;

namespace Server
{
    // The DataRepository interfaces with Entity Framework
    // to store data in a database file. Inheriting from DbContext
    // and listing properties in DbSet<T> makes the tables.
    public class DataRepository
        : DbContext, IDataRepository
    {
        List<Booking> IDataRepository.Bookings { get { return Bookings.Include(b => b.Rooms).Include(b => b.Students).Include(b => b.Subject).Include(b => b.Teacher).Include(b => b.TimeSlot).ToList(); } }
        public virtual DbSet<Booking> Bookings { get; set; }

        List<Department> IDataRepository.Departments { get { return Departments.Include(d => d.Teachers).Include(d => d.Rooms).ToList(); } }
        public virtual DbSet<Department> Departments { get; set; }

        List<Room> IDataRepository.Rooms { get { return Rooms.Include(r => r.Bookings).Include(r => r.Department).ToList(); } }
        public virtual DbSet<Room> Rooms { get; set; }

        public virtual DbSet<Student> Students { get; set; }

        public virtual DbSet<Teacher> Teachers { get; set; }

        List<User> IDataRepository.Users { get { return Users.ToList(); } }
        public virtual DbSet<User> Users { get; set; }

        List<Subject> IDataRepository.Subjects { get { return Subjects.Include(s => s.Bookings).ToList(); } }
        public virtual DbSet<Subject> Subjects { get; set; }

        List<TimeSlot> IDataRepository.Periods { get { return Periods.Include(p => p.Bookings).ToList(); } }
        public virtual DbSet<TimeSlot> Periods { get; set; }

        List<Class> IDataRepository.Classes { get { return Classes.Include(c => c.Owner).Include(c => c.Students).ToList(); } }
        public virtual DbSet<Class> Classes { get; set; }

        // Handy flag for testing this when I'm at home vs in school based off the user name
        public static readonly bool Home = Environment.UserName == "Keith";
        // Different versions of Entity Framework at home/school
        private static readonly string ServerProvider = Home ? "MSSQLLocalDb" : "v11.0";

        // Default path to the database
        private static string _Path = "Data.mdf";
        public static string Path { get { return _Path; } set { _Path = value; } }

        // Static object used for thread safety between multiple instantiations of this class
        protected static object Lock = new object();

        // Initialises the class and passes in a connection string to Entity Framework
        public DataRepository()
            : base(@"data source=(LocalDb)\" + ServerProvider + @";AttachDbFilename=" + Settings.DatabasePath + ";Database=Data;integrated security=True;MultipleActiveResultSets=True;App=EntityFramework")
        {
            // Acquire the lock for this instance of the class
            Monitor.Enter(Lock);
            SetProxies(false);
            Database.SetInitializer(new DropCreateDatabaseAlways<DataRepository>());
        }
        protected override void Dispose(bool Disposing)
        {
            base.Dispose(Disposing);

            // Release the lock for this instance
            Monitor.Exit(Lock);
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Allows Students and Teachers to be stored in the same table for ease of access
            modelBuilder.Entity<User>().Map<Student>(c => c.Requires("Discriminator").HasValue((int)UserType.Student));
            modelBuilder.Entity<User>().Map<Teacher>(c => c.Requires("Discriminator").HasValue((int)UserType.Teacher));

            base.OnModelCreating(modelBuilder);
        }

        // Returns a snapshot of the data in the database at the moment
        public static DataSnapshot TakeSnapshot()
        {
            DataSnapshot Frame = new DataSnapshot();
            using (DataRepository Repo = new DataRepository())
            {
                Repo.SetProxies(false);

                // Extract all the tables' data
                Frame.Bookings = Repo.Bookings.Include(b => b.Subject).Include(b => b.Teacher).Include(b => b.Rooms).Include(b => b.Students).ToList();
                Frame.Departments = Repo.Departments.Include(d => d.Teachers).ToList();
                Frame.Periods = Repo.Periods.Include(p => p.Bookings).ToList();
                Frame.Rooms = Repo.Rooms.Include(r => r.Department).Include(r => r.Bookings).ToList();
                Frame.Users = Repo.Users.ToList();
                Frame.Subjects = Repo.Subjects.Include(s => s.Bookings).ToList();
                Frame.Classes = Repo.Classes.Include(c => c.Students).ToList();

                Repo.SetProxies(true);
            }
            return Frame;
        }

        // Minor tweaks at runtime - Entity Framework uses proxies
        // to objects rather than the objects themselves, which can
        // cause problems when editing them. Use of this function
        // alleviates the problem
        public void SetProxies(bool Enabled)
        {
            Configuration.ProxyCreationEnabled = Enabled;
            Configuration.LazyLoadingEnabled = Enabled;
        }
    }
}
