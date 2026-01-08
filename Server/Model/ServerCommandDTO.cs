namespace Server.Model
{
    public class ServerCommand
    {
        public string Type { get; set; } = "";    // "set_interval", "set_queue_size"
        public int Value { get; set; }
    }
}
