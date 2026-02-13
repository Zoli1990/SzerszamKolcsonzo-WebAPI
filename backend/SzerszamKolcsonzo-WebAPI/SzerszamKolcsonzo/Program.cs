using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using SzerszamKolcsonzo.Data;
using SzerszamKolcsonzo.Features.Auth.Extensions;
using SzerszamKolcsonzo.Features.Push.Extensions;
using SzerszamKolcsonzo.Features.Push.Services;
using SzerszamKolcsonzo.Features.ToolRental.Extensions;
using SzerszamKolcsonzo.Services;

// ═══════════════════════════════════════════════════════════════════════════
// ✅ ROBUSZTUS PROGRAM.CS - AUTOMATIKUS SETUP
// ═══════════════════════════════════════════════════════════════════════════

var builder = WebApplication.CreateBuilder(args);

// ═══════════════════════════════════════════════════════════════════════════
// LOGGING KONFIGURÁCIÓ
// ═══════════════════════════════════════════════════════════════════════════
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

var logger = LoggerFactory.Create(config => config.AddConsole())
    .CreateLogger<Program>();

logger.LogInformation("🚀 Szerszámkölcsönző indítása...");
logger.LogInformation("🔧 Környezet: {Environment}", builder.Environment.EnvironmentName);

// ═══════════════════════════════════════════════════════════════════════════
// CONTROLLERS + JSON
// ═══════════════════════════════════════════════════════════════════════════
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

// ═══════════════════════════════════════════════════════════════════════════
// MODULOK REGISZTRÁCIÓJA
// ═══════════════════════════════════════════════════════════════════════════
try
{
    logger.LogInformation("📦 Modulok betöltése...");
    builder.Services.AddAuthModule(builder.Configuration);
    logger.LogInformation("  ✅ Auth modul betöltve");
    
    builder.Services.AddToolRentalModule(builder.Configuration);
    logger.LogInformation("  ✅ ToolRental modul betöltve");
    
    builder.Services.AddPushModule(builder.Configuration);
    logger.LogInformation("  ✅ Push modul betöltve");
}
catch (Exception ex)
{
    logger.LogCritical(ex, "❌ Hiba a modulok betöltése során!");
    throw;
}

// ═══════════════════════════════════════════════════════════════════════════
// BACKGROUND SERVICES
// ═══════════════════════════════════════════════════════════════════════════
builder.Services.AddHostedService<FoglalasCleanupService>();
builder.Services.AddHostedService<FoglalasStatusUpdateService>();
logger.LogInformation("⏰ Background services regisztrálva");

// ═══════════════════════════════════════════════════════════════════════════
// CORS
// ═══════════════════════════════════════════════════════════════════════════
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
logger.LogInformation("🌐 CORS konfiguráció: AllowAll");

// ═══════════════════════════════════════════════════════════════════════════
// SWAGGER
// ═══════════════════════════════════════════════════════════════════════════
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

// ═══════════════════════════════════════════════════════════════════════════
// BUILD APP
// ═══════════════════════════════════════════════════════════════════════════
var app = builder.Build();
logger.LogInformation("🏗️  Application built sikeresen");

// ═══════════════════════════════════════════════════════════════════════════
// ✅ AUTOMATIKUS SETUP - MAPPÁK, ADATBÁZIS, MIGRATIONS
// ═══════════════════════════════════════════════════════════════════════════
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var setupLogger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        // ─────────────────────────────────────────────────────────────────
        // 1. WWWROOT/UPLOADS MAPPA LÉTREHOZÁSA
        // ─────────────────────────────────────────────────────────────────
        var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        if (!Directory.Exists(uploadsPath))
        {
            Directory.CreateDirectory(uploadsPath);
            setupLogger.LogInformation("📁 'wwwroot/uploads' mappa létrehozva: {Path}", uploadsPath);
        }
        else
        {
            setupLogger.LogInformation("📁 'wwwroot/uploads' mappa már létezik");
        }

        // ─────────────────────────────────────────────────────────────────
        // 2. AUTH ADATBÁZIS MIGRÁCIÓ
        // ─────────────────────────────────────────────────────────────────
        setupLogger.LogInformation("🗄️  Auth adatbázis ellenőrzése...");
        var authContext = services.GetRequiredService<SzerszamKolcsonzo.Features.Auth.Data.AuthDbContext>();
        
        var authPendingMigrations = await authContext.Database.GetPendingMigrationsAsync();
        if (authPendingMigrations.Any())
        {
            setupLogger.LogWarning("  ⚠️  {Count} függőben lévő Auth migration", authPendingMigrations.Count());
            setupLogger.LogInformation("  🔄 Migrations futtatása...");
            await authContext.Database.MigrateAsync();
            setupLogger.LogInformation("  ✅ Auth adatbázis migrálva!");
        }
        else
        {
            setupLogger.LogInformation("  ✅ Auth adatbázis naprakész (0 migration)");
        }

        // ─────────────────────────────────────────────────────────────────
        // 3. APP ADATBÁZIS MIGRÁCIÓ
        // ─────────────────────────────────────────────────────────────────
        setupLogger.LogInformation("🗄️  Szerszámkölcsönző adatbázis ellenőrzése...");
        var appContext = services.GetRequiredService<AppDbContext>();
        
        var appPendingMigrations = await appContext.Database.GetPendingMigrationsAsync();
        if (appPendingMigrations.Any())
        {
            setupLogger.LogWarning("  ⚠️  {Count} függőben lévő App migration", appPendingMigrations.Count());
            setupLogger.LogInformation("  🔄 Migrations futtatása...");
            await appContext.Database.MigrateAsync();
            setupLogger.LogInformation("  ✅ Szerszámkölcsönző adatbázis migrálva!");
        }
        else
        {
            setupLogger.LogInformation("  ✅ Szerszámkölcsönző adatbázis naprakész (0 migration)");
        }

        // ─────────────────────────────────────────────────────────────────
        // 4. ADATBÁZIS KAPCSOLAT TESZT
        // ─────────────────────────────────────────────────────────────────
        setupLogger.LogInformation("🔌 Adatbázis kapcsolat tesztelése...");
        var canConnectAuth = await authContext.Database.CanConnectAsync();
        var canConnectApp = await appContext.Database.CanConnectAsync();

        if (canConnectAuth && canConnectApp)
        {
            setupLogger.LogInformation("  ✅ Adatbázis kapcsolat sikeres!");
        }
        else
        {
            setupLogger.LogError("  ❌ Adatbázis kapcsolat sikertelen!");
            setupLogger.LogError("     Auth: {AuthStatus}", canConnectAuth ? "OK" : "FAIL");
            setupLogger.LogError("     App: {AppStatus}", canConnectApp ? "OK" : "FAIL");
        }

        // ─────────────────────────────────────────────────────────────────
        // 5. STATISZTIKÁK
        // ─────────────────────────────────────────────────────────────────
        try
        {
            var eszkozokCount = await appContext.Eszkozok.CountAsync();
            var foglalasokCount = await appContext.Foglalasok.CountAsync();
            setupLogger.LogInformation("📊 Jelenlegi adatok:");
            setupLogger.LogInformation("   Eszközök: {Count}", eszkozokCount);
            setupLogger.LogInformation("   Foglalások: {Count}", foglalasokCount);
        }
        catch (Exception ex)
        {
            setupLogger.LogWarning(ex, "Statisztikák lekérése sikertelen (normális első indításkor)");
        }
    }
    catch (Exception ex)
    {
        setupLogger.LogCritical(ex, "❌❌❌ KRITIKUS HIBA az automatikus setup során! ❌❌❌");
        setupLogger.LogError("Az alkalmazás nem tud elindulni.");
        throw;
    }
}

// ═══════════════════════════════════════════════════════════════════════════
// MIDDLEWARE PIPELINE
// ═══════════════════════════════════════════════════════════════════════════

logger.LogInformation("⚙️  Middleware pipeline konfigurálása...");

// ✅ STATIC FILES - LEGELSŐ!
app.UseStaticFiles();
logger.LogInformation("  ✅ Static files middleware aktív");

// SWAGGER - csak Development-ben
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Szerszámkölcsönző API v1");
        c.RoutePrefix = "swagger";
    });
    logger.LogInformation("  ✅ Swagger UI elérhető: /swagger");
}
else
{
    logger.LogInformation("  ℹ️  Swagger letiltva (Production mode)");
}

// HTTPS REDIRECT - csak Production-ben
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
    logger.LogInformation("  ✅ HTTPS redirect aktív");
}
else
{
    logger.LogInformation("  ℹ️  HTTPS redirect kikapcsolva (Development mode)");
}

app.UseCors("AllowAll");
logger.LogInformation("  ✅ CORS middleware aktív");

app.UseAuthentication();
logger.LogInformation("  ✅ Authentication middleware aktív");

app.UseAuthorization();
logger.LogInformation("  ✅ Authorization middleware aktív");

app.MapControllers();
logger.LogInformation("  ✅ Controllers mapped");

// ═══════════════════════════════════════════════════════════════════════════
// STARTUP BANNER
// ═══════════════════════════════════════════════════════════════════════════
logger.LogInformation("");
logger.LogInformation("╔════════════════════════════════════════════════════════════════╗");
logger.LogInformation("║                                                                ║");
logger.LogInformation("║          🔧 SZERSZÁMKÖLCSÖNZŐ API SIKERESEN ELINDULT 🔧       ║");
logger.LogInformation("║                                                                ║");
logger.LogInformation("╚════════════════════════════════════════════════════════════════╝");
logger.LogInformation("");
logger.LogInformation("📍 API endpoints: http://localhost:5000/api/*");
if (app.Environment.IsDevelopment())
{
    logger.LogInformation("📚 Swagger UI:    http://localhost:5000/swagger");
}
logger.LogInformation("📁 Static files:  http://localhost:5000/uploads/*");
logger.LogInformation("");
logger.LogInformation("▶️  Nyomd meg CTRL+C a leállításhoz");
logger.LogInformation("");

// ═══════════════════════════════════════════════════════════════════════════
// APP FUTTATÁSA
// ═══════════════════════════════════════════════════════════════════════════
try
{
    app.Run();
}
catch (Exception ex)
{
    logger.LogCritical(ex, "❌ Az alkalmazás váratlanul leállt!");
    throw;
}