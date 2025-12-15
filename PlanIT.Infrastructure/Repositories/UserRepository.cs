using Microsoft.EntityFrameworkCore;
using PlanIT.Domain;
using PlanIT.Domain.Interfaces;
using PlanIT.Infrastructure.Data;

namespace PlanIT.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly PlanITDbContext _context;

        public UserRepository(PlanITDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(User user)
        {
            await _context.Users.AddAsync(user);
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }
    }
}