using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Device.Model
{
    public class LogMessage
    {
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;
        public string Level { get; init; } = "";
        public string Message { get; init; } = "";
    }
}
