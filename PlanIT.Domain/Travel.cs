using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanIT.Domain
{
    public class Travel
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }

        public string Destination { get; set; } = string.Empty;
        public int DurationDays { get; set; }
        public decimal EstimatedBudget { get; set; }
        public string TravelStyle { get; set; } = "Low Cost";

        
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime EndDate { get; set; } = DateTime.UtcNow;
       

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public string? ItineraryJson { get; set; }
        public bool IsGenerated { get; set; }

        public Travel()
        {
            if (Id == Guid.Empty)
            {
                Id = Guid.NewGuid();
            }
        }
    }
}
