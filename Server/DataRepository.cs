using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;

using Data.Models;
using Data;

namespace Server
{
    public class DataRepository
        : DbContext, IDataRepository
    {

        List<Booking> IDataRepository.Bookings { get { return Bookings.ToList(); } }
        public virtual DbSet<Booking> Bookings { get; set; }

        List<Department> IDataRepository.Departments { get { return Departments.ToList(); } }
        public virtual DbSet<Department> Departments { get; set; }

        List<Room> IDataRepository.Rooms { get { return Rooms.ToList(); } }
        public virtual DbSet<Room> Rooms { get; set; }

        List<Student> IDataRepository.Students { get { return Students.ToList(); } }
        public virtual DbSet<Student> Students { get; set; }

        List<Subject> IDataRepository.Subjects { get { return Subjects.ToList(); } }
        public virtual DbSet<Subject> Subjects { get; set; }

        List<Teacher> IDataRepository.Teachers { get { return Teachers.ToList(); } }
        public virtual DbSet<Teacher> Teachers { get; set; }

        List<TimeSlot> IDataRepository.Periods { get { return Periods.ToList(); } }
        public virtual DbSet<TimeSlot> Periods { get; set; }

        List<Class> IDataRepository.Classes { get { return Classes.ToList(); } }
        public virtual DbSet<Class> Classes { get; set; }

        public const bool Home = false;
        private const string ServerProvider = Home ? "MSSQLLocalDb" : "v11.0";
        private const string Drive = Home ? "G" : "E";

        public DataRepository()
            : base(@"data source=(LocalDb)\" + ServerProvider + @";AttachDbFilename=" + Drive + @":\Burford\Year 13\Computing\Project\Data\Data.mdf;Database=Data;integrated security=True;MultipleActiveResultSets=True;App=EntityFramework")
        {
            Database.SetInitializer(new DropCreateDatabaseAlways<DataRepository>());
        }

        public static DataSnapshot TakeSnapshot()
        {
            DataSnapshot Frame = new DataSnapshot();
            using (DataRepository Repo = new DataRepository())
            {
                Repo.SetProxies(false);

                Frame.Bookings = Repo.Bookings.Include(b => b.Rooms).Include(b => b.Students).ToList();
                Frame.Departments = Repo.Departments.Include(d => d.Teachers).ToList();
                Frame.Periods = Repo.Periods.Include(p => p.Bookings).ToList();
                Frame.Rooms = Repo.Rooms.Include(r => r.Bookings).ToList();
                Frame.Students = Repo.Students.Include(s => s.Bookings).ToList();
                Frame.Subjects = Repo.Subjects.Include(s => s.Bookings).ToList();
                Frame.Teachers = Repo.Teachers.Include(t => t.Bookings).ToList();
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
