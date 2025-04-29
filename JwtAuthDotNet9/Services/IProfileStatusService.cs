using System.Threading.Tasks;

namespace JwtAuthDotNet9.Services
{
    public interface IProfileStatusService
    {
        Task<string> GetProfileStatusAsync(Guid userId);
    }
}
