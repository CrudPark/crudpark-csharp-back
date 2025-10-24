using crud_park_back.DTOs;

namespace crud_park_back.Services
{
    public interface IReporteService
    {
        Task<ReporteIngresosDTO> GenerarReporteIngresosAsync(DateTime fechaInicio, DateTime fechaFin);
        Task<byte[]> ExportarReporteCSVAsync(DateTime fechaInicio, DateTime fechaFin);
        Task<byte[]> ExportarReporteExcelAsync(DateTime fechaInicio, DateTime fechaFin);
        Task<IEnumerable<IngresoDetalleDTO>> GetIngresosDiariosAsync(DateTime fechaInicio, DateTime fechaFin);
        Task<decimal> GetPromedioOcupacionAsync(DateTime fechaInicio, DateTime fechaFin);
    }
}
