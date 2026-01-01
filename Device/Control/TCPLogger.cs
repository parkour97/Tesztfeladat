using Device.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Device.Control
{
    public class TCPLogger : ILogger
    {
        private readonly string _category;
        private readonly TCPSender _sender;

        public TCPLogger(string category, TCPSender sender)
        {
            _category = category;
            _sender = sender;
        }

        public IDisposable? BeginScope<TState>(TState state) => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            var msg = new LogMessage
            {
                Timestamp = DateTime.UtcNow,
                Level = logLevel.ToString(),
                Message = formatter(state, exception)
            };

            // aszinkron küldés, fire-and-forget
            _ = _sender.SendAsync(msg, CancellationToken.None);
        }
    }
}
