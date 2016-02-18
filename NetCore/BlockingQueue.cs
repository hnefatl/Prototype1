using System;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace NetCore
{
    // Utility - effectively a typedef for a BlockingCollection that uses a ConcurrentQueue as the underlying Collection
    public class BlockingQueue<T>
        : BlockingCollection<T>
    {
        public BlockingQueue()
            : base(new ConcurrentQueue<T>())
        {
        }
        public BlockingQueue(IEnumerable<T> Collection)
            : base(new ConcurrentQueue<T>(Collection))
        {
        }
    }
}
