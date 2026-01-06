using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Model.View;

namespace Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DevicesController : ControllerBase
    {
        private readonly ServerDataContext context;

        public DevicesController(ServerDataContext context)
        {
            this.context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<DeviceView>>> GetDevices()
        {
            var result = await context.Device
            .Select(device => new DeviceView
            {
                Id = device.Id,
                Name = device.Name,
                IPAddress = device.IPAddress ?? string.Empty,
                Connected = device.Connected,
                MeasurementCount = device.MeasurementCount ?? 0,
                // Usages: legfrissebb "MeasurementCount" számú rekord
                Usages = context.SystemUsage
                    .Where(u => u.DeviceId == device.Id)
                    .OrderByDescending(u => u.Created)
                    .Take(device.MeasurementCount ?? 0)
                    .Select(u => new SystemUsageView
                    {
                        Id = u.Id,
                        DeviceId = u.DeviceId,
                        DeviceName = device.Name,
                        MeasurementName = u.MeasurementName,
                        Usage = u.Usage,
                        Timestamp = u.Created
                    })
                    .ToList(),
                // LastUpdate: a legutolsó mérés ideje
                LastUpdate = context.SystemUsage
                    .Where(u => u.DeviceId == device.Id)
                    .OrderByDescending(u => u.Created)
                    .Select(u => (DateTime?)u.Created)
                    .FirstOrDefault() ?? DateTime.MinValue
            })
            .ToListAsync();

            return Ok(result);
        }
    }
}
