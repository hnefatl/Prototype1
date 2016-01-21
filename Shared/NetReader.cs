using System;
using System.Text;
using System.IO;
using System.Net;

namespace Shared
{
    // Reads data from a stream using bytes rather than text.
    public class NetReader
        : Reader
    {
        public NetReader(Stream Base)
            : base(Base)
        {
        }

        // Read a specified number of bytes
        public virtual byte[] ReadBytes(int Count)
        {
            if (Count <= 0)
                return new byte[0];

            byte[] Buffer = new byte[Count];
            int Remaining = Count;
            // Not guaranteed to read all bytes on first try - rety until all read
            while (Remaining > 0)
                Remaining -= Base.Read(Buffer, Buffer.Length - Remaining, Remaining);

            if (Remaining != 0)
                throw new Exception("Bad read (read " + (Buffer.Length - Remaining) + " of " + Buffer.Length + " bytes).");

            return Buffer;
        }
        public override byte ReadByte()
        {
            return ReadBytes(1)[0];
        }
        public override bool ReadBool()
        {
            return Convert.ToBoolean(ReadByte());
        }
        public override short ReadInt16()
        {
            return IPAddress.NetworkToHostOrder(BitConverter.ToInt16(ReadBytes(sizeof(short)), 0));
        }
        public override int ReadInt32()
        {
            return IPAddress.NetworkToHostOrder(BitConverter.ToInt32(ReadBytes(sizeof(int)), 0));
        }
        public override long ReadInt64()
        {
            return IPAddress.NetworkToHostOrder(BitConverter.ToInt64(ReadBytes(sizeof(long)), 0));
        }
        public override string ReadString()
        {
            int Length = ReadInt32(); // Read the length of the data first
            return Encoding.UTF8.GetString(ReadBytes(Length));
        }

        // Asycnhronous methods using Begin-End paradigm
        public virtual IAsyncResult BeginReadBytes(int Count, AsyncCallback Callback)
        {
            byte[] Buffer = new byte[Count]; // Allocate the memory to be used

            // Delegate reading to the stream's async methods, passing the buffer as the state
            return Base.BeginRead(Buffer, 0, Buffer.Length, Callback, Buffer);
        }
        public virtual byte[] EndReadBytes(IAsyncResult Handle)
        {
            // Retrieve the buffer as the state
            byte[] Buffer = (byte[])Handle.AsyncState;

            int Read = Base.EndRead(Handle);

            // Check for a valid read (correct number of bytes)
            if (Read != Buffer.Length)
                throw new Exception("Bad read (read " + Read + " of " + Buffer.Length + " bytes).");

            Handle.AsyncWaitHandle.Dispose(); // Cleanup

            return Buffer;
        }

        public virtual IAsyncResult BeginReadByte(AsyncCallback Callback)
        {
            return BeginReadBytes(sizeof(byte), Callback);
        }
        public virtual byte EndReadByte(IAsyncResult Handle)
        {
            return EndReadBytes(Handle)[0];
        }

        public virtual IAsyncResult BeginReadBool(AsyncCallback Callback)
        {
            return BeginReadByte(Callback);
        }
        public virtual bool EndReadBool(IAsyncResult Handle)
        {
            return Convert.ToBoolean(EndReadByte(Handle));
        }

        public virtual IAsyncResult BeginReadInt16(AsyncCallback Callback)
        {
            return BeginReadBytes(sizeof(short), Callback);
        }
        public virtual short EndReadInt16(IAsyncResult Handle)
        {
            return IPAddress.NetworkToHostOrder(BitConverter.ToInt16(EndReadBytes(Handle), 0));
        }

        public virtual IAsyncResult BeginReadInt32(AsyncCallback Callback)
        {
            return BeginReadBytes(sizeof(int), Callback);
        }
        public virtual int EndReadInt32(IAsyncResult Handle)
        {
            return IPAddress.NetworkToHostOrder(BitConverter.ToInt32(EndReadBytes(Handle), 0));
        }

        public virtual IAsyncResult BeginReadInt64(AsyncCallback Callback)
        {
            return BeginReadBytes(sizeof(long), Callback);
        }
        public virtual long EndReadInt64(IAsyncResult Handle)
        {
            return IPAddress.NetworkToHostOrder(BitConverter.ToInt64(EndReadBytes(Handle), 0));
        }

        public virtual IAsyncResult BeginReadString(AsyncCallback Callback)
        {
            return BeginReadInt32(Callback);
        }
        public virtual string EndReadString(IAsyncResult Handle)
        {
            int Length = EndReadInt32(Handle);

            return Encoding.UTF8.GetString(ReadBytes(Length));
        }

        // Static utility function to determine the number of bytes required to send a particular object.
        public static int NetworkLength(object o)
        {
            if (o is short)
                return sizeof(short);
            else if (o is int)
                return sizeof(int);
            else if (o is long)
                return sizeof(long);
            else if (o is byte)
                return sizeof(byte);
            else if (o is string) // Sends size of contents + actual contents
                return sizeof(int) + Encoding.UTF8.GetByteCount((string)o);
            else
                throw new NotSupportedException();
        }
    }
}
