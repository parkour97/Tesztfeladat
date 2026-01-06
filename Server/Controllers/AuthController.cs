using Microsoft.AspNetCore.Mvc;
using Server.Interfaces;
using Server.Model.View;

namespace Server.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService userService;

        public AuthController(IUserService userService)
        {
            this.userService = userService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            var result = await userService.RegisterAsync(request);
            if (!result.Success) return BadRequest(result.Message);
            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var token = await userService.LoginAsync(request);
            if (token == null) return Unauthorized();
            return Ok(new { Token = token });
        }
    }
}
