using System;

using Data.Models;
using Shared;

namespace NetCore.Messages.DataMessages
{
    // Sent by the Server to the Client on connection
    public class UserInformationMessage
        : Message
    {
        // The User that the logged on user corresponds to
        public User User { get; set; }
        // Identifies the Room the user is currently in
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
                User = DataModel.DeserialiseExternal<User>(In);
            if (In.ReadBool())
                Room = DataModel.DeserialiseExternal<Room>(In);
        }
    }
}
