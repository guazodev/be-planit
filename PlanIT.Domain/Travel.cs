using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanIT.Domain
{
    public class Travel
    {
        // Id Principal de la entidad Travel
        public Guid Id { get; set; }

        // Id del Usuario que crea el viaje usando Guid 
        public Guid UserId { get; set; }

        public string Destination { get; set; } = string.Empty;
        public int DurationDays { get; set; }
        public decimal EstimatedBudget { get; set; }
        public string TravelStyle { get; set; } = "Low Cost";
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public string? ItineraryJson { get; set; }
        public bool IsGenerated { get; set; }

        public Travel()
        {
            // Esto asegura que cada objeto tenga un Id unico al ser creado.
            if (Id == Guid.Empty)
            {
                Id = Guid.NewGuid();
            }   
        }

    }
}
