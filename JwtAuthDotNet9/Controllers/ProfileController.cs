
using JwtAuthDotNet9.Models;
using JwtAuthDotNet9.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
namespace JwtAuthDotNet9.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // JWT nécessaire
    public class ProfileController(IProfileService profileService) : ControllerBase
    {
        private readonly IProfileService _profileService = profileService;
        [HttpGet("whoami")]
        [Authorize]
        public IActionResult WhoAmI()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var email = User.FindFirstValue(JwtRegisteredClaimNames.Email);

            return Ok(new { userId, email });
        }


        [HttpPost("complete")]
        public async Task<IActionResult> CompleteProfile([FromBody] CompleteProfileDTO request)
        {
            var userIdString = User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            {
                return Unauthorized();
            }

            await _profileService.CompleteProfileAsync(userId, request);

            return Ok(new { message = "Profile completed successfully." });
        }
    }
}
