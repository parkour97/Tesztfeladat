namespace Frontend.Models
{
    public class Device
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public double Usage { get; set; }
        public DateTime LastUpdate { get; set; }
    }
}
