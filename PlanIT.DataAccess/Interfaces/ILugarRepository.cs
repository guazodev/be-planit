using PlanIT.Domain.Entities;

namespace PlanIT.DataAccess.Interfaces;

public interface ILugarRepository
{
    Task<IEnumerable<Lugar>> ObtenerTodos();
}
