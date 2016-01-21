using System;
using System.IO;

namespace Shared
{
    // Writes data to the output Stream using standard text encoding
    public class TextWriter
        : Writer
    {
        // Internal wrapper object
        protected StreamWriter Writer { get; set; }

        public TextWriter(Stream Base)
            : base(Base)
        {
            Writer = new StreamWriter(Base);
        }
        public override void Dispose()
        {
            // Dispose the Writer, then dispose the base class
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
