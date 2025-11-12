using PlanIT.BusinessLogic.Interfaces; // Para ITravelService
using PlanIT.Domain.Interfaces;     // Para IUnitOfWork y ITravelRepository
using PlanIT.Domain;                // Para la entidad Travel
using PlanIT.BusinessLogic.DTOs;      
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PlanIT.BusinessLogic.Services
{
    public class TravelService : ITravelService
    {
        private readonly ITravelRepository _travelRepository;
        private readonly IUnitOfWork _unitOfWork;
        
        public TravelService(ITravelRepository travelRepository, IUnitOfWork unitOfWork)
        {
            _travelRepository = travelRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Travel> CreateTravelAsync(TravelCreationDto dto)
        {
            // Validaciones (usando el DTO)
            if (string.IsNullOrEmpty(dto.Destination) || dto.UserId == Guid.Empty)
                throw new ArgumentException("El Destino y el ID de usuario son Obligatorios.");

            // Crear la entiendad
            var travel = new Travel
            {
                Id = Guid.NewGuid(),
                UserId = dto.UserId,
                Destination = dto.Destination,
                DurationDays = dto.DurationDays,
                EstimatedBudget = dto.EstimatedBudget,
                TravelStyle = dto.TravelStyle,
            };
            
            await _travelRepository.AddAsync(travel);
            await _unitOfWork.SaveChangesAsync();

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