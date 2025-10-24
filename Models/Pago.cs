using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace crud_park_back.Models
{
    public class Pago : BaseEntity
    {
        [Required]
        public int TicketId { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Monto { get; set; }
        
        [Required]
        [StringLength(20)]
        public string MetodoPago { get; set; } = string.Empty;
        
        [Required]
        public int OperadorId { get; set; }
        
        [StringLength(500)]
        public string? Observaciones { get; set; }
        
        // Navegación
        public virtual Ingreso Ticket { get; set; } = null!;
        public virtual Operador Operador { get; set; } = null!;
    }
}