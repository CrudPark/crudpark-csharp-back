using System.ComponentModel.DataAnnotations;
using crud_park_back.Models;

namespace crud_park_back.DTOs
{
    public class TurnoDTO
    {
        public int Id { get; set; }
        public DateTime FechaApertura { get; set; }
        public DateTime? FechaCierre { get; set; }
        public decimal TotalIngresos { get; set; }
        public int TotalVehiculos { get; set; }
        public string? Observaciones { get; set; }
        public int OperadorId { get; set; }
        public string? OperadorNombre { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
    
    public class CreateTurnoDTO
    {
        [Required(ErrorMessage = "El operador es requerido")]
        public int OperadorId { get; set; }
    }
    
    public class CerrarTurnoDTO
    {
        [Required(ErrorMessage = "El ID del turno es requerido")]
        public int TurnoId { get; set; }
    }
}

