using Microsoft.AspNetCore.Mvc;
using crud_park_back.DTOs;
using crud_park_back.Services;

namespace crud_park_back.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportesController : ControllerBase
    {
        private readonly IReporteService _reporteService;
        private readonly ILogger<ReportesController> _logger;

        public ReportesController(IReporteService reporteService, ILogger<ReportesController> logger)
        {
            _reporteService = reporteService;
            _logger = logger;
        }

        /// <summary>
        /// Genera un reporte de ingresos para un rango de fechas
        /// </summary>
        [HttpGet("ingresos")]
        public async Task<ActionResult<ReporteIngresosDTO>> GenerarReporteIngresos(
            [FromQuery] DateTime fechaInicio, 
            [FromQuery] DateTime fechaFin)
        {
            try
            {
                if (fechaInicio > fechaFin)
                {
                    return BadRequest("La fecha de inicio no puede ser posterior a la fecha de fin");
                }

                var reporte = await _reporteService.GenerarReporteIngresosAsync(fechaInicio, fechaFin);
                return Ok(reporte);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generando reporte de ingresos");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Obtiene el detalle de ingresos diarios para un rango de fechas
        /// </summary>
        [HttpGet("ingresos-diarios")]
        public async Task<ActionResult<IEnumerable<IngresoDetalleDTO>>> GetIngresosDiarios(
            [FromQuery] DateTime fechaInicio, 
            [FromQuery] DateTime fechaFin)
        {
            try
            {
                if (fechaInicio > fechaFin)
                {
                    return BadRequest("La fecha de inicio no puede ser posterior a la fecha de fin");
                }

                var ingresosDiarios = await _reporteService.GetIngresosDiariosAsync(fechaInicio, fechaFin);
                return Ok(ingresosDiarios);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo ingresos diarios");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Obtiene el promedio de ocupación para un rango de fechas
        /// </summary>
        [HttpGet("promedio-ocupacion")]
        public async Task<ActionResult<decimal>> GetPromedioOcupacion(
            [FromQuery] DateTime fechaInicio, 
            [FromQuery] DateTime fechaFin)
        {
            try
            {
                if (fechaInicio > fechaFin)
                {
                    return BadRequest("La fecha de inicio no puede ser posterior a la fecha de fin");
                }

                var promedio = await _reporteService.GetPromedioOcupacionAsync(fechaInicio, fechaFin);
                return Ok(new { promedioOcupacion = promedio });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo promedio de ocupación");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Exporta un reporte de ingresos en formato CSV
        /// </summary>
        [HttpGet("exportar-csv")]
        public async Task<IActionResult> ExportarReporteCSV(
            [FromQuery] DateTime fechaInicio, 
            [FromQuery] DateTime fechaFin)
        {
            try
            {
                if (fechaInicio > fechaFin)
                {
                    return BadRequest("La fecha de inicio no puede ser posterior a la fecha de fin");
                }

                var csvData = await _reporteService.ExportarReporteCSVAsync(fechaInicio, fechaFin);
                var fileName = $"reporte_ingresos_{fechaInicio:yyyyMMdd}_{fechaFin:yyyyMMdd}.csv";

                return File(csvData, "text/csv", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exportando reporte CSV");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Exporta un reporte de ingresos en formato Excel
        /// </summary>
        [HttpGet("exportar-excel")]
        public async Task<IActionResult> ExportarReporteExcel(
            [FromQuery] DateTime fechaInicio, 
            [FromQuery] DateTime fechaFin)
        {
            try
            {
                if (fechaInicio > fechaFin)
                {
                    return BadRequest("La fecha de inicio no puede ser posterior a la fecha de fin");
                }

                var excelData = await _reporteService.ExportarReporteExcelAsync(fechaInicio, fechaFin);
                var fileName = $"reporte_ingresos_{fechaInicio:yyyyMMdd}_{fechaFin:yyyyMMdd}.xlsx";

                return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exportando reporte Excel");
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
}
