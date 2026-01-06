namespace Frontend.Models
{
    public class Device
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string IPAddress { get; set; } = string.Empty;
        public bool Connected { get; set; }
        public int MeasurementCount { get; set; }
        public List<SystemUsage> Usages { get; set; } = new List<SystemUsage>();
        public DateTime LastUpdate { get; set; }
    }
}
