using Device.Model;
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
        private readonly string _host;
        private readonly int _port;

        private TcpClient? _client;
        private NetworkStream? _stream;
        private StreamReader? _reader;

        public event Action<ServerCommand>? CommandReceived;

        public TCPSender(string host, int port)
        {
            _host = host;
            _port = port;
        }

        public async Task ConnectAsync(CancellationToken ct)
        {
            _client = new TcpClient();
            await _client.ConnectAsync(_host, _port, ct);

            _stream = _client.GetStream();
            _reader = new StreamReader(_stream, Encoding.UTF8);

            // háttérben figyeljük a szerver üzeneteit
            _ = Task.Run(() => ListenAsync(ct), ct);
        }

        public async Task SendAsync<T>(T data, CancellationToken ct)
        {
            if (_stream == null)
                throw new InvalidOperationException("TCP client is not connected.");

            var json = JsonSerializer.Serialize(data);
            var bytes = Encoding.UTF8.GetBytes(json + "\n");

            await _stream.WriteAsync(bytes, ct);
        }

        private async Task ListenAsync(CancellationToken ct)
        {
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    var line = await _reader!.ReadLineAsync();
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    HandleCommand(line);
                }
            }
            catch (Exception)
            {
            }
        }

        private void HandleCommand(string line)
        {
            try
            {
                var cmd = JsonSerializer.Deserialize<ServerCommand>(line);
                if (cmd == null)
                    return;

                CommandReceived?.Invoke(cmd);
            }
            catch (JsonException)
            {
            }
        }

        public void Dispose()
        {
            _reader?.Dispose();
            _stream?.Dispose();
            _client?.Dispose();
        }
    }

}
