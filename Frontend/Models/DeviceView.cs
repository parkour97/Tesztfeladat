namespace Frontend.Models
{
    public class DeviceView
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string IPAddress { get; set; } = string.Empty;
        public bool Connected { get; set; }
        public int MeasurementCount { get; set; }
        public List<SystemUsageView> Usages { get; set; } = new List<SystemUsageView>();
        public DateTime LastUpdate { get; set; }
    }
}
