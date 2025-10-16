using System.ComponentModel.DataAnnotations;

namespace crud_park_back.Models
{
    public class Configuracion : BaseEntity
    {
        [Required]
        [StringLength(50)]
        public string Clave { get; set; } = string.Empty;
        
        [Required]
        public string Valor { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Descripcion { get; set; }
    }
}
