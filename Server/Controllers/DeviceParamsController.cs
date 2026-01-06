using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Model.View;

namespace Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DeviceParamsController : ControllerBase
    {
        private readonly ServerDataContext context;

        public DeviceParamsController(ServerDataContext context)
        {
            this.context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<DeviceParamView>>> GetDeviceParams()
        {
            var result = await context.DeviceParam
                .Join(
                    context.Device,
                    dp => dp.DeviceId,
                    d => d.Id,
                    (dp, d) => new DeviceParamView
                    {
                        Id = dp.Id,
                        Name = dp.Name,
                        DeviceId = dp.DeviceId,
                        DeviceName = d.Name,
                        Value = dp.Value
                    }
                )
                .OrderBy(dp => dp.DeviceId)
                .ThenBy(dp => dp.Name)
                .ToListAsync();

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Post(DeviceParamRecieve request)
        {
            // Megkeressük a DeviceParam rekordot
            var param = await context.DeviceParam
                .FirstOrDefaultAsync(dp => dp.Id == request.DeviceParameterId);

            if (param == null)
                return NotFound("DeviceParam not found.");

            // Frissítjük az értéket és a modifier mezőt
            param.Value = request.Value;
            param.Modifier = request.Username;

            await context.SaveChangesAsync();

            return Ok();
        }
    }
}
