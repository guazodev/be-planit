// Crear contrato donde definimos el generador de tokens
using PlanIT.Domain;

namespace PlanIT.BusinessLogic.Interfaces
{
    public interface IJwtProvider
    {
        string GenerateToken(User user);
    }
}