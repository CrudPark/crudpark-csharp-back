using Microsoft.EntityFrameworkCore;
using crud_park_back.Models;
using crud_park_back.Services;
using crud_park_back.Mappings;
using FluentValidation;
using FluentValidation.AspNetCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "CrudPark API", Version = "v1" });
});

// Database
builder.Services.AddDbContext<ParkingDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"), npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure();
    });
});

// Configuración de Npgsql para TIMESTAMP sin timezone
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Services
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IParkingService, ParkingService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IReporteService, ReporteService>();

// CORS - Configuración mejorada
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() 
    ?? new[] { "http://localhost:3000", "http://localhost:5173" }; // Defaults para desarrollo

builder.Services.AddCors(options =>
{
    options.AddPolicy("CrudParkPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
    
    // Mantener AllowAll solo para desarrollo
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Obtener logger para configuración inicial
var logger = app.Services.GetRequiredService<ILogger<Program>>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Usar política de CORS según el entorno
if (app.Environment.IsDevelopment())
{
    app.UseCors("AllowAll");
    logger.LogWarning("⚠️ Usando política CORS 'AllowAll' - Solo para desarrollo");
}
else
{
    app.UseCors("CrudParkPolicy");
    logger.LogInformation("✅ Usando política CORS 'CrudParkPolicy' - Producción");
}

app.UseAuthorization();
app.MapControllers();

// Ensure database is created
// using (var scope = app.Services.CreateScope())
// {
//     var context = scope.ServiceProvider.GetRequiredService<ParkingDbContext>();
//     context.Database.EnsureCreated();
// }

// Add health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

app.Run();
