using System.Net.Http.Json;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Configuration;
using PlanIT.DataAccess.Interfaces;
using PlanIT.Domain;

namespace PlanIT.Infrastructure.Integrations;

public class GoogleApiIntegrationService : IApiIntegrationService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public GoogleApiIntegrationService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        // Usamos el cliente HTTP nombrado (ver Program.cs)
        _httpClient = httpClientFactory.CreateClient("GoogleApi");
        _apiKey = configuration["ApiKeys:GooglePlaces"] ?? throw new Exception("Falta API Key Google");
        HttpClientFactory = httpClientFactory;
    }

    public IHttpClientFactory HttpClientFactory { get; }

    public async Task<IEnumerable<ApiPlaceDetail>> GetNearbyPointsOfInterestAsync(string destination, string travelStyle)
    {
        var query = $"{travelStyle} points of interest in {destination}";
        var url = $"https://maps.googleapis.com/maps/api/place/textsearch/json?query={query}&key={_apiKey}";

        return new List<ApiPlaceDetail>
        {
            new ("Lugar Dummy 1", "P1", 40.71, -74.00, 4.5f),
            new ("Lugar Dummy 2", "P2", 40.72, -74.01, 4.0f)
        };
    }

    public Task<WeatherForecast> GetWeatherForecastAsync(string destination, DateTime startDate, int durationDays)
    {
        return Task.FromResult(new WeatherForecast(startDate, "Soleado", 25f));
    }
}