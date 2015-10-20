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
        : IWriter
    {
        protected Stream Base { get; set; }

        public NetWriter(Stream Base)
        {
            this.Base = Base;
        }
        public void Dispose()
        {
            Base.Dispose();
        }

        public void Write(byte[] Data)
        {
            Base.Write(Data, 0, Data.Length);
        }
        public void Write(byte b)
        {
            Write(new byte[] { b });
        }
        public void Write(bool b)
        {
            Write(new byte[] { Convert.ToByte(b) });
        }
        public void Write(short s)
        {
            Write(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(s)));
        }
        public void Write(int i)
        {
            Write(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(i)));
        }
        public void Write(long l)
        {
            Write(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(l)));
        }
        public void Write(string s)
        {
            Write(Encoding.UTF8.GetByteCount(s));
            Write(Encoding.UTF8.GetBytes(s));
        }
    }
}
