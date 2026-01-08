using Device.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Device.Control
{
    public class SystemUsageQueue
    {
        private int capacity;
        private readonly Queue<SystemUsageDTO> queue = new();
        private readonly object @lock = new();

        public SystemUsageQueue(int capacity)
        {
            this.capacity = capacity;
        }

        public void Resize(int newCapacity)
        {
            lock (@lock)
            {
                capacity = newCapacity;

                while (queue.Count > capacity)
                    queue.Dequeue();
            }
        }

        public void Add(SystemUsageDTO sample)
        {
            lock (@lock)
            {
                if (queue.Count >= capacity)
                {
                    queue.Dequeue(); // legrégebbi törlése
                }

                queue.Enqueue(sample);
            }
        }

        public IReadOnlyCollection<SystemUsageDTO> Snapshot()
        {
            lock (@lock)
            {
                return queue.ToList();
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
