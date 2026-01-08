using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Interfaces;
using Server.Model.Entity;
using Server.Model.View;
using Server.Services;

namespace Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DeviceParamsController : ControllerBase
    {
        private readonly ServerDataContext context;
        private readonly ITcpServerClient tcpClient;

        public DeviceParamsController(ServerDataContext context, ITcpServerClient tcpServer)
        {
            this.context = context;
            this.tcpClient = tcpServer;
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

            await tcpClient.SendDeviceDataAsync(param.DeviceId);

            return Ok();
        }
    }
}
