using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using PlanIT.DataAccess.Interfaces;
using PlanIT.BusinessLogic.Interfaces;

namespace PlanIT.Infrastructure.Integrations
{
    public class GooglePlacesService : IApiIntegrationService
    {
        private readonly string _apiKey;
        private readonly HttpClient _httpClient;

        public GooglePlacesService(IConfiguration configuration)
        {
            _apiKey = configuration["ApiKeys:GoogleMaps"]
                      ?? throw new Exception("Google Maps API Key no encontrada en appsettings.");
            _httpClient = new HttpClient();
        }

        public async Task<IEnumerable<ApiPlaceDetail>> GetNearbyPointsOfInterestAsync(string destination, string travelStyle)
        {
            // 1. Armamos la busqueda. Ej: "Lugares de Anime en Tokio, Aguante ONE PIECE"
            var query = $"{travelStyle} in {destination}";

            // 2. Usamos la API de TextSearch (Busqueda de texto) de Google
            var url = $"https://maps.googleapis.com/maps/api/place/textsearch/json?query={query}&key={_apiKey}";

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var errorGoogle = await response.Content.ReadAsStringAsync();
                throw new Exception($"ERROR DE GOOGLE ({response.StatusCode}): {errorGoogle}");
            }

            var jsonString = await response.Content.ReadAsStringAsync();

            // 3. Deserializamos la respuesta de Google
            // Usamos JsonDocument para navegar rápido sin crear mil clases
            using JsonDocument doc = JsonDocument.Parse(jsonString);
            var root = doc.RootElement;

            var results = new List<ApiPlaceDetail>();

            if (root.TryGetProperty("results", out JsonElement placesArray))
            {
                // Tomamos solo los primeros 5 o 10 para no saturar
                foreach (var place in placesArray.EnumerateArray().Take(5))
                {
                    var name = place.GetProperty("name").GetString() ?? "Sin nombre";
                    var placeId = place.GetProperty("place_id").GetString() ?? "";
                    var rating = 0f;

                    //PRUEBITA TRAMPA de Consola jajajajaja 
                    Console.WriteLine($"[GOOGLE MAPS] Encontré: {name}");

                    if (place.TryGetProperty("rating", out JsonElement ratingElement))
                    {
                        rating = (float)ratingElement.GetDouble();
                    }

                    // Coordenadas
                    var location = place.GetProperty("geometry").GetProperty("location");
                    var lat = location.GetProperty("lat").GetDouble();
                    var lng = location.GetProperty("lng").GetDouble();

                    results.Add(new ApiPlaceDetail(name, placeId, lat, lng, rating));
                }
            }

            return results;
        }

        // Este metodo lo dejamos "boludo" por ahora
        public Task<WeatherForecast> GetWeatherForecastAsync(string destination, DateTime startDate, int durationDays)
        {
            return Task.FromResult(new WeatherForecast(startDate, "Soleado (Simulado)", 25f));
        }
    }
}
