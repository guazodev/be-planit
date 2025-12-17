using PlanIT.Domain;
using System;
using System.Threading.Tasks;

namespace PlanIT.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetUserByEmailAsync(string email);
        Task AddAsync(User user);
        Task<User?> GetByEmailAsync(string email); // Obtiene usuario por email. Funcion para Google Oauth
    }
}