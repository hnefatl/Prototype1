using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Shared;
using Data.Models;

namespace NetCore.Messages.DataMessages
{
    public class NewRoomMessage
        : Message
    {
        public Room Room { get; set; }



        public override void Serialise(Writer Writer)
        {
            base.Serialise(Writer);


        }
        public override void Deserialise(Reader Reader)
        {
            throw new NotImplementedException();
        }
    }
}
