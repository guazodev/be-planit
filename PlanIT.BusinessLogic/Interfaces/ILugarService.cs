using PlanIT.Domain.Entities;

namespace PlanIT.BusinessLogic.Interfaces;

public interface ILugarService
{
    Task<IEnumerable<Lugar>> ObtenerLugares();
}
