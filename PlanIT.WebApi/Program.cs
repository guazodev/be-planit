// ======================================================================
// USINGS
// ======================================================================

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

// BusinessLogic
using PlanIT.BusinessLogic.Interfaces;
using PlanIT.BusinessLogic.Services;
using PlanIT.BusinessLogic.DTOs;

// Domain
using PlanIT.Domain;
using PlanIT.Domain.Interfaces;

// DataAccess
using PlanIT.DataAccess.Interfaces;

// Infraestructure
using PlanIT.Infraestructure.Data;
using PlanIT.Infraestructure.Repositories;
using PlanIT.Infraestructure.Authentication;
using PlanIT.Infrastructure.Repositories;
using PlanIT.DataAccess.Repositories;

// ======================================================================
// 0. Builder
// ======================================================================

var builder = WebApplication.CreateBuilder(args);



// ======================================================================
// 1. Framework
// ======================================================================

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ======================================================================
// 2. DB — POSTGRESQL
// ======================================================================

var connectionString =
    builder.Configuration.GetConnectionString("postgres")
    ?? throw new InvalidOperationException("Connection string not found");

builder.Services.AddDbContext<PlanITDbContext>(options =>
    options.UseNpgsql(connectionString));   //  POSTGRES

// ======================================================================
// 3. Dependency Injection
// ======================================================================

builder.Services.AddScoped<ITravelRepository, TravelRepository>();
builder.Services.AddScoped<ITravelService, TravelService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ILugarService, LugarService>();

builder.Services.AddScoped<ILugarRepository, LugarRepository>();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddScoped<IJwtProvider, JwtProvider>();

// ======================================================================
// JWT
// ======================================================================

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
            )
        };
    });

builder.Services.AddAuthorization();

// ======================================================================
// Mocks
// ======================================================================

builder.Services.AddScoped<IApiIntegrationService, FakeApiIntegrationService>();
builder.Services.AddScoped<IIaAssistantService, FakeIaAssistantService>();
builder.Services.AddScoped<IEmailService, FakeEmailService>();

// ======================================================================
// Build
// ======================================================================


// Cargar appsettings.json
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);



var app = builder.Build();

// ======================================================================
// Middleware
// ======================================================================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

// ===========================================================================================================================
// 5. ENDPOINTS (Presentaci�n)
// ===========================================================================================================================
 
// ENDPOINT POST: CREAR Viaje
app.MapPost("/api/travels", async (
    TravelCreationDto dto,
    ITravelService travelService) =>
{
    try
    {
        // 1. Llamar al servicio
        var createdTravel = await travelService.CreateTravelAsync(dto);
 
        // 2. Devolver 201 Created con el objeto
        return Results.Created($"/api/travels/{createdTravel.Id}", createdTravel);
    }
    catch (ArgumentException ex)
    {
        // 3. Manejar errores de validación (400)
        return Results.BadRequest(new { message = ex.Message });
    }
    catch (Exception ex)
    {
        // 4. Manejar errores del servidor (500)
        return Results.Problem("Ocurrió un error inesperado: " + ex.Message);
    }
})
.WithName("CreateTravel")
.RequireAuthorization();
 
// ENDPOINT GET: LISTAR Viajes por Usuario
app.MapGet("/api/travels/user/{userId:guid}", async (
    Guid userId,
    ITravelService travelService) =>
{
    var travels = await travelService.GetTravelsByUserIdAsync(userId);
 
    return travels == null || !travels.Any()
        ? Results.NotFound(new { message = $"No se encontraron viajes para el usuario {userId}." })
        : Results.Ok(travels);
})
.WithName("GetUserTravels")
.RequireAuthorization();
 
// Reset de Password - nuevos endpoints
app.MapPost("/api/auth/forgot-password", async (
    ForgotPasswordDto dto,
    IUserService userService) =>
{
    await userService.RequestPasswordResetAsync(dto);
    return Results.Ok(new { message = "Si el email está registrado, se ha enviado una instrucción para resetear la contraseña." });
});
 
// Ejecución del reset de password
app.MapPost("/api/auth/reset-password", async (
    ResetPasswordDto dto,
    IUserService userService) =>
{
    try
    {
        await userService.ResetPasswordAsync(dto);
        return Results.Ok(new { message = "Contraseña reseteada exitosamente." });
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
    catch (Exception ex)
    {
        return Results.Problem("Ocurrió un error inesperado: " + ex.Message);
    }
});
 
 
 
// ENDPOINT POST: Registro Usuario
app.MapPost("/api/auth/register", async (
    UserRegisterDto dto,
    IUserService userService) =>
{
    try
    {
        await userService.RegisterAsync(dto);
        return Results.Ok(new { message = "Usuario registrado exitosamente." });
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
    catch (Exception ex)
    {
        return Results.Problem("Ocurrió un error inesperado: " + ex.Message);
    }
})
.WithName("RegisterUser");
 
// ENDPOINT POST: Login User
app.MapPost("/api/auth/login", async (
    UserLoginDto dto,
    IUserService userService) =>
{
    try
    {
        var token = await userService.LoginAsync(dto);
        return Results.Ok(new { token = token }); // Devuelve el token
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
    catch (Exception ex)
    {
        return Results.Problem("Ocurrió un error inesperado: " + ex.Message);
    }
})
.WithName("LoginUser");
 
app.MapControllers();
app.Run();

// Estos records son necesarios para que los Mocks compilen en WebAPI.
// despues hay que ELIMINARLOS y usar los reales de las capas correspondientes mientras no borren esto porfavor jajaja 
public record ApiPlaceDetail(string Name, string PlaceId, double Latitude, double Longitude, float Rating);
public record WeatherForecast(DateTime Date, string Description, float MaxTemp);
 
// =========================================================================================================================================================
// CLASES DE MOCK (NECESARIAS PARA QUE LA DI EN PROGRAM.CS COMPILE)
// =========================================================================================================================================================
public class FakeApiIntegrationService : IApiIntegrationService
{
    public Task<IEnumerable<ApiPlaceDetail>> GetNearbyPointsOfInterestAsync(string destination, string travelStyle) => Task.FromResult(Enumerable.Empty<ApiPlaceDetail>());
    public Task<WeatherForecast> GetWeatherForecastAsync(string destination, DateTime startDate, int durationDays) => Task.FromResult(new WeatherForecast(startDate, "Fake Weather", 25f));
 
    Task<IEnumerable<PlanIT.DataAccess.Interfaces.ApiPlaceDetail>> IApiIntegrationService.GetNearbyPointsOfInterestAsync(string destination, string travelStyle)
    {
        throw new NotImplementedException();
    }
 
    Task<PlanIT.DataAccess.Interfaces.WeatherForecast> IApiIntegrationService.GetWeatherForecastAsync(string destination, DateTime startDate, int durationDays)
    {
        throw new NotImplementedException();
    }
}
public class FakeIaAssistantService : IIaAssistantService
{
    public Task<string> GenerateItineraryJsonAsync(Travel travel, IEnumerable<ApiPlaceDetail> placeDetails) => Task.FromResult("{}");
    public Task<string> ChatWithAssistantAsync(string conversationHistoryJson, string newUserMessage) => Task.FromResult("Fake response from AI.");
 
    public Task<string> GenerateItineraryJsonAsync(Travel travel, IEnumerable<PlanIT.DataAccess.Interfaces.ApiPlaceDetail> placeDetails)
    {
        throw new NotImplementedException();
    }
}
 
public class FakeEmailService : IEmailService
{
    private readonly ILogger<FakeEmailService> _logger;
    public FakeEmailService(ILogger<FakeEmailService> logger)
    {
        _logger = logger;
    }
    public async Task SendPasswordResetEmailAsync(string toEmail, string resetToken)
    {
        // Simular el envío de correo electrónico
        _logger.LogInformation($"Simulando el envío de correo a {toEmail} con el token de reseteo: {resetToken}");
        await Task.CompletedTask;
    }
}