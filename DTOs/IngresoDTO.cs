using System.ComponentModel.DataAnnotations;
using crud_park_back.Models;

namespace crud_park_back.DTOs
{
    public class IngresoDTO
    {
        public int Id { get; set; }
        public string NumeroFolio { get; set; } = string.Empty;
        public string Placa { get; set; } = string.Empty;
        public DateTime FechaIngreso { get; set; }
        public DateTime? FechaSalida { get; set; }
        public TipoIngreso TipoIngreso { get; set; }
        public decimal ValorCobrado { get; set; }
        public int? TiempoEstadiaMinutos { get; set; }
        public bool Pagado { get; set; }
        public string? QrCode { get; set; }
        public int? OperadorIngresoId { get; set; }
        public int? OperadorSalidaId { get; set; }
        public string? OperadorIngresoNombre { get; set; }
        public string? OperadorSalidaNombre { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
    
    public class CreateIngresoDTO
    {
        [Required(ErrorMessage = "La placa es requerida")]
        [StringLength(10, ErrorMessage = "La placa no puede exceder 10 caracteres")]
        public string Placa { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "El tipo de ingreso es requerido")]
        public TipoIngreso TipoIngreso { get; set; }
        
        [Required(ErrorMessage = "El operador es requerido")]
        public int OperadorIngresoId { get; set; }
    }
    
    public class FinalizarIngresoDTO
    {
        [Required(ErrorMessage = "El ID del ingreso es requerido")]
        public int IngresoId { get; set; }
        
        [Required(ErrorMessage = "El operador es requerido")]
        public int OperadorSalidaId { get; set; }
    }
    
    public class IngresoResumenDTO
    {
        public int TotalVehiculosActivos { get; set; }
        public int TotalIngresosHoy { get; set; }
        public decimal TotalIngresosHoyValor { get; set; }
        public int MensualidadesActivas { get; set; }
        public int MensualidadesProximasVencer { get; set; }
        public int MensualidadesVencidas { get; set; }
    }
}

