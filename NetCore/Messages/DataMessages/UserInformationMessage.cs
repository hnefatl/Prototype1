using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Data.Models;
using Shared;

namespace NetCore.Messages.DataMessages
{
    public class UserInformationMessage
        : Message
    {
        public User User { get; set; }
        public Room Room { get; set; }

        public UserInformationMessage()
            : this(null, null)
        {
        }
        public UserInformationMessage(User User, Room Room)
        {
            this.User = User;
            this.Room = Room;
        }

        public override void Serialise(Writer Out)
        {
            base.Serialise(Out);

            Out.Write(User != null);
            if (User != null)
                User.Serialise(Out);
            Out.Write(Room != null);
            if (Room != null)
                Room.Serialise(Out);
        }
        public override void Deserialise(Reader In)
        {
            if (In.ReadBool())
                User = (User)DataModel.DeserialiseExternal(In);
            if (In.ReadBool())
                Room = (Room)DataModel.DeserialiseExternal(In);
        }
    }
}
