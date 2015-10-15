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

        public override void Serialise(IWriter Writer)
        {
            base.Serialise(Writer);

            Writer.Write(User != null);
            if (User != null)
            {
                Writer.Write((int)User.Access);
                User.Serialise(Writer);
            }
        }
        public override void Deserialise(IReader Reader)
        {
            if (Reader.ReadBool())
            {
                AccessMode Access = (AccessMode)Reader.ReadInt32();
                if (Access == AccessMode.Teacher || Access == AccessMode.Admin)
                    User = new Teacher();
                else
                    User = new Student();
                User.Deserialise(Reader);
            }
        }
    }
}
