using KT1.model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KT1.infrastructure
{
    public class ThreadSafePriorityQueue<T>
    {
        private readonly PriorityQueue<T, int> _queue = new PriorityQueue<T, int>();
        private readonly object _lock = new object();
        public void Enqueue(T item, int priority)
        {
            lock (_lock)
            {
                _queue.Enqueue(item, priority);
                Monitor.Pulse(_lock);
            }
        }
        public bool TryDequeue(out T item)
        {
            lock (_lock)
            {
                while (_queue.Count == 0)
                {
                    Monitor.Wait(_lock);
                }
                item = _queue.Dequeue();
                return true;
            }
        }
        public int Count {
            get
            {
                lock (_lock)
                {
                    return _queue.Count;
                }
            }
        }
        public List<T> Snapshot()
        {
            lock (_lock)
            {
                return _queue.UnorderedItems.Select(x => x.Element).ToList();
            }
        }
    }
}
