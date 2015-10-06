using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public interface IWriter
        : IDisposable
    {
        void Write(byte[] Data);
        void Write(byte b);
        void Write(bool b);
        void Write(short s);
        void Write(int i);
        void Write(long l);
        void Write(string s);
    }
}