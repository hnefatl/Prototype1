using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Data.Models;
using Shared;

namespace NetCore.Messages.DataMessages
{
    /// <summary>
    /// Used to add/edit/delete a booking
    /// If the internal Booking is set, the server should add the booking if it doesn't conflict with any other booking,
    /// edit a booking if it does conflict but the same teacher is involved in both, and fail if the bookings clash and
    /// they're different teachers.
    /// </summary>
    public class BookingMessage
        : DataMessage<Booking>
    {
        public BookingMessage()
            : base(new Booking(), false)
        {
        }
        public BookingMessage(Booking b)
            : base(b, false)
        {
        }
        public BookingMessage(Booking b, bool Delete)
            :base(b, Delete)
        {
        }
    }
}
