using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Model.View;

namespace Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SystemUsageController : ControllerBase
    {
        private readonly ServerDataContext context;

        public SystemUsageController(ServerDataContext context)
        {
            this.context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<SystemUsageView>>> Get(
            [FromQuery] int deviceId,
            [FromQuery] DateTime from,
            [FromQuery] DateTime to)
        {
            if (from > to)
                return BadRequest("From date can't be later than To date.");

            var result = await context.SystemUsage
                .Where(su => su.DeviceId == deviceId &&
                             su.Created >= from &&
                             su.Created <= to)
                .OrderBy(su => su.Created).Join(
                context.Device,
                su => su.DeviceId,
                d => d.Id,
                (su, d) => new SystemUsageView
                {
                    Id = su.Id,
                    DeviceId = su.DeviceId,
                    DeviceName = d.Name,
                    MeasurementName = su.MeasurementName,
                    Usage = su.Usage,
                    Timestamp = su.Created
                }
            )
            .OrderBy(su => su.Timestamp).ToListAsync();

            return Ok(result);
        }
    }
}
