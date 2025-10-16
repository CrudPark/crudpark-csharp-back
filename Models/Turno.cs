using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace crud_park_back.Models
{
    public class Turno : BaseEntity
    {
        [Required]
        public DateTime FechaApertura { get; set; }
        
        public DateTime? FechaCierre { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalIngresos { get; set; } = 0;
        
        public int TotalVehiculos { get; set; } = 0;
        
        [StringLength(500)]
        public string? Observaciones { get; set; }
        
        // Clave foránea
        [Required]
        public int OperadorId { get; set; }
        
        // Navegación
        public virtual Operador Operador { get; set; } = null!;
    }
}

