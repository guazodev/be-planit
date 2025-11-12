using PlanIT.Domain.Interfaces;
using System.Threading.Tasks;

namespace PlanIT.Infraestructure.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly PlanITDbContext _context;

        // Inyectamos el DbContext
        public UnitOfWork(PlanITDbContext context)
        {
            _context = context;
        }

        // implementación del método
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return _context.SaveChangesAsync(cancellationToken);
        }
    }
}