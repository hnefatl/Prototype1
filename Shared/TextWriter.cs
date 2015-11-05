using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Shared
{
    public class TextWriter
        : Writer
    {
        protected StreamWriter Writer { get; set; }

        public TextWriter(Stream Base)
            : base(Base)
        {
            Writer = new StreamWriter(Base);
        }
        public override void Dispose()
        {
            Writer.Dispose();
            base.Dispose();
        }

        public override void Write(byte b)
        {
            Writer.WriteLine(b);
        }
        public override void Write(bool b)
        {
            Writer.WriteLine(b);
        }
        public override void Write(short s)
        {
            Writer.WriteLine(s);
        }
        public override void Write(int i)
        {
            Writer.WriteLine(i);
        }
        public override void Write(long l)
        {
            Writer.WriteLine(l);
        }
        public override void Write(string s)
        {
            Writer.WriteLine(s);
        }
    }
}
