using System.ComponentModel.DataAnnotations;

namespace crud_park_back.DTOs
{
    public class MensualidadDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Placa { get; set; } = string.Empty;
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public decimal Valor { get; set; }
        public bool NotificacionEnviada { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
    
    public class CreateMensualidadDTO
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;
        
        [EmailAddress(ErrorMessage = "El formato del email no es válido")]
        [StringLength(200, ErrorMessage = "El email no puede exceder 200 caracteres")]
        public string? Email { get; set; }
        
        [Required(ErrorMessage = "La placa es requerida")]
        [StringLength(10, ErrorMessage = "La placa no puede exceder 10 caracteres")]
        public string Placa { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "La fecha de inicio es requerida")]
        public DateTime FechaInicio { get; set; }
        
        [Required(ErrorMessage = "La fecha de fin es requerida")]
        public DateTime FechaFin { get; set; }
    }
    
    public class UpdateMensualidadDTO
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;
        
        [EmailAddress(ErrorMessage = "El formato del email no es válido")]
        [StringLength(200, ErrorMessage = "El email no puede exceder 200 caracteres")]
        public string? Email { get; set; }
        
        [Required(ErrorMessage = "La placa es requerida")]
        [StringLength(10, ErrorMessage = "La placa no puede exceder 10 caracteres")]
        public string Placa { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "La fecha de inicio es requerida")]
        public DateTime FechaInicio { get; set; }
        
        [Required(ErrorMessage = "La fecha de fin es requerida")]
        public DateTime FechaFin { get; set; }
        
        public bool IsActive { get; set; } = true;
    }
}

