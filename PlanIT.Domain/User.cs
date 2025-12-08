using System;
using System.Collections.Generic;

namespace PlanIT.Domain
{
    public class User
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;

        // Guardá el HASH de la contraseña.
        public string PasswordHash { get; set; } = string.Empty;

        // Guardar y expiracion de token de forgot password
        public string? PasswordResetToken { get; set; } = string.Empty;
        public DateTime? PasswordResetExpires { get; set; } = DateTime.MinValue;

        // Relación (opcional pero recomendada):
        // Un usuario puede tener muchos viajes
        public virtual ICollection<Travel> Travels { get; set; } = new List<Travel>();
    }
}