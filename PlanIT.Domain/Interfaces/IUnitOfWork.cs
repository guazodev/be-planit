
using System.Threading;
using System.Threading.Tasks;

namespace PlanIT.Domain.Interfaces
{
    public interface IUnitOfWork
    {

        // Metodo que guarda los cambios en la base de datos
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}