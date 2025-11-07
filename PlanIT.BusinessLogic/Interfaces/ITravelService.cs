using PlanIT.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanIT.BusinessLogic.Interfaces
{
    public interface ITravelService
    {
        Task<Travel> CreateTravelAsync(Travel travel);
        Task<IEnumerable<Travel>> GetTravelsByUserIdAsync(Guid userId);

    }
}
