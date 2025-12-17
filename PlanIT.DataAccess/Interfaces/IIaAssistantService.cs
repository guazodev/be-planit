using PlanIT.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanIT.DataAccess.Interfaces
{
    public interface IIaAssistantService
    {
        Task<string> GenerateItineraryJsonAsync(Travel travel, IEnumerable<ApiPlaceDetail> placeDetails);
        Task<string> ChatWithAssistantAsync(string conversationHistoryJson, string newUserMessage) => throw new NotImplementedException();
    }
}
