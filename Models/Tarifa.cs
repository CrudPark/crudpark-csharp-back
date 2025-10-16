using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace crud_park_back.Models
{
    public class Tarifa : BaseEntity
    {
        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal ValorBaseHora { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal ValorAdicionalFraccion { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal TopeDiario { get; set; }
        
        public int TiempoGraciaMinutos { get; set; } = 30;
        
        public bool EsActiva { get; set; } = true;
        
        // Navegación
        public virtual ICollection<Ingreso> Ingresos { get; set; } = new List<Ingreso>();
    }
}

