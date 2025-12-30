using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Device
{
    public class SystemUsageQueue
    {
        private int _capacity;
        private readonly Queue<SystemUsageDTO> _queue = new();
        private readonly object _lock = new();

        public SystemUsageQueue(int capacity)
        {
            _capacity = capacity;
        }

        public void Resize(int newCapacity)
        {
            lock (_lock)
            {
                _capacity = newCapacity;

                while (_queue.Count > _capacity)
                    _queue.Dequeue();
            }
        }

        public void Add(SystemUsageDTO sample)
        {
            lock (_lock)
            {
                if (_queue.Count >= _capacity)
                {
                    _queue.Dequeue(); // legrégebbi törlése
                }

                _queue.Enqueue(sample);
            }
        }

        public IReadOnlyCollection<SystemUsageDTO> Snapshot()
        {
            lock (_lock)
            {
                return _queue.ToList();
            }
        }

        public static void LogBuffer(
        ILogger logger,
        IEnumerable<SystemUsageDTO> samples)
        {
            logger.LogInformation("---- System Usage History ----");

            foreach (var s in samples)
            {
                logger.LogInformation(
                    "{Time} | CPU: {Cpu}% | MEM: {Mem}%",
                    s.Timestamp,
                    s.CpuPercent,
                    s.MemoryPercent);
            }

            logger.LogInformation("------------------------");
        }
    }
}
