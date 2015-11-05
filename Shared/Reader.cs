using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Shared
{
    public abstract class Reader
        : IDisposable
    {
        protected Stream Base { get; set; }

        public Reader(Stream Base)
        {
            this.Base = Base;
        }
        public virtual void Dispose()
        {
            Base.Dispose();
        }

        public abstract byte ReadByte();
        public abstract bool ReadBool();
        public abstract short ReadInt16();
        public abstract int ReadInt32();
        public abstract long ReadInt64();
        public abstract string ReadString();
        
        public virtual T Read<T>()
        {
            Type t = typeof(T);
            if (t == typeof(byte))
                return (T)Convert.ChangeType(ReadByte(), t);
            else if (t == typeof(bool))
                return (T)Convert.ChangeType(ReadBool(), t);
            else if (t == typeof(short))
                return (T)Convert.ChangeType(ReadInt16(), t);
            else if (t == typeof(int))
                return (T)Convert.ChangeType(ReadInt32(), t);
            else if (t == typeof(long))
                return (T)Convert.ChangeType(ReadInt64(), t);
            else if (t == typeof(string))
                return (T)Convert.ChangeType(ReadString(), t);
            else
                throw new ArgumentException("Cannot read values of the type specified.");
        }
    }
}