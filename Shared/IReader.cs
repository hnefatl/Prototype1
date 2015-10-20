using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public interface IReader
        : IDisposable
    {
        byte[] ReadBytes(int Count);
        byte ReadByte();
        bool ReadBool();
        short ReadInt16();
        int ReadInt32();
        long ReadInt64();
        string ReadString();

        IAsyncResult BeginReadBytes(int Count, AsyncCallback Callback);
        byte[] EndReadBytes(IAsyncResult Handle);

        IAsyncResult BeginReadByte(AsyncCallback Callback);
        byte EndReadByte(IAsyncResult Handle);

        IAsyncResult BeginReadBool(AsyncCallback Callback);
        bool EndReadBool(IAsyncResult Handle);

        IAsyncResult BeginReadInt16(AsyncCallback Callback);
        short EndReadInt16(IAsyncResult Handle);

        IAsyncResult BeginReadInt32(AsyncCallback Callback);
        int EndReadInt32(IAsyncResult Handle);

        IAsyncResult BeginReadInt64(AsyncCallback Callback);
        long EndReadInt64(IAsyncResult Handle);

        IAsyncResult BeginReadString(AsyncCallback Callback);
        string EndReadString(IAsyncResult Handle);
    }
}