using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using System.Threading;

using Data.Models;
using Data;

namespace Server
{
    public class DataRepository
        : DbContext, IDataRepository
    {
        List<Booking> IDataRepository.Bookings { get { return Bookings.Include(b => b.Rooms).Include(b => b.Students).Include(b => b.Subject).Include(b => b.Teacher).ToList(); } }
        public virtual DbSet<Booking> Bookings { get; set; }

        List<Department> IDataRepository.Departments { get { return Departments.Include(d => d.Teachers).ToList(); } }
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

        public const bool Home = true;
        private const string ServerProvider = Home ? "MSSQLLocalDb" : "v11.0";
        private const string Drive = Home ? "G" : "E";

        protected static object Lock = new object();

        public DataRepository(bool Proxies = true)
            : base(@"data source=(LocalDb)\" + ServerProvider + @";AttachDbFilename=" + Drive + @":\Burford\Year 13\Computing\Project\Data\Data.mdf;Database=Data;integrated security=True;MultipleActiveResultSets=True;App=EntityFramework")
        {
            Monitor.Enter(Lock);
            Configuration.LazyLoadingEnabled = false;
            Configuration.ProxyCreationEnabled = false;
            Database.SetInitializer(new DropCreateDatabaseAlways<DataRepository>());
        }
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            Monitor.Exit(Lock);
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().Map<Student>(c => c.Requires("Discriminator").HasValue((int)UserType.Student));
            modelBuilder.Entity<User>().Map<Teacher>(c => c.Requires("Discriminator").HasValue((int)UserType.Teacher));

            base.OnModelCreating(modelBuilder);
        }

        public static DataSnapshot TakeSnapshot(bool Lock = true)
        {
            DataSnapshot Frame = new DataSnapshot();
            using (DataRepository Repo = new DataRepository(Lock))
            {
                Repo.SetProxies(false);

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

        public void SetProxies(bool Enabled)
        {
            Configuration.ProxyCreationEnabled = Enabled;
            Configuration.LazyLoadingEnabled = Enabled;
        }
    }
}
