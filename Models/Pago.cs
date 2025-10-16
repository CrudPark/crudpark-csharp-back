using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace crud_park_back.Models
{
    public enum MetodoPago
    {
        Efectivo = 1,
        Tarjeta = 2,
        Transferencia = 3
    }
    
    public class Pago : BaseEntity
    {
        [Required]
        public int IngresoId { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Monto { get; set; }
        
        [Required]
        public MetodoPago MetodoPago { get; set; }
        
        public int? OperadorId { get; set; }
        
        public DateTime FechaPago { get; set; } = DateTime.UtcNow;
        
        [StringLength(500)]
        public string? Observaciones { get; set; }
        
        // Navegación
        public virtual Ingreso Ingreso { get; set; } = null!;
        public virtual Operador? Operador { get; set; }
    }
}
