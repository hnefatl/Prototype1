using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Shared;
using Data.Models;

namespace NetCore.Messages
{
    public class NewRoomMessage
        : Message
    {
        public Room Room { get; set; }



        public override void Serialise(IWriter Writer)
        {
            base.Serialise(Writer);


        }
        public override void Deserialise(IReader Reader)
        {
            throw new NotImplementedException();
        }
    }
}
