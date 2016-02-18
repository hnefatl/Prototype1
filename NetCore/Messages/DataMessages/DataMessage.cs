using System;

using Data.Models;
using Shared;

namespace NetCore.Messages.DataMessages
{
    // Sent when an item in the database is changed somehow
    public class DataMessage
        : Message
    {
        // The item that was changed
        public DataModel Item { get; set; }
        // Whether or not the item was deleted
        public bool Delete { get; set; }
        
        public DataMessage()
            : this(null, false)
        {
        }
        public DataMessage(DataModel Item, bool Delete)
        {
            this.Item = Item;
            this.Delete = Delete;
        }

        public override void Serialise(Writer Out)
        {
            base.Serialise(Out);

            Out.Write(Delete);
            Item.Serialise(Out);
        }
        public override void Deserialise(Reader In)
        {
            Delete = In.ReadBool();
            Item = DataModel.DeserialiseExternal(In);
        }
    }
}
