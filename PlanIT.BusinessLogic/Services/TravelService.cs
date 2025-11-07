using PlanIT.BusinessLogic.Interfaces;
using PlanIT.DataAccess.Interfaces;
using PlanIT.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanIT.BusinessLogic.Services
{
    public class TravelService : ITravelService
    {
        private readonly ITravelRepository _travelRepository;
        public TravelService(ITravelRepository travelRepository)
        {
            _travelRepository = travelRepository;
        }

        public async Task<Travel> CreateTravelAsync(Travel travel)
        {

            //Validaciones de negocio: Validar datos obligatorios antes de guardar
            if (string.IsNullOrEmpty(travel.Destination) || travel.UserId == Guid.Empty)
                throw new ArgumentException("El Destino y el ID de usuario son Obligatorios.");
            
            // Se llama al repositorio con el objeto travel que ya tiene el Guid
            await _travelRepository.AddAsync(travel);
            await _travelRepository.SaveChangesAsync();


            return travel;
     
        }

        public async Task<IEnumerable<Travel>> GetTravelsByUserIdAsync(Guid userId)
        {
            return await _travelRepository.GetByUserIdAsync(userId);
        }

        public Task<object?> GetUserTravelsAsync(Guid userId)
        {
            throw new NotImplementedException();
        }
    }
}
