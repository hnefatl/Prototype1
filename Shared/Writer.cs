using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace Shared
{
    public abstract class Writer
        : IDisposable
    {
        protected Stream Base { get; set; }

        public Writer(Stream Base)
        {
            this.Base = Base;
        }
        public virtual void Dispose()
        {
            Base.Dispose();
        }

        public abstract void Write(byte b);
        public abstract void Write(bool b);
        public abstract void Write(short s);
        public abstract void Write(int i);
        public abstract void Write(long l);
        public abstract void Write(string s);

        public virtual void Write(object Item)
        {
            Type t = Item.GetType();

            if (t == typeof(byte))
                Write((byte)Convert.ChangeType(Item, typeof(byte)));
            else if (t == typeof(bool))
                Write((bool)Convert.ChangeType(Item, typeof(bool)));
            else if (t == typeof(short))
                Write((short)Convert.ChangeType(Item, typeof(short)));
            else if (t == typeof(int))
                Write((int)Convert.ChangeType(Item, typeof(int)));
            else if (t == typeof(long))
                Write((long)Convert.ChangeType(Item, typeof(long)));
            else if (t == typeof(string))
                Write((string)Convert.ChangeType(Item, typeof(string)));
            else
                throw new ArgumentException("Cannot write values of the type specified.");
        }
    }
}