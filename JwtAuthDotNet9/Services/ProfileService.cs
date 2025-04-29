using JwtAuthDotNet9.Data;

using JwtAuthDotNet9.Entities;
using JwtAuthDotNet9.Models;
using Microsoft.EntityFrameworkCore;

namespace JwtAuthDotNet9.Services
{
    public class ProfileService(UserDbContext context) : IProfileService
    {
        public async Task CompleteProfileAsync(Guid userId, CompleteProfileDTO profileDto)
        {
            var existingProfile = await context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);

            if (existingProfile != null)
            {
                existingProfile.PhoneNumber = profileDto.PhoneNumber;
                existingProfile.CIN = profileDto.CIN;
                existingProfile.ProfilePictureUrl = profileDto.ProfilePictureUrl;
                existingProfile.BirthDate = profileDto.BirthDate;
                existingProfile.Address = profileDto.Address;
                existingProfile.AdditionalInfos = profileDto.AdditionalInfos;
            }
            else
            {
                var newProfile = new UserProfile
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    PhoneNumber = profileDto.PhoneNumber,
                    CIN = profileDto.CIN,
                    ProfilePictureUrl = profileDto.ProfilePictureUrl,
                    BirthDate = profileDto.BirthDate,
                    Address = profileDto.Address,
                    AdditionalInfos = profileDto.AdditionalInfos
                };

                context.UserProfiles.Add(newProfile);
            }

            await context.SaveChangesAsync();
        }
    }
}
