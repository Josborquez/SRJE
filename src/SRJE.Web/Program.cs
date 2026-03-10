using Microsoft.EntityFrameworkCore;
using SRJE.Web.Infrastructure.Data;
using SRJE.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Oracle DbContext
builder.Services.AddDbContext<SrjeDbContext>(options =>
    options.UseOracle(builder.Configuration.GetConnectionString("OracleConnection")));

// Services (DI)
builder.Services.AddScoped<IBeneficiarioService, BeneficiarioService>();
builder.Services.AddScoped<IArchivoService, ArchivoService>();

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

if (app.Environment.IsDevelopment())
{
    app.UseCors("VueDev");
}

app.UseStaticFiles();
app.MapControllers();

// Fallback: sirve index.html para Vue Router (SPA)
app.MapFallbackToFile("index.html");

app.Run();
