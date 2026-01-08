namespace Device.Model
{
    public record SystemUsageDTO(
        double CpuPercent,
        double MemoryPercent,
        DateTime Timestamp
    );
}
