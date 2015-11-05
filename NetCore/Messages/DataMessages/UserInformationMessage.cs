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

        public UserInformationMessage()
            : this(null)
        {
        }
        public UserInformationMessage(User User)
        {
            this.User = User;
        }

        public override void Serialise(Writer Out)
        {
            base.Serialise(Out);

            Out.Write(User != null);
            if (User != null)
            {
                User.Serialise(Out);
            }
        }
        public override void Deserialise(Reader In)
        {
            if (In.ReadBool())
            {
                User = (User)DataModel.DeserialiseExternal(In);
            }
        }
    }
}
