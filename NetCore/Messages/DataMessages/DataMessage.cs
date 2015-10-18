using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Data.Models;
using Shared;

namespace NetCore.Messages.DataMessages
{
    public class DataMessage
        : Message
    {
        public virtual DataModel Item { get; set; }
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

        public override void Serialise(IWriter Out)
        {
            base.Serialise(Out);

            Out.Write(Delete);
            Item.Serialise(Out);
        }
        public override void Deserialise(IReader In)
        {
            Delete = In.ReadBool();
            Item = DataModel.DeserialiseExternal(In);
        }
    }
}
