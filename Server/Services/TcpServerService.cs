using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Interfaces;
using Server.Model;
using Server.Model.Entity;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Net.NetworkInformation;

namespace Server.Services
{
    public class TcpServerService : BackgroundService, ITcpServerClient, IDisposable
    {
        private readonly ILogger<TcpServerService> logger;
        private TcpListener? listener;
        private readonly CancellationTokenSource cts = new();
        private readonly string ipAddress;
        private readonly int port;
        private readonly int sendPort;
        private readonly IServiceScopeFactory serviceScopeFactory;
        private readonly ConcurrentDictionary<string, TcpClient> clients = new();
        private readonly int monitorIntervalSeconds;
        private readonly int pingTimeoutMs;

        public TcpServerService(ILogger<TcpServerService> logger, IConfiguration configuration, IServiceScopeFactory serviceScopeFactory)
        {
            this.logger = logger;
            this.serviceScopeFactory = serviceScopeFactory;
            ipAddress = configuration.GetValue<string>("TcpServer:IpAddress") ?? "0.0.0.0";
            port = configuration.GetValue<int>("TcpServer:Port");
            sendPort = configuration.GetValue<int>("TcpServer:SendPort");

            monitorIntervalSeconds = configuration.GetValue<int>("DeviceMonitor:IntervalSeconds", 5);

            pingTimeoutMs = configuration.GetValue<int>("DeviceMonitor:PingTimeoutMs", 1000);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _ = Task.Run(() => MonitorDevicesAsync(stoppingToken), stoppingToken);

            var ip = IPAddress.Parse(ipAddress);
            listener = new TcpListener(ip, port);
            listener.Start();

            logger.LogInformation("TCP server started on port {Port}", port);

            while (!cts.Token.IsCancellationRequested)
            {
                try
                {
                    var client = await listener.AcceptTcpClientAsync(cts.Token);
                    _ = Task.Run(() => HandleClientAsync(client), cts.Token);
                }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error on accepting messages from client");
                }
            }
        }

        private async Task HandleClientAsync(TcpClient client)
        {
            var clientIp = ((IPEndPoint)client.Client.RemoteEndPoint!).Address.ToString();

            try
            {
                clients[clientIp] = client;
                logger.LogInformation("Client connected: {ClientIp}", clientIp);
                using var stream = client.GetStream();
                using var reader = new StreamReader(stream, Encoding.UTF8);
                string? line;

                while ((line = await reader.ReadLineAsync()) != null && !cts.Token.IsCancellationRequested)
                {
                    using var scope = serviceScopeFactory.CreateScope();  // scoped DbContext minden üzenethez
                    var context = scope.ServiceProvider.GetRequiredService<ServerDataContext>();
                    await ProcessMessageAsync(line, clientIp, context);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in processing message");
            }
            finally
            {
                clients.TryRemove(clientIp, out _);
                client.Dispose();
                logger.LogInformation("Client disconnected: {ClientIp}", clientIp);
            }
        }

        private async Task ProcessMessageAsync(string json, string clientIp, ServerDataContext context)
        {
            try
            {
                if (await ProcessSystemUsageAsync(json, clientIp, context))
                    return;
                // Ha nem illik, akkor Log-ként
                if (JsonSerializer.Deserialize<LogMessageDTO>(json, new JsonSerializerOptions()) is LogMessageDTO logDto)
                {
                    var device = await context.Device.FirstOrDefaultAsync(d => d.IPAddress == clientIp);

                    if (device == null)
                    {
                        device = new Device
                        {
                            Name = $"Device-{clientIp}",
                            IPAddress = clientIp,
                            Connected = true,
                            MeasurementCount = 0
                        };
                        context.Device.Add(device);
                        await context.SaveChangesAsync();

                        var deviceParam = new DeviceParam()
                        {
                            Name = "Interval",
                            Value = 60
                        };
                        context.DeviceParam.Add(deviceParam);
                        await context.SaveChangesAsync();

                        deviceParam = new DeviceParam()
                        {
                            Name = "Queue_size",
                            Value = 20
                        };
                        context.DeviceParam.Add(deviceParam);
                        await context.SaveChangesAsync();
                    }

                    var logEntity = new Log
                    {
                        Source = logDto.Source ?? device.Name,
                        LogType = logDto.LogType ?? "Information",
                        Message = logDto.Message,
                        Timestamp = logDto.Timestamp ?? DateTime.UtcNow,
                        Created = DateTime.UtcNow
                    };
                    context.Logs.Add(logEntity);
                    await context.SaveChangesAsync();

                    return;
                }
                logger.LogDebug("Message not recognisable: {Json}", json[..Math.Min(100, json.Length)]);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Processing error: {Json}", json);
            }
        }

        private async Task<bool> ProcessSystemUsageAsync(string json, string clientIp, ServerDataContext context)
        {
            try
            {
                using var document = JsonDocument.Parse(json);
                var root = document.RootElement;

                if (root.ValueKind != JsonValueKind.Object)
                    return false;

                // Device keresése IP alapján
                var device = await context.Device.FirstOrDefaultAsync(d => d.IPAddress == clientIp);
                if (device == null)
                {
                    device = new Device
                    {
                        Name = $"Device-{clientIp}",
                        IPAddress = clientIp,
                        Connected = true,
                        MeasurementCount = 0
                    };
                    context.Device.Add(device);
                    await context.SaveChangesAsync();  // ID generálás után

                    var deviceParam = new DeviceParam()
                    {
                        DeviceId = device.Id,
                        Name="Interval",
                        Value=60
                    };
                    context.DeviceParam.Add(deviceParam);
                    await context.SaveChangesAsync();

                    deviceParam = new DeviceParam()
                    {
                        DeviceId = device.Id,
                        Name = "Queue_size",
                        Value = 20
                    };
                    context.DeviceParam.Add(deviceParam);
                    await context.SaveChangesAsync();
                }

                var timestamp = root.TryGetProperty("Timestamp", out var tsProp)
                    ? DateTime.Parse(tsProp.GetString() ?? "")
                    : DateTime.UtcNow;

                var measurements = new List<SystemUsage>();

                // Minden property-t végigmegyünk (timestamp kivételével)
                foreach (var prop in root.EnumerateObject())
                {
                    if (prop.Name.Equals("Timestamp", StringComparison.OrdinalIgnoreCase))
                        continue;

                    if (prop.Value.ValueKind == JsonValueKind.Number &&
                        prop.Value.TryGetDouble(out double doubleValue))
                    {
                        float value = (float)doubleValue;  // double -> float safe konverzió

                        measurements.Add(new SystemUsage
                        {
                            DeviceId = device.Id,
                            MeasurementName = prop.Name,
                            Usage = value,
                            Timestamp = timestamp
                        });
                    }
                }

                if (measurements.Count > 0)
                {
                    context.SystemUsage.AddRange(measurements);
                    device.MeasurementCount = measurements.Count;
                    device.Connected = true;
                    await context.SaveChangesAsync();

                    logger.LogInformation("Saved {Count} records DeviceId={Id}: {Names}",
                        measurements.Count, device.Id,
                        string.Join(", ", measurements.Select(m => $"{m.MeasurementName}={m.Usage:F1}%")));
                    return true;
                }
            }
            catch (JsonException)
            {
                // Érvénytelen JSON -> nem SystemUsage
            }
            return false;
        }

        public async Task SendDeviceDataAsync(int deviceId)
        {
            using var scope = serviceScopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ServerDataContext>();

            // Device IP lekérdezése
            var device = await context.Device
                .FirstOrDefaultAsync(d => d.Id == deviceId);

            if (device?.IPAddress == null)
            {
                logger.LogWarning("DeviceId {DeviceId} ha no IP address", deviceId);
                return;
            }

            var paramsData = await context.DeviceParam
                .Where(p => p.DeviceId == deviceId)
                .Select(p => new ServerCommand
                {
                    Type = "set_" + p.Name.ToLower(),
                    Value = p.Value
                })
                .ToListAsync();

            if (!paramsData.Any())
            {
                logger.LogDebug("No parameters found for DeviceId {DeviceId}", deviceId);
                return;
            }

            _ = Task.Run(async () =>
            {
                try
                {
                    using var tempClient = new TcpClient();
                    await tempClient.ConnectAsync(device.IPAddress, sendPort);
                    using var stream = tempClient.GetStream();
                    var json = JsonSerializer.Serialize(paramsData) + "\r\n";
                    var bytes = Encoding.UTF8.GetBytes(json);
                    await stream.WriteAsync(bytes);
                    await stream.FlushAsync();
                    logger.LogInformation("Params sent to {Ip}:{SendPort} (DeviceId {DeviceId})",
                        device.IPAddress, sendPort, device.Id);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to send to {Ip}:{Port}", device.IPAddress, port);
                }
            });
        }

        private async Task MonitorDevicesAsync(CancellationToken token)
        {
            using var ping = new Ping();

            while (!token.IsCancellationRequested)
            {
                try
                {
                    using var scope = serviceScopeFactory.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<ServerDataContext>();

                    var devices = await context.Device.ToListAsync(token);

                    foreach (var device in devices)
                    {
                        bool isConnected = false;

                        if (!string.IsNullOrWhiteSpace(device.IPAddress))
                        {
                            try
                            {
                                var reply = await ping.SendPingAsync(device.IPAddress, pingTimeoutMs);
                                isConnected = reply.Status == IPStatus.Success;
                            }
                            catch
                            {
                                isConnected = false;
                            }
                        }

                        if (device.Connected != isConnected)
                        {
                            device.Connected = isConnected;
                        }
                    }

                    await context.SaveChangesAsync(token);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Device ping monitor error");
                }

                await Task.Delay(TimeSpan.FromSeconds(monitorIntervalSeconds), token);
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            cts.Cancel();
            listener?.Stop();
            await base.StopAsync(cancellationToken);
        }

        public void Dispose()
        {
            cts.Dispose();
            listener?.Dispose();
        }
    }
}
