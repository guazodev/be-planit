using PlanIT.BusinessLogic.Interfaces;
using PlanIT.DataAccess.Interfaces;
using PlanIT.Domain.Entities;

namespace PlanIT.BusinessLogic.Services;

public class LugarService : ILugarService
{
    private readonly ILugarRepository _repo;

    public LugarService(ILugarRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<Lugar>> ObtenerLugares()
    {
        // Por ahora sin lógica, solo devuelve
        return await _repo.ObtenerTodos();
    }
}
