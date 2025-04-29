using JwtAuthDotNet9.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace JwtAuthDotNet9.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] // Spécifie explicitement le schéma Bearer
    public class ProfileStatusController : ControllerBase
    {
        private readonly IProfileStatusService _profileStatusService;

        public ProfileStatusController(IProfileStatusService profileStatusService)
        {
            _profileStatusService = profileStatusService;
        }

        [HttpGet("check-profile-status")]
        public async Task<IActionResult> CheckProfileStatus()
        {
            // Récupération de l'ID utilisateur depuis les claims du token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");

            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized("Token invalide ou ID utilisateur manquant");
            }

            try
            {
                var status = await _profileStatusService.GetProfileStatusAsync(userId);
                return Ok(new { status });
            }
            catch (Exception ex)
            {
                // Log l'erreur si nécessaire
                return StatusCode(500, new { error = "Une erreur est survenue lors de la vérification du statut du profil" });
            }
        }

        // Exemple d'endpoint avec autorisation basée sur les rôles
        [HttpGet("admin-stats")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public IActionResult GetAdminStats()
        {
            // Logique pour récupérer les statistiques d'administrateur
            return Ok(new { message = "Statistiques d'administration accessibles uniquement aux administrateurs" });
        }
    }
}