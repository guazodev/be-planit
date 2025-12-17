using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlanIT.Domain;



namespace PlanIT.Domain.Interfaces;

// Contrato de Puerto de DB, Contrato que la Infraestructura debe implementar.
public interface ITravelRepository
{
    Task<Travel?> GetByIdAsync(Guid id);
    Task<IEnumerable<Travel>> GetByUserIdAsync(Guid userId);

    // ERROR Que tuve: El metodo debe terminar en Async si devuelve Task.
    Task AddAsync(Travel travel);

    Task<int> SaveChangesAsync();
    void Update(Travel travel);
}
