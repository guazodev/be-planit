namespace PlanIT.BusinessLogic.DTOs
{
    public class TravelCreationDto
    {
        public Guid UserId { get; set; }
        public string Destination { get; set; } = string.Empty;
        public int DurationDays { get; set; }   
        public decimal EstimatedBudget { get; set; }
        public string TravelStyle { get; set; } = "Low Cost";

    }
}
