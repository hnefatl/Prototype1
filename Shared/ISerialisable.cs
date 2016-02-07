using System;

namespace Shared
{
    public interface ISerialisable
    {
        void Serialise(Writer Out);
        void Deserialise(Reader In);
    }
}
