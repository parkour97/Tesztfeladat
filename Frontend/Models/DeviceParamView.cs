namespace Frontend.Models
{
    public class DeviceParamView
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int DeviceId { get; set; }
        public string DeviceName { get; set; } = string.Empty;
        public int Value { get; set; }
    }

    public record DeviceParamSend(int DeviceParameterId, int Value, string Username);
}
