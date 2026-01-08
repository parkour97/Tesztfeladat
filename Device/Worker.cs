using Device.Control;
using Device.Model;
using System;
using System.Net;

namespace Device
{
    public class Worker : BackgroundService
    {
        private int interval = 60000;
        private int queueCapacity = 20;

        private readonly string ipAddress;
        private readonly int port;
        private readonly int recievePort;

        private readonly SystemUsageMonitor monitor;
        private readonly TCPSender sender;
        private readonly ILogger<Worker> logger;
        private readonly TCPLogger tcpLogger;
        private readonly SystemUsageQueue queue;

        public Worker(ILogger<Worker> logger, IConfiguration configuration)
        {
            ipAddress = configuration.GetValue<string>("TcpServer:IpAddress") ?? "0.0.0.0";
            port = configuration.GetValue<int>("TcpServer:Port");
            recievePort = configuration.GetValue<int>("TcpServer:RecievePort");
            monitor = new SystemUsageMonitor();
            sender = new TCPSender(ipAddress, port, recievePort, logger);
            sender.CommandReceived += OnServerCommand;
            this.logger = logger;
            tcpLogger = new TCPLogger(nameof(Worker), sender);

            queue = new SystemUsageQueue(capacity: queueCapacity);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (logger.IsEnabled(LogLevel.Information))
                logger.LogInformation("Windows monitoring worker started at: {time}", DateTimeOffset.Now);
            tcpLogger.LogInformation("Worker started");

            await sender.ConnectAsync(ct: default, maxRetries: 15);
            _ = sender.StartReceiveLoopAsync(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (logger.IsEnabled(LogLevel.Information))
                        logger.LogInformation("Getting Data {time}", DateTimeOffset.Now);
                    tcpLogger.LogInformation("Getting data");
                    var cpu = monitor.GetCpuUsagePercent();
                    double memPercentage = monitor.GetMemoryUsagePercent();

                    var dto = new SystemUsageDTO(
                        CpuPercent: cpu,
                        MemoryPercent: memPercentage,
                        Timestamp: DateTime.UtcNow
                    );

                    queue.Add(dto);

                    await sender.SendAsync(dto, stoppingToken);

                    LogBuffer();
                }
                catch (Exception ex)
                {
                    if (logger.IsEnabled(LogLevel.Information))
                        logger.LogError(ex, "Sending error");
                    tcpLogger.LogError(ex, "Something went wrong");
                }

                await Task.Delay(interval, stoppingToken);
            }
        }

        private void LogBuffer()
        {
            var snapshot = queue.Snapshot();

            logger.LogInformation("---- Last {Count} system usage samples ----", snapshot.Count);

            foreach (var s in snapshot)
            {
                logger.LogInformation(
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
                    interval = cmd.Value * 1000;
                    logger.LogInformation(
                        "Interval updated by server to {Interval} ms",
                        interval);
                    break;

                case "set_queue_size":
                    queue.Resize(cmd.Value);
                    logger.LogInformation(
                        "Queue size updated by server to {Size}",
                        cmd.Value);
                    break;

                default:
                    logger.LogWarning(
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
