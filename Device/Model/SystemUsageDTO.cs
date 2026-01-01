using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Device.Model
{
    public record SystemUsageDTO(
        double CpuPercent,
        double MemoryPercent,
        DateTime Timestamp
    );
}
