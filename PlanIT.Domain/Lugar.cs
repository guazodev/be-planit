namespace PlanIT.Domain.Entities;

public class Lugar
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Provincia { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int Presupuesto_Estimado { get; set; }
    public int Duracion_Dias { get; set; }
}
