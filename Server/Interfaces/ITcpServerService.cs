namespace Server.Interfaces
{
    public interface ITcpServerClient
    {
        Task SendDeviceDataAsync(int deviceId);
    }
}
