using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Data.Models;
using Shared;

namespace NetCore.Messages.DataMessages
{
    public class NewBookingMessage
        : Message
    {
        public Booking Booking { get; set; }

        public NewBookingMessage()
            :this(null)
        {
        }
        public NewBookingMessage(Booking b)
        {
            Booking = b;
        }

        public override void Serialise(IWriter Writer)
        {
            base.Serialise(Writer);

            Booking.Serialise(Writer);
        }
        public override void Deserialise(IReader Reader)
        {
            Booking.Deserialise(Reader);
        }
    }
}
