using System;
using System.Collections.Generic;

using Data.Models;

namespace Data
{
    public interface IDataRepository
    {
        List<Booking> Bookings { get; }
        List<Department> Departments { get; }
        List<Room> Rooms { get; }
        List<User> Users { get; }
        List<Subject> Subjects { get; }
        List<TimeSlot> Periods { get; }
        List<Class> Classes { get; }
    }
}
