using System;
using System.Text;
using System.IO;
using System.Net;

namespace Shared
{
    // Write data to a stream using bytes rather than text.
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
            // Writing to a network, so use Network order conversion
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
            // Write the length of the string, then the actual string data
            Write(Encoding.BigEndianUnicode.GetByteCount(s));
            Write(Encoding.BigEndianUnicode.GetBytes(s));
        }
    }
}
