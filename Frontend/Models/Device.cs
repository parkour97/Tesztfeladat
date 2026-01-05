namespace Frontend.Models
{
    public class Device
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string IPAddress { get; set; }
        public bool Connected { get; set; }
        public double Usage { get; set; }
        public DateTime LastUpdate { get; set; }
    }
}
