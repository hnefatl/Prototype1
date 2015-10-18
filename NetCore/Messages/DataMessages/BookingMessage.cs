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
        : Message
    {
        public Booking Booking { get; set; }
        public bool Delete { get; set; }

        public BookingMessage()
            : this(new Booking(), false)
        {
        }
        public BookingMessage(Booking b)
            : this(b, false)
        {
        }
        public BookingMessage(Booking b, bool Delete)
        {
            Booking = b;
            this.Delete = Delete;
        }

        public override void Serialise(IWriter Out)
        {
            base.Serialise(Out);

            Out.Write(Delete);
            Booking.Serialise(Out);
        }
        public override void Deserialise(IReader In)
        {
            Delete = In.ReadBool();
            if (!Delete)
                Booking.Deserialise(In);
        }
    }
}
