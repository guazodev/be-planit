using PlanIT.Domain;
using PlanIT.BusinessLogic.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanIT.BusinessLogic.Interfaces
{
    public interface ITravelService
    {
        Task<Travel> CreateTravelAsync(TravelCreationDto travel);
        Task<IEnumerable<Travel>> GetTravelsByUserIdAsync(Guid userId);


        //Nuevo: 
        Task<Travel> GenerateItineraryAsync(Guid travelId);
    }
}
