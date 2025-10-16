using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace crud_park_back.Models
{
    public enum TipoIngreso
    {
        Mensualidad = 1,
        Invitado = 2
    }
    
    public class Ingreso : BaseEntity
    {
        [Required]
        [StringLength(20)]
        public string NumeroFolio { get; set; } = string.Empty;
        
        [Required]
        [StringLength(10)]
        public string Placa { get; set; } = string.Empty;
        
        [Required]
        public DateTime FechaIngreso { get; set; }
        
        public DateTime? FechaSalida { get; set; }
        
        [Required]
        public TipoIngreso TipoIngreso { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal ValorCobrado { get; set; } = 0;
        
        public int? TiempoEstadiaMinutos { get; set; }
        
        public bool Pagado { get; set; } = false;
        
        [StringLength(500)]
        public string? QrCode { get; set; }
        
        // Claves foráneas
        public int? OperadorIngresoId { get; set; }
        public int? OperadorSalidaId { get; set; }
        
        // Navegación
        public virtual Operador? OperadorIngreso { get; set; }
        public virtual Operador? OperadorSalida { get; set; }
    }
}

