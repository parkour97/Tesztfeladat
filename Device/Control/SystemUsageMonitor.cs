using System.Diagnostics;

namespace Device.Control
{
    public class SystemUsageMonitor
    {
        private TimeSpan _lastCpuTime;
        private DateTime _lastCheckTime;

        public SystemUsageMonitor()
        {
        }

        public double GetCpuUsagePercent()
        {
            var process = Process.GetCurrentProcess();
            var now = DateTime.UtcNow;

            var cpuUsed = process.TotalProcessorTime - _lastCpuTime;
            var timeElapsed = now - _lastCheckTime;

            _lastCpuTime = process.TotalProcessorTime;
            _lastCheckTime = now;

            if (timeElapsed.TotalMilliseconds == 0)
                return 0;

            var cpuUsageTotal =
                cpuUsed.TotalMilliseconds /
                (Environment.ProcessorCount * timeElapsed.TotalMilliseconds);

            return Math.Round(cpuUsageTotal * 100, 2);
        }

        public double GetMemoryUsagePercent()
        {
            if (OperatingSystem.IsWindows())
                return GetWindowsMemoryPercent();

            if (OperatingSystem.IsLinux())
                return GetLinuxMemoryPercent();

            throw new PlatformNotSupportedException();
        }

        private static double GetWindowsMemoryPercent()
        {
            using var counter = new PerformanceCounter(
                "Memory",
                "% Committed Bytes In Use");

            // warm-up
            counter.NextValue();
            Thread.Sleep(100);

            return Math.Round(counter.NextValue(), 2);
        }

        private static double GetLinuxMemoryPercent()
        {
            var mem = File.ReadAllLines("/proc/meminfo")
                .Select(l => l.Split(':'))
                .ToDictionary(
                    p => p[0],
                    p => ulong.Parse(
                        p[1].Trim().Split(' ')[0]));

            ulong total = mem["MemTotal"];
            ulong available = mem["MemAvailable"];

            return Math.Round(
                (double)(total - available) / total * 100,
                2);
        }
    }
}
