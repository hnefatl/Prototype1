using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetCore.Sessions
{
    public interface ISessionCreator
    {
        Dictionary<Guid, Session> Sessions { get; }
        Session CreateSession<T>() where T : Session;
    }
}
