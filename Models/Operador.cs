using System.ComponentModel.DataAnnotations;

namespace crud_park_back.Models
{
    public class Operador : BaseEntity
    {
        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;
        
        [StringLength(200)]
        public string? Email { get; set; }
        
        // Navegación
        public virtual ICollection<Ingreso> Ingresos { get; set; } = new List<Ingreso>();
    }
}

