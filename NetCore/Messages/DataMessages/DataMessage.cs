using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Data.Models;
using Shared;

namespace NetCore.Messages.DataMessages
{
    public abstract class DataMessage
        : Message
    {
        public virtual DataModel Item { get; set; }
        public bool Delete { get; set; }

        public override void Serialise(IWriter Out)
        {
            base.Serialise(Out);

            Out.Write(Delete);
            Item.Serialise(Out);
        }
        public override void Deserialise(IReader In)
        {
            Delete = In.ReadBool();
            Item.Deserialise(In);
        }
    }
    public class DataMessage<T>
        : DataMessage where T : DataModel
    {
        public new T Item { get; set; }

        public DataMessage()
            : this(Activator.CreateInstance<T>(), false)
        {
        }
        public DataMessage(T Item, bool Delete)
        {
            this.Item = Item;
            this.Delete = Delete;
        }
    }

    public static class DataMessageHelper
    {
        public static Message CreateMessage(DataModel Item, bool Delete)
        {
            if (Item is Booking)
                return new BookingMessage((Booking)Item, Delete);
            else
                return null;
        }
    }
}
