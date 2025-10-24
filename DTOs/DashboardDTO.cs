namespace crud_park_back.DTOs
{
    public class DashboardDTO
    {
        public int TotalOperadores { get; set; }
        public int VehiculosActualmenteEnParqueadero { get; set; }
        public decimal IngresosDelDia { get; set; }
        public int TotalIngresosDelDia { get; set; }
        public int MensualidadesActivas { get; set; }
        public int MensualidadesProximasVencer { get; set; }
        public int MensualidadesVencidas { get; set; }
        public List<IngresoDiarioDTO> IngresosPorHora { get; set; } = new List<IngresoDiarioDTO>();
        public List<MensualidadVencimientoDTO> MensualidadesProximas { get; set; } = new List<MensualidadVencimientoDTO>();
    }
    
    public class IngresoDiarioDTO
    {
        public int Hora { get; set; }
        public int CantidadIngresos { get; set; }
        public decimal ValorTotal { get; set; }
    }
    
    public class MensualidadVencimientoDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Placa { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime FechaFin { get; set; }
        public int DiasRestantes { get; set; }
    }
    
    public class ReporteIngresosDTO
    {
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public decimal TotalIngresos { get; set; }
        public int TotalVehiculos { get; set; }
        public decimal PromedioOcupacion { get; set; }
        public int MensualidadesVsInvitados { get; set; }
        public List<IngresoDetalleDTO> DetalleIngresos { get; set; } = new List<IngresoDetalleDTO>();
    }
    
    public class IngresoDetalleDTO
    {
        public DateTime Fecha { get; set; }
        public int CantidadIngresos { get; set; }
        public decimal ValorTotal { get; set; }
        public int Mensualidades { get; set; }
        public int Invitados { get; set; }
    }
}

