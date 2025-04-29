using JwtAuthDotNet9.Models;

using JwtAuthDotNet9.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JwtAuthDotNet9.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDTO request)
        {
            var tokenResponse = await authService.LoginAsync(request);
            if (tokenResponse == null)
            {
                return Unauthorized(new { message = "Invalid credentials." });
            }
            return Ok(tokenResponse);
        }

        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDTO request)
        {
            var tokenResponse = await authService.RefreshTokensAsync(request);
            if (tokenResponse == null)
            {
                return Unauthorized(new { message = "Invalid or expired refresh token." });
            }
            return Ok(tokenResponse);
        }
    }
}
