using Microsoft.EntityFrameworkCore;
using Serilog;
using SRJE.Web.Infrastructure.Data;
using SRJE.Web.Middleware;
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

// Oracle DbContext
builder.Services.AddDbContext<SrjeDbContext>(options =>
    options.UseOracle(builder.Configuration.GetConnectionString("OracleConnection")));

// Services (DI)
builder.Services.AddScoped<IBeneficiarioService, BeneficiarioService>();
builder.Services.AddScoped<IRemuneracionesService, RemuneracionesService>();
builder.Services.AddScoped<ITemgeService, TemgeService>();
builder.Services.AddScoped<INuevasCuentasService, NuevasCuentasService>();

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
              .AllowAnyHeader());
});

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseCors("VueDev");
}

app.UseStaticFiles();
app.MapControllers();

// Fallback: sirve index.html para Vue Router (SPA)
app.MapFallbackToFile("index.html");

app.Run();
