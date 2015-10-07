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

        IList<Booking> IDataRepository.Bookings { get { return (IList<Booking>)Bookings; } }
        public virtual DbSet<Booking> Bookings { get; set; }

        IList<Department> IDataRepository.Departments { get { return (IList<Department>)Departments; } }
        public virtual DbSet<Department> Departments { get; set; }

        IList<Room> IDataRepository.Rooms { get { return (IList<Room>)Rooms; } }
        public virtual DbSet<Room> Rooms { get; set; }

        IList<Student> IDataRepository.Students { get { return (IList<Student>)Students; } }
        public virtual DbSet<Student> Students { get; set; }

        IList<Subject> IDataRepository.Subjects { get { return (IList<Subject>)Subjects; } }
        public virtual DbSet<Subject> Subjects { get; set; }

        IList<Teacher> IDataRepository.Teachers { get { return (IList<Teacher>)Teachers; } }
        public virtual DbSet<Teacher> Teachers { get; set; }

        IList<TimeSlot> IDataRepository.Periods { get { return (IList<TimeSlot>)Periods; } }
        public virtual DbSet<TimeSlot> Periods { get; set; }

        public DataRepository()
            : base(@"data source=(LocalDb)\MSSQLLocalDb;AttachDbFilename=G:\Burford\Year 13\Computing\Project\Data\Data.mdf;Database=Data;integrated security=True;MultipleActiveResultSets=True;App=EntityFramework")
        {
            Database.SetInitializer(new DropCreateDatabaseAlways<DataRepository>());
        }

        public static DataSnapshot TakeSnapshot()
        {
            DataSnapshot Frame = new DataSnapshot();
            using (DataRepository Repo = new DataRepository())
            {
                bool OriginalProxy = Repo.Configuration.ProxyCreationEnabled;
                Repo.Configuration.ProxyCreationEnabled = false;
                Frame.Bookings = Repo.Bookings.Include(b => b.Rooms).Include(b => b.Students).ToList();
                Frame.Departments = Repo.Departments.ToList();
                Frame.Periods = Repo.Periods.ToList();
                Frame.Rooms = Repo.Rooms.ToList();
                Frame.Students = Repo.Students.ToList();
                Frame.Subjects = Repo.Subjects.ToList();
                Frame.Teachers = Repo.Teachers.ToList();
                Repo.Configuration.ProxyCreationEnabled = OriginalProxy;
            }
            return Frame;
        }

    }
}
