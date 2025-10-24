using crud_park_back.DTOs;

namespace crud_park_back.Services
{
    public interface IDashboardService
    {
        Task<DashboardDTO> GetDashboardDataAsync();
        Task<IngresoResumenDTO> GetResumenIngresosAsync();
        Task<IEnumerable<IngresoDiarioDTO>> GetIngresosPorHoraAsync(DateTime fecha);
        Task<IEnumerable<MensualidadVencimientoDTO>> GetMensualidadesProximasVencerAsync(int dias = 3);
    }
}
