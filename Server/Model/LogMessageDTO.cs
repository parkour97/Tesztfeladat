namespace Server.Model
{
    public class LogMessageDTO
    {
        public string? Source { get; set; }
        public string? LogType { get; set; }  // "Error", "Information"
        public string Message { get; set; } = "";
        public DateTime? Timestamp { get; set; }
    }
}
