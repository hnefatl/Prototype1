using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Data.Models;

namespace Data
{
    public interface IDataRepository
    {
        IList<Booking> Bookings { get; }
        IList<Department> Departments { get; }
        IList<Room> Rooms { get; }
        IList<Student> Students { get; }
        IList<Subject> Subjects { get; }
        IList<Teacher> Teachers { get; }
        IList<TimeSlot> Periods { get; }
    }
}
