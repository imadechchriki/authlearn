using JwtAuthDotNet9.Data;
using Microsoft.EntityFrameworkCore;

namespace JwtAuthDotNet9.Services
{
    public class ProfileStatusService(UserDbContext context) : IProfileStatusService
    {
        public async Task<string> GetProfileStatusAsync(Guid userId)
        {
            var profile = await context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null ||
                string.IsNullOrEmpty(profile.PhoneNumber) ||
                string.IsNullOrEmpty(profile.CIN))
            {
                return "firstLogin";
            }

            return "profileCompleted";
        }
    }
}
