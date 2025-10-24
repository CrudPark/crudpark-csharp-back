using Microsoft.AspNetCore.Mvc;
using crud_park_back.DTOs;
using crud_park_back.Services;
using crud_park_back.Models;
using Microsoft.EntityFrameworkCore;

namespace crud_park_back.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;
        private readonly ILogger<DashboardController> _logger;
        private readonly ParkingDbContext _context;

        public DashboardController(IDashboardService dashboardService, ILogger<DashboardController> logger, ParkingDbContext context)
        {
            _dashboardService = dashboardService;
            _logger = logger;
            _context = context;
        }

        /// <summary>
        /// Obtiene los datos del dashboard principal
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<DashboardDTO>> GetDashboardData()
        {
            try
            {
                var dashboardData = await _dashboardService.GetDashboardDataAsync();
                return Ok(dashboardData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo datos del dashboard");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("test")]
        public async Task<ActionResult> TestConnection()
        {
            try
            {
                var count = await _context.Operadores.CountAsync();
                return Ok(new { message = "Conexión exitosa", operadores = count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en test de conexión");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene el resumen de ingresos
        /// </summary>
        [HttpGet("resumen")]
        public async Task<ActionResult<IngresoResumenDTO>> GetResumenIngresos()
        {
            try
            {
                var resumen = await _dashboardService.GetResumenIngresosAsync();
                return Ok(resumen);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo resumen de ingresos");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Obtiene los ingresos por hora de un día específico
        /// </summary>
        [HttpGet("ingresos-por-hora")]
        public async Task<ActionResult<IEnumerable<IngresoDiarioDTO>>> GetIngresosPorHora([FromQuery] DateTime? fecha = null)
        {
            try
            {
                var fechaConsulta = fecha ?? DateTime.Today;
                var ingresosPorHora = await _dashboardService.GetIngresosPorHoraAsync(fechaConsulta);
                return Ok(ingresosPorHora);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo ingresos por hora");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Obtiene las mensualidades próximas a vencer
        /// </summary>
        [HttpGet("mensualidades-proximas")]
        public async Task<ActionResult<IEnumerable<MensualidadVencimientoDTO>>> GetMensualidadesProximas([FromQuery] int dias = 3)
        {
            try
            {
                var mensualidades = await _dashboardService.GetMensualidadesProximasVencerAsync(dias);
                return Ok(mensualidades);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo mensualidades próximas a vencer");
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
}
