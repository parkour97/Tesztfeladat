using System.Diagnostics;
using System.Linq;

namespace Device.Control
{
    public class SystemUsageMonitor
    {
        private TimeSpan _lastCpuTime;
        private DateTime _lastCheckTime;
        private PerformanceCounter? _cpuCounter;
        private PerformanceCounter? _memCounter;

#if WINDOWS
        public SystemUsageMonitor()
        {
            InitializeCounters();
        }

        private void InitializeCounters()
        {
            _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            _memCounter = new PerformanceCounter("Memory", "% Committed Bytes In Use");

            // Warm-up
            _cpuCounter.NextValue();
            _memCounter?.NextValue();
        }
#endif

        public double GetCpuUsagePercent()
        {
#if WINDOWS
            return GetWindowsCpuUsage();
#elif LINUX
            return GetLinuxCpuUsage();
#else
            return GetFallbackCpuUsage();
#endif
        }

        private double GetWindowsCpuUsage()
        {
            if (_cpuCounter == null) return 0;

            var value = _cpuCounter.NextValue();
            return Math.Round(value, 2);
        }

        private double GetLinuxCpuUsage()
        {
            var start = ReadLinuxCpuTimes();
            Thread.Sleep(100);
            var end = ReadLinuxCpuTimes();

            var idleStart = start.Item2;
            var idleEnd = end.Item2;
            var totalStart = start.Item1;
            var totalEnd = end.Item1;

            var totalDiff = totalEnd - totalStart;
            var idleDiff = idleEnd - idleStart;

            return Math.Round(100 * (1.0 - (double)idleDiff / totalDiff), 2);
        }

        private static (ulong total, ulong idle) ReadLinuxCpuTimes()
        {
            var line = File.ReadAllLines("/proc/stat")[0];
            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Skip(1).Select(ulong.Parse).ToArray();
            ulong total = parts.Aggregate(0UL, (current, value) => current + value);
            var idle = parts[0] + parts[3];  // user + idle
            return (total, idle);
        }

        private double GetFallbackCpuUsage()
        {
            // eredeti folyamat CPU (cross-platform fallback)
            var process = Process.GetCurrentProcess();
            var now = DateTime.UtcNow;

            var cpuUsed = process.TotalProcessorTime - _lastCpuTime;
            var timeElapsed = now - _lastCheckTime;

            _lastCpuTime = process.TotalProcessorTime;
            _lastCheckTime = now;

            if (timeElapsed.TotalMilliseconds == 0)
                return 0;

            return Math.Round(
                cpuUsed.TotalMilliseconds /
                (Environment.ProcessorCount * timeElapsed.TotalMilliseconds) * 100, 2);
        }

        public double GetMemoryUsagePercent()
        {
#if WINDOWS
            return GetWindowsMemoryPercent();
#elif LINUX
            return GetLinuxMemoryPercent();
#else
            throw new PlatformNotSupportedException();
#endif
        }

        private double GetWindowsMemoryPercent()
        {
            if (_memCounter == null) return 0;
            _memCounter.NextValue();
            return Math.Round(_memCounter.NextValue(), 2);
        }

        private static double GetLinuxMemoryPercent()
        {
            var mem = File.ReadAllLines("/proc/meminfo")
                .Select(l => l.Split(':'))
                .ToDictionary(
                    p => p[0],
                    p => ulong.Parse(p[1].Trim().Split(' ')[0]));

            ulong total = mem["MemTotal"];
            ulong available = mem["MemAvailable"];

            return Math.Round((double)(total - available) / total * 100, 2);
        }

        public void Dispose()
        {
#if WINDOWS
            _cpuCounter?.Dispose();
            _memCounter?.Dispose();
#endif
        }
    }
}