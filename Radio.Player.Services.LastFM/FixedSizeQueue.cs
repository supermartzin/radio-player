using System.Collections.Concurrent;

namespace Radio.Player.Services.LastFM;

internal class FixedSizeQueue<TElement>
{
    private readonly object _lock = new();

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
        // check for limit size
        lock (_lock)
        {
            // insert into queue
            _internalQueue.Enqueue(element);

            while (_internalQueue.Count > SizeLimit)
            {
                _internalQueue.TryDequeue(out _);
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