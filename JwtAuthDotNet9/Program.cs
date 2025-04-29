using JwtAuthDotNet9.Data;
using JwtAuthDotNet9.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using Scalar.AspNetCore;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddScoped<IProfileStatusService, ProfileStatusService>();
builder.Services.AddScoped<IProfileService, ProfileService>();

// ✅ Utiliser PostgreSQL au lieu de SQL Server
builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("UserDatabase"))
);

// Configuration de l'authentification JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = builder.Environment.IsProduction(); // Exiger HTTPS en production
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["AppSettings:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["AppSettings:Audience"],
        ValidateLifetime = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["AppSettings:Token"]!)
        ),
        ValidateIssuerSigningKey = true,
        ClockSkew = TimeSpan.Zero,
        // Vous pouvez également configurer LifetimeValidator pour une validation personnalisée
    };

    // Gestion des événements d'authentification (logging, etc.)
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            // Vous pouvez logger l'erreur ici
            if (context.Exception is SecurityTokenExpiredException)
            {
                context.Response.Headers.Add("Token-Expired", "true");
            }
            return Task.CompletedTask;
        }
    };
});

// Configuration des politiques d'autorisation
builder.Services.AddAuthorization(options =>
{
    // Politique par défaut - exige l'authentification
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    // Exemples de politiques basées sur les rôles
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));

    options.AddPolicy("UserOrAdmin", policy =>
        policy.RequireRole("User", "Admin"));

    // Désactiver le fallback pour des endpoints spécifiques comme Swagger
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

builder.Services.AddScoped<IAuthService, AuthService>();

var app = builder.Build();

// Seed database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbContext = services.GetRequiredService<UserDbContext>();
    await UserDbContext.SeedAsync(dbContext);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();

    // Permettre l'accès à Swagger sans authentification en développement
    app.UseWhen(
        context => context.Request.Path.StartsWithSegments("/swagger"),
        appBuilder => appBuilder.Use(async (context, next) =>
        {
            context.Response.Headers.Add("Authorization-Bypass", "true");
            await next();
        })
    );
}

app.UseHttpsRedirection();

// Middleware d'authentification AVANT l'autorisation
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Vous pouvez spécifier des routes qui ne nécessitent pas d'authentification
app.MapGet("/health", () => "Service is running")
   .AllowAnonymous();

app.Run();