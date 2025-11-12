using PlanIT.BusinessLogic.DTOs;

namespace PlanIT.BusinessLogic.Interfaces
{
    public interface IUserService
    {
        Task RegisterAsync(UserRegisterDto dto);
        Task<string> LoginAsync(UserLoginDto dto); // Devuelve un string (el token JWT)
    }
}