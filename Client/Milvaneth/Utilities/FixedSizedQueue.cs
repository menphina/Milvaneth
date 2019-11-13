using System.Collections.Concurrent;

namespace Milvaneth.Utilities
{
    public class FixedSizedQueue<T> : ConcurrentQueue<T>
    {
        public int Limit { get; set; }

        public FixedSizedQueue(int limit) : base()
        {
            Limit = limit;
        }

        public new void Enqueue(T item)
        {
            while (Count >= Limit)
            {
                TryDequeue(out _);
            }
            base.Enqueue(item);
        }
    }
}
