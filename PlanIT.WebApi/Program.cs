// todos los using necesarios
using Microsoft.EntityFrameworkCore;
using PlanIT.BusinessLogic.Interfaces;
using PlanIT.BusinessLogic.Services;
using PlanIT.DataAccess.Interfaces;
using PlanIT.Domain;
using PlanIT.Domain.Interfaces;
using PlanIT.Infraestructure.Data;
using PlanIT.Infraestructure.Repositories;
using PlanIT.BusinessLogic.DTOs;




// ========================================================================================================================
// 0. Builder, aca inicia la aplicacion 
// ==========================================================================================================================
var builder = WebApplication.CreateBuilder(args);



// ===========================================================================================================================
// 1. FrameWork & Mechanisms Setup (Capa Externa)
// ===========================================================================================================================

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
    });
});

// ===========================================================================================================================
// 2. Infraestruture Setup (Capa de Infraestructura)
// ===========================================================================================================================

var connectionString = builder.Configuration.GetConnectionString("PlanITDbConnection") ?? throw new InvalidOperationException("Connection string 'PlanITDbConnection' not found.");
builder.Services.AddDbContext<PlanITDbContext>(options =>
    options.UseSqlServer(connectionString));

// ===========================================================================================================================
// 3. Registros de DEPENDENCIAS (Inyeccion de Control)
// Contrato entre capas: BusinessLogic <-> DataAccess -----> Implementacion (Infraestructure/BusinessLogic)
// ===========================================================================================================================

builder.Services.AddScoped<ITravelRepository, TravelRepository>();
builder.Services.AddScoped<ITravelService, TravelService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// ===========================================================================================================================
// Integraciones, FEIKS Jei, mientras tanto despues cambiamos 

builder.Services.AddScoped<IApiIntegrationService, FakeApiIntegrationService>();
builder.Services.AddScoped<IIaAssistantService, FakeIaAssistantService>();



// ===========================================================================================================================
// Para construir la app:

var app = builder.Build();



// ===========================================================================================================================
// 4. MIDDLEWARE
// ===========================================================================================================================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

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
.WithName("CreateTravel");

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
.WithName("GetUserTravels");

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