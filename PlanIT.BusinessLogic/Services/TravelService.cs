using PlanIT.BusinessLogic.Interfaces; // Para ITravelService
using PlanIT.Domain.Interfaces;     // Para IUnitOfWork y ITravelRepository
using PlanIT.Domain;                // Para la entidad Travel
using PlanIT.BusinessLogic.DTOs;      
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PlanIT.DataAccess.Interfaces;


namespace PlanIT.BusinessLogic.Services
{
    public class TravelService : ITravelService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITravelRepository _travelRepository;

        //las APIs
        private readonly IApiIntegrationService _googleService;
        private readonly IIaAssistantService _aiService;


       public TravelService(
            ITravelRepository travelRepository, 
            IUnitOfWork unitOfWork,
            IApiIntegrationService googleService, // Google Places
            IIaAssistantService aiService)      // OpenAI
        {
            _travelRepository = travelRepository;
            _unitOfWork = unitOfWork;
            _googleService = googleService;
            _aiService = aiService;
        }

        public async Task<Travel> CreateTravelAsync(TravelCreationDto dto)
        {
            var travel = new Travel
            {
                Id = Guid.NewGuid(),
                UserId = dto.UserId,
                Destination = dto.Destination,
                DurationDays = dto.DurationDays,
                EstimatedBudget = dto.EstimatedBudget,
                TravelStyle = dto.TravelStyle,
                // Nota: Los campos ItineraryJson e IsGenerated seran nulos/false por defecto
            };
            
            await _travelRepository.AddAsync(travel);
            await _unitOfWork.SaveChangesAsync();

            return travel;
        }

        public async Task<IEnumerable<Travel>> GetTravelsByUserIdAsync(Guid userId)
        {
            return await _travelRepository.GetByUserIdAsync(userId);
        }

        // =========================================================
        // GENERAR ITINERARIO CON IA
        // =========================================================
        public async Task<Travel> GenerateItineraryAsync(Guid travelId)
        {
            // A. Buscar el viaje en la base de datos
            var travel = await _travelRepository.GetByIdAsync(travelId);
            if (travel == null) 
                throw new KeyNotFoundException($"Viaje con ID {travelId} no encontrado.");
            
            // B. ORQUESTACIÓN: De la Base de Datos a la API de Google
            // Usamos el destino y estilo del viaje para encontrar puntos de interés reales.
            var placesOfInterest = await _googleService.GetNearbyPointsOfInterestAsync(
                travel.Destination, 
                travel.TravelStyle);
            
            // C. ORQUESTACIÓN: De Google a la IA de OpenAI
            // Pedimos a la IA que use esa información para crear un plan en formato JSON.
            var itineraryJson = await _aiService.GenerateItineraryJsonAsync(travel, placesOfInterest);

            // D. Guardar el resultado en PostgreSQL (columna JSONB)
            travel.ItineraryJson = itineraryJson;
            travel.IsGenerated = true;

            _travelRepository.Update(travel);
            await _unitOfWork.SaveChangesAsync();

            return travel;
        }
    }
}