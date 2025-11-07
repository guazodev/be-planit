using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlanIT.Domain;

namespace PlanIT.DataAccess.Interfaces
{
    // DTOS:
    public record ApiPlaceDetail(string Name, string PlaceId, double Latitude, double Longitude, float Rating);
    public record WeatherForecast(DateTime Date, string Description, float MaxTemp);

    public interface IApiIntegrationService
    {
        Task<IEnumerable<ApiPlaceDetail>> GetNearbyPointsOfInterestAsync(string destination, string travelStyle);
        Task<WeatherForecast> GetWeatherForecastAsync(string destination, DateTime startDate, int durationDays);
    }
}
