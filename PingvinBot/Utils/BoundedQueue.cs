using System.Collections.Generic;
using System.Linq;

namespace PingvinBot.Utils;

public class BoundedQueue<T>
{
    private readonly int _maxSize;

    private readonly Queue<(T Item, int ItemSize)> _queue;

    public BoundedQueue(int maxSize)
    {
        _maxSize = maxSize;

        _queue = new Queue<(T, int)>();
    }

    public int CurrentSize { get; private set; } = 0;

    public void Enqueue(T item, int itemSize)
    {
        _queue.Enqueue((item, itemSize));

        CurrentSize += itemSize;

        RemoveOverflowingItems();
    }

    public void Enqueue(IEnumerable<(T Item, int ItemSize)> items)
    {
        foreach (var (item, itemSize) in items)
        {
            _queue.Enqueue((item, itemSize));

            CurrentSize += itemSize;
        }

        RemoveOverflowingItems();
    }

    public IEnumerable<T> GetAll()
    {
        return _queue.ToList().Select(l => l.Item);
    }

    private void RemoveOverflowingItems()
    {
        if (_queue.Count == 0) return;

        while (CurrentSize > _maxSize)
        {
            var (_, itemSize) = _queue.Dequeue();

            CurrentSize -= itemSize;
        }
    }
}
