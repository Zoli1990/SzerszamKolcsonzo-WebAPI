using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using SzerszamKolcsonzo.Data;
using SzerszamKolcsonzo.Features.Auth.Extensions;
using SzerszamKolcsonzo.Features.Push.Extensions;
using SzerszamKolcsonzo.Features.Push.Services;
using SzerszamKolcsonzo.Features.ToolRental.Extensions;
using SzerszamKolcsonzo.Services;

var builder = WebApplication.CreateBuilder(args);

// =====================
// Controllers + JSON config
// =====================
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

// =====================
// Modulok regisztrációja
// =====================
builder.Services.AddAuthModule(builder.Configuration);
builder.Services.AddToolRentalModule(builder.Configuration);
builder.Services.AddPushModule(builder.Configuration);

// =====================
// Background services
// =====================
builder.Services.AddHostedService<FoglalasCleanupService>();
builder.Services.AddHostedService<FoglalasStatusUpdateService>();

// =====================
// CORS - Development & Production
// =====================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// =====================
// Swagger (csak Development-ben)
// =====================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Szerszámkölcsönző API",
        Version = "v1",
        Description = ".NET 8 WebAPI - Szerszámkölcsönző + Auth + PWA"
    });

    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// =====================
// Build app
// =====================
var app = builder.Build();

// =====================
// Adatbázis migráció automatikusan
// =====================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        // Auth DB migráció
        var authContext = services.GetRequiredService<SzerszamKolcsonzo.Features.Auth.Data.AuthDbContext>();
        await authContext.Database.MigrateAsync();
        logger.LogInformation("✅ Auth adatbázis migrációja sikeres!");

        // App DB migráció
        var appContext = services.GetRequiredService<AppDbContext>();
        await appContext.Database.MigrateAsync();
        logger.LogInformation("✅ Szerszámkölcsönző adatbázis migrációja sikeres!");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "❌ Hiba történt az adatbázis migráció során!");
        throw;
    }
}

// =====================
// Middleware pipeline (HELYES SORREND!)
// =====================

// Swagger (csak Development-ben)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Statikus fájlok (wwwroot) - ELSŐ helyen!
app.UseStaticFiles();

// HTTPS redirect (Production-ben ajánlott)
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// CORS
app.UseCors("AllowAll");

// Authentikáció & Authorizáció
app.UseAuthentication();
app.UseAuthorization();

// Controllers
app.MapControllers();

// =====================
// App futtatása
// =====================
app.Run();