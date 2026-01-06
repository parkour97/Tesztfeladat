using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Data;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ServerDataContext context;

        public UsersController(ServerDataContext context)
        {
            this.context = context;
        }

        [HttpGet(Name = "AllUsers")]
        public async Task<IActionResult> Get()
        {
            return Ok(await context.User.ToListAsync());
        }
    }
}
