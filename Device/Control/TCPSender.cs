using Device.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Device.Control
{
    public class TCPSender : IDisposable
    {
        private readonly string host;
        private readonly int port;
        private readonly int recievePort;

        private TcpClient? client;
        private TcpListener? listener;
        private readonly CancellationTokenSource cts = new();
        private NetworkStream? stream;
        private StreamReader? reader;
        private ILogger logger;

        public event Action<ServerCommand>? CommandReceived;

        public TCPSender(string host, int port, int recievePort, ILogger logger)
        {
            this.host = host;
            this.port = port;
            this.recievePort = recievePort;
            this.logger = logger;
        }

        public async Task ConnectAsync(CancellationToken ct, int maxRetries = 10, int baseDelayMs = 1000)
        {
            var retryCount = 0;
            while (retryCount < maxRetries && !ct.IsCancellationRequested)
            {
                try
                {
                    client?.Dispose();
                    client = new TcpClient();
                    await client.ConnectAsync(host, port, ct);

                    stream = client.GetStream();

                    logger?.LogInformation("Connected to {Host}:{Port}", host, port);

                    return;
                }
                catch (Exception ex) when (ex is SocketException or ObjectDisposedException)
                {
                    retryCount++;
                    var delayMs = baseDelayMs * (int)Math.Pow(2, retryCount - 1);  // 1s, 2s, 4s, 8s...
                    logger?.LogWarning("Connection failed to {Host}:{Port} (retry {Retry}/{Max}): {Error}. Waiting {Delay}ms",
                        host, port, retryCount, maxRetries, ex.Message, delayMs);

                    await Task.Delay(delayMs, ct);
                }
            }

            throw new InvalidOperationException($"Failed to connect to {host}:{port} after {maxRetries} retries");
        }

        public Task StartReceiveLoopAsync(CancellationToken ct)
        {
            return Task.Run(async () =>
            {
                var ip = IPAddress.Parse(host);
                listener = new TcpListener(ip, recievePort);
                listener.Start();

                logger.LogInformation("TCP listener started on port {RecievePort}", recievePort);

                try
                {
                    while (!ct.IsCancellationRequested)
                    {
                        var client = await listener.AcceptTcpClientAsync(ct);
                        _ = HandleClientAsync(client, ct);
                    }
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "TCP receive loop crashed");
                }
                finally
                {
                    listener.Stop();
                }
            }, ct);
        }

        private async Task HandleClientAsync(TcpClient client, CancellationToken ct)
        {
            var clientIp = ((IPEndPoint)client.Client.RemoteEndPoint!).Address.ToString();

            try
            {
                logger.LogInformation("Client connected: {ClientIp}", clientIp);
                using var stream = client.GetStream();
                using var reader = new StreamReader(stream, Encoding.UTF8);

                while (!ct.IsCancellationRequested)
                {
                    var line = await reader.ReadLineAsync();
                    if (line == null) break;

                    HandleCommand(line);
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Client processing error");
            }
            finally
            {
                client.Dispose();
                logger.LogInformation("Client disconnected: {ClientIp}", clientIp);
            }
        }

        public async Task SendAsync<T>(T data, CancellationToken ct)
        {
            if (stream == null)
                throw new InvalidOperationException("TCP client is not connected.");

            var json = JsonSerializer.Serialize(data);
            var bytes = Encoding.UTF8.GetBytes(json + "\n");

            await stream.WriteAsync(bytes, ct);
        }

        private void HandleCommand(string line)
        {
            try
            {
                logger.LogInformation($"Message Recieved");
                var commands = JsonSerializer.Deserialize<ServerCommand[]>(line);
                if (commands == null) return;

                foreach (var cmd in commands)
                    CommandReceived?.Invoke(cmd);
            }
            catch (JsonException)
            {
            }
        }

        public void Dispose()
        {
            reader?.Dispose();
            stream?.Dispose();
            client?.Dispose();
        }
    }

}
