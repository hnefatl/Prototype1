using System;
using System.Text;
using System.IO;

namespace Shared
{
    // Reads data from a stream using standard encoding.
    public class TextReader
        : Reader
    {
        // Internal wrapper object
        protected StreamReader Reader { get; set; }

        public TextReader(Stream Base)
            :base(Base)
        {
            Reader = new StreamReader(Base);
        }
        public override void Dispose()
        {
            // Dispose of the base class first, then the wrapper.
            base.Dispose();
            Reader.Dispose();
        }

        public override byte ReadByte()
        {
            return (byte)Reader.Read();
        }
        public override bool ReadBool()
        {
            return bool.Parse(Reader.ReadLine());
        }
        public override short ReadInt16()
        {
            return short.Parse(Reader.ReadLine());
        }
        public override int ReadInt32()
        {
            return int.Parse(Reader.ReadLine());
        }
        public override long ReadInt64()
        {
            return long.Parse(Reader.ReadLine());
        }
        public override string ReadString()
        {
            return Reader.ReadLine();
        }
    }
}
