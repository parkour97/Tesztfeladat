using Device.Model;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
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

        private TcpClient? client;
        private NetworkStream? stream;
        private StreamReader? reader;

        public event Action<ServerCommand>? CommandReceived;

        public TCPSender(string host, int port)
        {
            this.host = host;
            this.port = port;
        }

        public async Task ConnectAsync(CancellationToken ct, ILogger logger, int maxRetries = 10, int baseDelayMs = 1000)
        {
            var retryCount = 0;
            while (retryCount < maxRetries && !ct.IsCancellationRequested)
            {
                try
                {
                    client?.Dispose();  // előző kapcsolat tisztítása
                    client = new TcpClient();
                    await client.ConnectAsync(host, port, ct);

                    stream = client.GetStream();
                    reader = new StreamReader(stream, Encoding.UTF8);

                    logger?.LogInformation("Connected to {Host}:{Port}", host, port);

                    // Listener indítás
                    _ = Task.Run(() => ListenAsync(ct), ct);
                    return;  // siker!
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

        public async Task SendAsync<T>(T data, CancellationToken ct)
        {
            if (stream == null)
                throw new InvalidOperationException("TCP client is not connected.");

            var json = JsonSerializer.Serialize(data);
            var bytes = Encoding.UTF8.GetBytes(json + "\n");

            await stream.WriteAsync(bytes, ct);
        }

        private async Task ListenAsync(CancellationToken ct)
        {
            try
            {
                while (!ct.IsCancellationRequested && reader != null)
                {
                    var line = await reader.ReadLineAsync();
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    HandleCommand(line);
                }
            }
            catch (Exception ex)  // logold!
            {
                Console.WriteLine($"Listen error: {ex.Message}");  // vagy logger
            }
        }

        private void HandleCommand(string line)
        {
            try
            {
                var commands = JsonSerializer.Deserialize<ServerCommand[]>(line);  // TÖBB command!
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
