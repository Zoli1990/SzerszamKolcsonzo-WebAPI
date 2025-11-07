// ============================================================================
// Features/Auth/Extensions/AuthServiceExtensions.cs
// ============================================================================

using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SzerszamKolcsonzo.Features.Auth.Data;
using SzerszamKolcsonzo.Features.Auth.Services;

namespace SzerszamKolcsonzo.Features.Auth.Extensions
{
    public static class AuthServiceExtensions
    {
        public static IServiceCollection AddAuthModule(this IServiceCollection services, IConfiguration configuration)
        {
            // AuthDbContext
            services.AddDbContext<AuthDbContext>(options =>
            {
                var connectionString = configuration.GetConnectionString("AuthConnection");
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
            });

            // Auth service
            services.AddScoped<IAuthService, AuthService>();

            // JWT Authentication
            var jwtSettings = configuration.GetSection("Jwt");
            var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey hiányzik!");

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
                };
            });

            return services;
        }

        public static async Task MigrateAuthDatabaseAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

            try
            {
                await context.Database.MigrateAsync();
                Console.WriteLine("✅ Auth adatbázis migrációja sikeres!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Auth migrációs hiba: {ex.Message}");
            }
        }
    }
}
