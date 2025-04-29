
using JwtAuthDotNet9.Models;
using System.Threading.Tasks;

namespace JwtAuthDotNet9.Services
{
    public interface IProfileService
    {
        Task CompleteProfileAsync(Guid userId, CompleteProfileDTO profileDto);
    }
}
