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
// PROGRAM.CS
// ═══════════════════════════════════════════════════════════════════════════

var builder = WebApplication.CreateBuilder(args);

// ═══════════════════════════════════════════════════════════════════════════
// LOGGING
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
// MODULOK
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
// BACKGROUND SERVICE
// ═══════════════════════════════════════════════════════════════════════════
builder.Services.AddHostedService<FoglalasCleanupService>();
logger.LogInformation("⏰ FoglalasCleanupService regisztrálva (15 perc auto-törlés)");

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
logger.LogInformation("🏗️  Application built");

// ═══════════════════════════════════════════════════════════════════════════
// ADATBÁZIS SETUP
// ═══════════════════════════════════════════════════════════════════════════
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var setupLogger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        // UPLOADS MAPPA
        var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        if (!Directory.Exists(uploadsPath))
        {
            Directory.CreateDirectory(uploadsPath);
            setupLogger.LogInformation("📁 'wwwroot/uploads' mappa létrehozva");
        }

        // ╔═══════════════════════════════════════════════════════════════╗
        // ║  🔧 FEJLESZTŐI KÖRNYEZET - ADATBÁZIS ÚJRALÉTREHOZÁS         ║
        // ║                                                               ║
        // ║  Ez a blokk CSAK Debug buildben és Development környezetben   ║
        // ║  fut le. Release/Production buildben a fordító KIHAGYJA.      ║
        // ║                                                               ║
        // ║  Használat (sémaváltás, enum módosítás után):                 ║
        // ║    Windows:  set RECREATE_DB=true && dotnet run               ║
        // ║    Linux:    RECREATE_DB=true dotnet run                      ║
        // ║                                                               ║
        // ║  PUBLIKÁLÁSHOZ: `dotnet publish -c Release` → ez a blokk     ║
        // ║  automatikusan KIMARAD a lefordított kódból.                  ║
        // ╚═══════════════════════════════════════════════════════════════╝
#if DEBUG
        if (app.Environment.IsDevelopment())
        {
            var forceRecreate = Environment.GetEnvironmentVariable("RECREATE_DB") == "true";
            if (forceRecreate)
            {
                setupLogger.LogWarning("╔══════════════════════════════════════════════════╗");
                setupLogger.LogWarning("║  ⚠️  RECREATE_DB=true AKTÍV                      ║");
                setupLogger.LogWarning("║  Mindkét adatbázis TÖRLÉSE és ÚJRALÉTREHOZÁSA!   ║");
                setupLogger.LogWarning("╚══════════════════════════════════════════════════╝");

                var authCtxDev = services.GetRequiredService<SzerszamKolcsonzo.Features.Auth.Data.AuthDbContext>();
                await authCtxDev.Database.EnsureDeletedAsync();
                setupLogger.LogWarning("  🗑️  Auth adatbázis törölve");

                var appCtxDev = services.GetRequiredService<AppDbContext>();
                await appCtxDev.Database.EnsureDeletedAsync();
                setupLogger.LogWarning("  🗑️  App adatbázis törölve");
            }
        }
#endif
        // ╔═══════════════════════════════════════════════════════════════╗
        // ║  🔧 FEJLESZTŐI BLOKK VÉGE                                    ║
        // ╚═══════════════════════════════════════════════════════════════╝

        // ─────────────────────────────────────────────────────────────────
        // ADATBÁZIS LÉTREHOZÁS (Production + Development egyaránt)
        // Ha az adatbázis nem létezik → létrehozza a sémát + seed adatok
        // Ha már létezik → nem nyúl hozzá
        // ─────────────────────────────────────────────────────────────────

        // AUTH ADATBÁZIS
        setupLogger.LogInformation("🗄️  Auth adatbázis...");
        var authContext = services.GetRequiredService<SzerszamKolcsonzo.Features.Auth.Data.AuthDbContext>();
        var authCreated = await authContext.Database.EnsureCreatedAsync();
        setupLogger.LogInformation(authCreated
            ? "  ✅ Auth adatbázis LÉTREHOZVA (seed adatokkal)"
            : "  ✅ Auth adatbázis már létezik");

        // APP ADATBÁZIS
        setupLogger.LogInformation("🗄️  Szerszámkölcsönző adatbázis...");
        var appContext = services.GetRequiredService<AppDbContext>();
        var appCreated = await appContext.Database.EnsureCreatedAsync();
        setupLogger.LogInformation(appCreated
            ? "  ✅ App adatbázis LÉTREHOZVA (seed adatokkal)"
            : "  ✅ App adatbázis már létezik");

        // KAPCSOLAT TESZT
        var canConnectAuth = await authContext.Database.CanConnectAsync();
        var canConnectApp = await appContext.Database.CanConnectAsync();
        setupLogger.LogInformation("🔌 DB kapcsolat: Auth={Auth}, App={App}",
            canConnectAuth ? "OK" : "FAIL",
            canConnectApp ? "OK" : "FAIL");

        // STATISZTIKÁK
        try
        {
            var eszkozokCount = await appContext.Eszkozok.CountAsync();
            var foglalasokCount = await appContext.Foglalasok.CountAsync();
            setupLogger.LogInformation("📊 Eszközök: {E}, Foglalások: {F}", eszkozokCount, foglalasokCount);
        }
        catch (Exception ex)
        {
            setupLogger.LogWarning(ex, "Statisztikák lekérése sikertelen (normális első indításkor)");
        }
    }
    catch (Exception ex)
    {
        setupLogger.LogCritical(ex, "❌ KRITIKUS HIBA az adatbázis setup során!");
        throw;
    }
}

// ═══════════════════════════════════════════════════════════════════════════
// MIDDLEWARE PIPELINE
// ═══════════════════════════════════════════════════════════════════════════
app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Szerszámkölcsönző API v1");
        c.RoutePrefix = "swagger";
    });
    logger.LogInformation("📚 Swagger: /swagger");
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// ═══════════════════════════════════════════════════════════════════════════
// START
// ═══════════════════════════════════════════════════════════════════════════
logger.LogInformation("");
logger.LogInformation("╔════════════════════════════════════════════════════════════════╗");
logger.LogInformation("║       🔧 SZERSZÁMKÖLCSÖNZŐ API SIKERESEN ELINDULT 🔧          ║");
logger.LogInformation("╚════════════════════════════════════════════════════════════════╝");
logger.LogInformation("");
logger.LogInformation("📍 API: http://localhost:5000/api/*");
if (app.Environment.IsDevelopment())
{
    logger.LogInformation("📚 Swagger: http://localhost:5000/swagger");
}
logger.LogInformation("📁 Uploads: http://localhost:5000/uploads/*");
logger.LogInformation("");

try
{
    app.Run();
}
catch (Exception ex)
{
    logger.LogCritical(ex, "❌ Az alkalmazás váratlanul leállt!");
    throw;
}