using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Radio.Player.Services.Utilities
{
    public class FixedSizeQueue<TElement>
    {
        private readonly object _lock = new object();

        private readonly ConcurrentQueue<TElement> _internalQueue;

        public int SizeLimit { get; }

        public FixedSizeQueue(int sizeLimit)
        {
            if (sizeLimit < 1)
                throw new ArgumentOutOfRangeException(nameof(sizeLimit));

            SizeLimit = sizeLimit;

            _internalQueue = new ConcurrentQueue<TElement>();
        }

        public void Enqueue(TElement element)
        {
            // insert into queue
            _internalQueue.Enqueue(element);

            // check for limit size
            lock (_lock)
            {
                while (_internalQueue.Count > SizeLimit)
                {
                    _internalQueue.TryDequeue(out TElement overflowingElement);
                }
            }
        }

        public bool Contains(TElement element)
        {
            lock (_lock)
            {
                return _internalQueue.Contains(element);
            }
        }
    }
}