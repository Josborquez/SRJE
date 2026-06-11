using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Serilog;
using SRJE.Web.Infrastructure.Data;
using SRJE.Web.Middleware;
using SRJE.Web.Models;
using SRJE.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/srje-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}"));

// Configuracion tipada (elimina magic numbers)
builder.Services.Configure<SrjeSettings>(
    builder.Configuration.GetSection(SrjeSettings.SectionName));

// Oracle DbContext — la connection string viene de appsettings.Development.json (dev)
// o de la variable de entorno ConnectionStrings__OracleConnection (produccion). Nunca en appsettings.json.
var connectionString = builder.Configuration.GetConnectionString("OracleConnection");
if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException(
        "Connection string 'OracleConnection' no configurada. " +
        "Definir variable de entorno ConnectionStrings__OracleConnection.");

builder.Services.AddDbContext<SrjeDbContext>(options =>
    options.UseOracle(connectionString));

// Autenticacion por cookies (pluggable: cambiar DevAuthService por otra implementacion en produccion)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "SRJE.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        // Para API: devolver 401 en vez de redirect a login
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
    });
builder.Services.AddAuthorization();

// Auth service segun Auth:Provider — "Database" (USUARIOS_SISTEMA + BCrypt) o "Development" (solo dev)
var authProvider = builder.Configuration["Auth:Provider"] ?? "Development";
if (authProvider.Equals("Database", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddScoped<IAuthService, DbAuthService>();
}
else
{
    if (!builder.Environment.IsDevelopment())
        throw new InvalidOperationException(
            $"Auth:Provider '{authProvider}' no permitido fuera de Development. Usar 'Database'.");
    builder.Services.AddScoped<IAuthService, DevAuthService>();
}

// Services (DI)
builder.Services.AddScoped<IBeneficiarioService, BeneficiarioService>();
builder.Services.AddScoped<IRemuneracionesService, RemuneracionesService>();
builder.Services.AddScoped<ITemgeService, TemgeService>();
builder.Services.AddScoped<INuevasCuentasService, NuevasCuentasService>();
builder.Services.AddScoped<IAuditoriaBeneficiariosService, AuditoriaBeneficiariosService>();
builder.Services.AddScoped<IFuncionarioService, FuncionarioService>();

// MVC + JSON
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
        options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
    });

// CORS para Vue.js dev server
builder.Services.AddCors(options =>
{
    options.AddPolicy("VueDev", policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials());
});

var app = builder.Build();

// Bootstrap usuario admin inicial (solo provider Database, solo si no existe).
// El password se entrega por variable de entorno SRJE_ADMIN_PASSWORD; el hash nunca va en el repo.
var adminPassword = Environment.GetEnvironmentVariable("SRJE_ADMIN_PASSWORD");
if (authProvider.Equals("Database", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(adminPassword))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<SrjeDbContext>();
    if (!await db.UsuariosSistema.AnyAsync(u => u.Usuario == "admin"))
    {
        db.UsuariosSistema.Add(new SRJE.Web.Models.Entities.UsuarioSistema
        {
            Usuario = "admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword),
            NombreCompleto = "Administrador SRJE",
            Rol = "admin",
            Estado = "A"
        });
        await db.SaveChangesAsync();
        Log.Information("Usuario admin inicial creado via SRJE_ADMIN_PASSWORD");
    }
}

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseCors("VueDev");
}

app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Fallback: sirve index.html para Vue Router (SPA)
app.MapFallbackToFile("index.html");

app.Run();
