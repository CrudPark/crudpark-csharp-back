using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace crud_park_back.Models
{
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
        [StringLength(20)]
        public string TipoIngreso { get; set; } = "Invitado";
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal MontoCobrado { get; set; } = 0;
        
        public int? TiempoEstadiaMinutos { get; set; }
        
        public bool Pagado { get; set; } = false;
        
        public bool Activo { get; set; } = true;
        
        [StringLength(500)]
        public string? QrCode { get; set; }
        
        [Required]
        [StringLength(20)]
        public string Estado { get; set; } = "Activo";
        
        // Claves foráneas
        public int? OperadorIngresoId { get; set; }
        public int? OperadorSalidaId { get; set; }
        public int? MensualidadId { get; set; }
        public int? TarifaId { get; set; }
        
        // Navegación
        public virtual Operador? OperadorIngreso { get; set; }
        public virtual Operador? OperadorSalida { get; set; }
        public virtual Mensualidad? Mensualidad { get; set; }
        public virtual Tarifa? Tarifa { get; set; }
        public virtual ICollection<Pago> Pagos { get; set; } = new List<Pago>();
    }
}

