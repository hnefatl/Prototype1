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
        : DbContext
    {
        public virtual DbSet<Booking> Bookings { get; set; }
        public virtual DbSet<Department> Departments { get; set; }
        public virtual DbSet<Room> Rooms { get; set; }
        public virtual DbSet<Student> Students { get; set; }
        public virtual DbSet<Subject> Subjects { get; set; }
        public virtual DbSet<Teacher> Teachers { get; set; }
        public virtual DbSet<TimeSlot> Periods { get; set; }

        public DataRepository()
            : base("Data")
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
                Frame.Bookings = Repo.Bookings.ToList();
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
