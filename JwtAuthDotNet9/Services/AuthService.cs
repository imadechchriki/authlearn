using JwtAuthDotNet9.Data;
using JwtAuthDotNet9.Entities;
using JwtAuthDotNet9.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;


namespace JwtAuthDotNet9.Services
{
    public class AuthService(UserDbContext context, IConfiguration configuration) : IAuthService
    {
        

        public async Task<AuthResponseDTO?> LoginAsync(LoginDTO request)
        {
            var user = await context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user is null || !user.IsActive)
            {
                return null;
            }

            if (new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, request.Password)
                == PasswordVerificationResult.Failed)
            {
                return null;
            }

            return await CreateTokenResponse(user);
        }

        public async Task<AuthResponseDTO?> RefreshTokensAsync(RefreshTokenRequestDTO request)
        {
            var user = await ValidateRefreshTokenAsync(request.RefreshToken);
            if (user is null)
                return null;

            return await CreateTokenResponse(user);
        }

        private async Task<AuthResponseDTO> CreateTokenResponse(User user)
        {
            var accessToken = CreateToken(user);
            var refreshToken = await GenerateAndSaveRefreshTokenAsync(user);

            return new AuthResponseDTO
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                Expiration = DateTime.UtcNow.AddMinutes(15) // ou ce que tu veux pour AccessToken
            };
        }

        private async Task<User?> ValidateRefreshTokenAsync(string refreshToken)
        {
            var token = await context.RefreshTokens
                .Include(rt => rt.User)
                .ThenInclude(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken && rt.ExpiresAt > DateTime.UtcNow && rt.RevokedAt == null);

            return token?.User;
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private async Task<string> GenerateAndSaveRefreshTokenAsync(User user)
        {
            var refreshToken = GenerateRefreshToken();

            // Invalider tous les anciens tokens
            var existingTokens = await context.RefreshTokens.Where(rt => rt.UserId == user.Id && rt.RevokedAt == null).ToListAsync();
            foreach (var token in existingTokens)
            {
                token.RevokedAt = DateTime.UtcNow;
            }

            var newRefreshToken = new RefreshToken
            {
                Token = refreshToken,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow
            };

            context.RefreshTokens.Add(newRefreshToken);
            await context.SaveChangesAsync();

            return refreshToken;
        }

        private string CreateToken(User user)
        {
            // Claims de base pour l'identifiant utilisateur et l'email
            var claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),  // Subject (identifiant)
        new Claim(JwtRegisteredClaimNames.Email, user.Email),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),  // JWT ID unique
        new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())  // Issued At
    };

            // Ajouter les rôles
            var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();

            // Ajouter un claim spécifique 'role' pour le rôle principal (premier rôle)
            if (roles.Any())
            {
                claims.Add(new Claim("role", roles.First()));
            }

            // Ajouter tous les rôles dans un claim séparé pour la compatibilité avec ClaimTypes.Role
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // Pour une meilleure compatibilité avec différents systèmes, ajoutons aussi un claim avec tous les rôles au format JSON
            if (roles.Any())
            {
                claims.Add(new Claim("roles", System.Text.Json.JsonSerializer.Serialize(roles)));
            }

            // Ajouter des informations utilisateur supplémentaires si nécessaire
            // Par exemple : claims.Add(new Claim("firstName", user.FirstName ?? string.Empty));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["AppSettings:Token"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var tokenExpirationMinutes = 15; // Vous pouvez le déplacer dans la configuration

            var token = new JwtSecurityToken(
                issuer: configuration["AppSettings:Issuer"],
                audience: configuration["AppSettings:Audience"],
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddMinutes(tokenExpirationMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}
