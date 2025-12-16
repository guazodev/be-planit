// todos los using necesarios
using Microsoft.EntityFrameworkCore;
using PlanIT.BusinessLogic.Interfaces;
using PlanIT.BusinessLogic.Services;
using PlanIT.DataAccess.Interfaces;
using PlanIT.Domain;
using PlanIT.Domain.Interfaces;
using PlanIT.Infrastructure.Data;
using PlanIT.BusinessLogic.DTOs;
using PlanIT.Infrastructure.Repositories; // Aca está la clase de UserRepository, casi me mareo
// Importaciones para Token
using PlanIT.Infrastructure.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
//NUEVOS DE IA 
using PlanIT.Infrastructure.Integrations;
using Microsoft.Extensions.AI;
using OpenAI;

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

var connectionString = builder.Configuration.GetConnectionString("PlanITDbConnection")
    ?? throw new InvalidOperationException("Connection string not found.");

// CAMBIO ACA: UseNpgsql
builder.Services.AddDbContext<PlanITDbContext>(options =>
    options.UseNpgsql(connectionString));

// ===========================================================================================================================
// 3. Registros de DEPENDENCIAS (Inyeccion de Control)
// Contrato entre capas: Definición de Contratos (Domain) -----> Implementación (Infrastructure / BusinessLogic). Domain define las interfaces (IUserRepository, IUnitOfWork).
// ===========================================================================================================================

builder.Services.AddScoped<ITravelRepository, TravelRepository>();
builder.Services.AddScoped<ITravelService, TravelService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddScoped<IJwtProvider, JwtProvider>();

// Configuracion de autenticacion
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

builder.Services.AddAuthorization(); // Necesario para .RequireAuthorization()

// ===========================================================================================================================
// INTEGRACIONES REALES (Google & OpenAI) 
// ===========================================================================================================================

var keyParaOpenAi = builder.Configuration["ApiKeys:OpenAI"]
    ?? throw new InvalidOperationException("OpenAI Key no encontrada.");

// 1. Instanciamos el cliente general
OpenAI.OpenAIClient openAiClient = new(keyParaOpenAi);

// 2. Obtenemos el cliente de Chat
var openAiChatClient = openAiClient.GetChatClient("gpt-4o-mini");

// 3. Conversión (CORREGIDO: Es .AsChatClient, sin la 'I')
// Al usar la versión 2.1.0-beta.2, este método aparece mágicamente.
IChatClient chatClient = Microsoft.Extensions.AI.OpenAIClientExtensions.AsIChatClient(openAiChatClient);

builder.Services.AddChatClient(chatClient);
builder.Services.AddScoped<IIaAssistantService, OpenAiAssistantService>();

// Mantenemos el FakeEmailService por ahora (LUCHO ESTO TENES QUE HACER VOS)
builder.Services.AddScoped<IEmailService, FakeEmailService>();

builder.Services.AddScoped<IApiIntegrationService, FakeApiIntegrationService>();

builder.Services.AddScoped<ITravelService, TravelService>();





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

// Orden de token
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

// ENDPOINT POST: Generar Itinerario con IA 

app.MapPost("/api/travels/{id:guid}/generate", async (
    Guid id,
    ITravelRepository travelRepo, // 1. Necesitamos buscar el viaje en la BD
    IChatClient chatClient) =>    // 2. Necesitamos a la IA
{
    try
    {
        // PASO A: Buscamos el viaje para saber destino, días, presupuesto...
        var travel = await travelRepo.GetByIdAsync(id);

        if (travel == null)
            return Results.NotFound(new { message = "No encontré ese viaje en la base de datos." });

        // PASO B: Creamos el "Prompt" (la orden para la IA)
        var prompt = $@"Actúa como un guía de viajes experto.
                        Crea un itinerario día por día para un viaje a {travel.Destination}.
                        Duración: {travel.DurationDays} días.
                        Presupuesto: {travel.EstimatedBudget} USD.
                        Estilo de viaje: {travel.TravelStyle}.
                        Dame solo el itinerario sin introducciones.";

        // PASO C: Enviamos el mensaje a OpenAI
        var mensajes = new List<ChatMessage>
        {
            new(ChatRole.User, prompt)
        };

        // Usamos .ToString() porque vimos en tu prueba que funciona bien en esta versión
        var respuesta = await chatClient.GetResponseAsync(mensajes);
        var textoItinerario = respuesta.ToString();

        // PASO D: Devolvemos el resultado al usuario (y a Postman)
        return Results.Ok(new
        {
            TravelId = id,
            Destination = travel.Destination,
            GeneratedItinerary = textoItinerario
        });
    }
    catch (Exception ex)
    {
        return Results.Problem("La IA falló: " + ex.Message);
    }
})
.WithName("GenerateItinerary")
.RequireAuthorization();



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

// Test rapido para ver si funca (FUNCO) lo dejo por las dudas 
//app.MapGet("/api/test-ai", async (IChatClient chatClient) =>
//{
//    var mensajes = new List<ChatMessage>
//    {
//        new(ChatRole.User, "Di algo sobre Boca Juniors")
//    };


//    var respuesta = await chatClient.GetResponseAsync(mensajes);

 
//    return Results.Ok(respuesta.ToString());
//});

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