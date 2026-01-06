namespace Server.Model.View
{
    public class SystemUsageView
    {
        public int Id { get; set; }
        public int DeviceId { get; set; }
        public string DeviceName { get; set; } = string.Empty;
        public string MeasurementName { get; set; } = string.Empty;
        public double Usage { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
