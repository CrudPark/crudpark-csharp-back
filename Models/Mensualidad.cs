using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace crud_park_back.Models
{
    public class Mensualidad : BaseEntity
    {
        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;
        
        [StringLength(200)]
        public string? Email { get; set; }
        
        [Required]
        [StringLength(10)]
        public string Placa { get; set; } = string.Empty;
        
        [Required]
        public DateTime FechaInicio { get; set; }
        
        [Required]
        public DateTime FechaFin { get; set; }
        
        public bool NotificacionEnviada { get; set; } = false;
    }
}

