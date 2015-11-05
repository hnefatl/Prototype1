using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;

using Shared;

namespace NetCore
{
    public class NetWriter
        : Writer
    {
        public NetWriter(Stream Base)
            :base(Base)
        {
        }

        public virtual void Write(byte[] Data)
        {
            Base.Write(Data, 0, Data.Length);
        }
        public override void Write(byte b)
        {
            Write(new byte[] { b });
        }
        public override void Write(bool b)
        {
            Write(new byte[] { Convert.ToByte(b) });
        }
        public override void Write(short s)
        {
            Write(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(s)));
        }
        public override void Write(int i)
        {
            Write(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(i)));
        }
        public override void Write(long l)
        {
            Write(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(l)));
        }
        public override void Write(string s)
        {
            Write(Encoding.UTF8.GetByteCount(s));
            Write(Encoding.UTF8.GetBytes(s));
        }
    }
}
