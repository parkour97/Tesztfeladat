using Server.Data;
using Server.Model.Entity;

namespace Server.Services
{
    public class LogService
    {
        private readonly ServerDataContext _context;

        public LogService(ServerDataContext context)
        {
            _context = context;
        }

        public async Task LogAsync(string source, string logType, string message, DateTime timestamp)
        {
            var log = new Log
            {
                Source = source,
                LogType = logType,
                Message = message,
                Timestamp = timestamp
            };

            _context.Logs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}
