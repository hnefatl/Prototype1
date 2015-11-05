using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Shared
{
    public class TextReader
        : Reader
    {
        protected StreamReader Reader { get; set; }

        public TextReader(Stream Base)
            :base(Base)
        {
            Reader = new StreamReader(Base);
        }
        public override void Dispose()
        {
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
