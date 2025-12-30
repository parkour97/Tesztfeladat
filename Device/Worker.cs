using System;

namespace Device
{
    public class Worker : BackgroundService
    {
        private int _interval;

        private readonly SystemUsageMonitor _monitor;
        private readonly TCPSender _sender;
        private readonly ILogger<Worker> _logger;
        private readonly SystemUsageQueue _queue;

        public Worker(ILogger<Worker> logger)
        {
            _monitor = new SystemUsageMonitor();
            _sender = new TCPSender("127.0.0.1", 9000);
            _sender.CommandReceived += OnServerCommand;
            _logger = logger;

            
            _queue = new SystemUsageQueue(capacity: 20); // N méretű sor
            _interval = 5000;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("Windows monitoring worker started at: {time}", DateTimeOffset.Now);

            await _sender.ConnectAsync(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var cpu = _monitor.GetCpuUsagePercent();
                    double memPercentage = _monitor.GetMemoryUsagePercent();

                    var dto = new SystemUsageDTO(
                        CpuPercent: cpu,
                        MemoryPercent: memPercentage,
                        Timestamp: DateTime.UtcNow
                    );

                    _queue.Add(dto);

                    await _sender.SendAsync(dto, stoppingToken);

                    LogBuffer();
                }
                catch (Exception ex)
                {
                    if (_logger.IsEnabled(LogLevel.Information))
                        _logger.LogError(ex, "Sending error");
                }

                await Task.Delay(_interval, stoppingToken);
            }
        }

        private void LogBuffer()
        {
            var snapshot = _queue.Snapshot();

            _logger.LogInformation("---- Last {Count} system usage samples ----", snapshot.Count);

            foreach (var s in snapshot)
            {
                _logger.LogInformation(
                    "{Time} | CPU: {Cpu}% | MEM: {Mem}%",
                    s.Timestamp,
                    s.CpuPercent,
                    s.MemoryPercent);
            }
        }

        private void OnServerCommand(ServerCommand cmd)
        {
            switch (cmd.Type.ToLowerInvariant())
            {
                case "set_interval":
                    _interval = cmd.Value;
                    _logger.LogInformation(
                        "Interval updated by server to {Interval} ms",
                        _interval);
                    break;

                case "set_queue_size":
                    _queue.Resize(cmd.Value);
                    _logger.LogInformation(
                        "Queue size updated by server to {Size}",
                        cmd.Value);
                    break;

                default:
                    _logger.LogWarning(
                        "Unknown command type received: {Type}",
                        cmd.Type);
                    break;
            }
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}
