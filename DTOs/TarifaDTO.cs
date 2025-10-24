using System.ComponentModel.DataAnnotations;

namespace crud_park_back.DTOs
{
    public class TarifaDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public decimal ValorBaseHora { get; set; }
        public decimal ValorAdicionalFraccion { get; set; }
        public decimal TopeDiario { get; set; }
        public int TiempoGraciaMinutos { get; set; }
        public bool EsActiva { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
    
    public class CreateTarifaDTO
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "El valor base por hora es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El valor base debe ser mayor a 0")]
        public decimal ValorBaseHora { get; set; }
        
        [Required(ErrorMessage = "El valor adicional por fracción es requerido")]
        [Range(0, double.MaxValue, ErrorMessage = "El valor adicional no puede ser negativo")]
        public decimal ValorAdicionalFraccion { get; set; }
        
        [Required(ErrorMessage = "El tope diario es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El tope diario debe ser mayor a 0")]
        public decimal TopeDiario { get; set; }
        
        [Required(ErrorMessage = "El tiempo de gracia es requerido")]
        [Range(30, int.MaxValue, ErrorMessage = "El tiempo de gracia mínimo es 30 minutos")]
        public int TiempoGraciaMinutos { get; set; } = 30;
        
        public bool IsActive { get; set; } = true;
    }
    
    public class UpdateTarifaDTO
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "El valor base por hora es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El valor base debe ser mayor a 0")]
        public decimal ValorBaseHora { get; set; }
        
        [Required(ErrorMessage = "El valor adicional por fracción es requerido")]
        [Range(0, double.MaxValue, ErrorMessage = "El valor adicional no puede ser negativo")]
        public decimal ValorAdicionalFraccion { get; set; }
        
        [Required(ErrorMessage = "El tope diario es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El tope diario debe ser mayor a 0")]
        public decimal TopeDiario { get; set; }
        
        [Required(ErrorMessage = "El tiempo de gracia es requerido")]
        [Range(30, int.MaxValue, ErrorMessage = "El tiempo de gracia mínimo es 30 minutos")]
        public int TiempoGraciaMinutos { get; set; } = 30;
        
        public bool IsActive { get; set; } = true;
    }
}

