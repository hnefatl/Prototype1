using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Data.Models;

namespace Data
{
    public interface IDataRepository
    {
        List<Booking> Bookings { get; }
        List<Department> Departments { get; }
        List<Room> Rooms { get; }
        List<Student> Students { get; }
        List<Subject> Subjects { get; }
        List<Teacher> Teachers { get; }
        List<TimeSlot> Periods { get; }
    }
}
