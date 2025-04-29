using JwtAuthDotNet9.Entities;
using JwtAuthDotNet9.Models;

namespace JwtAuthDotNet9.Services
{
    public interface IAuthService
    {



        // Authentifier un utilisateur (login)
        Task<AuthResponseDTO?> LoginAsync(LoginDTO request);

        // Rafraîchir un AccessToken via un RefreshToken
        Task<AuthResponseDTO?> RefreshTokensAsync(RefreshTokenRequestDTO request);
    }
}
