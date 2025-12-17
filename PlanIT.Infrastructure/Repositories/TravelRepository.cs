using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PlanIT.Domain;
using PlanIT.Domain.Interfaces; // Implementa la Interfaz (Contrato) ---> "ITravelRepository"
using PlanIT.Infrastructure.Data; // Depende del DbContext ---> "PlanITDbContext"

namespace PlanIT.Infrastructure.Repositories
{
    /// <summary>
    /// [Adaptador de Repositorio] 
    /// </summary>

    public class TravelRepository : ITravelRepository
    {
        private readonly PlanITDbContext _dbContext;
     

        public TravelRepository(PlanITDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // 1. Implementación de AddAsync
        // Erro que me daba: El nombre del metodo debe ser "AddAsync" para coincidir con el contrato.
        public async Task AddAsync(Travel travel)
        {
            await _dbContext.Travels.AddAsync(travel);
        }

        // 2. Implementacion de GetByIdAsync
        public async Task<Travel?> GetByIdAsync(Guid id)
        {
            return await _dbContext.Travels.FindAsync(id);
        }

        // 3. Implementacion de GetByUserIdAsync
        public async Task<IEnumerable<Travel>> GetByUserIdAsync(Guid userId)
        {
            return await _dbContext.Travels
                .Where(t => t.UserId == userId)
                .ToListAsync();
        }

        // 4. Implementacion de SaveChangesAsync
        public async Task<int> SaveChangesAsync()
        {
            return await _dbContext.SaveChangesAsync();
        }

        public void Update(Travel entity)
        {
            _dbContext.Travels.Update(entity);
            // Nota: El SaveChanges lo hace el UnitOfWork, no el repositorio.
        }
    }
}