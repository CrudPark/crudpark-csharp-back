using System.ComponentModel.DataAnnotations;

namespace crud_park_back.Models
{
    public class Operador : BaseEntity
    {
        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string? Email { get; set; }
        
        // Navegación
        public virtual ICollection<Ingreso> IngresosIngreso { get; set; } = new List<Ingreso>();
        public virtual ICollection<Ingreso> IngresosSalida { get; set; } = new List<Ingreso>();
        public virtual ICollection<Turno> Turnos { get; set; } = new List<Turno>();
        public virtual ICollection<Pago> Pagos { get; set; } = new List<Pago>();
    }
}

