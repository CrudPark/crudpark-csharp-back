using Microsoft.AspNetCore.Mvc;
using crud_park_back.DTOs;
using crud_park_back.Services;

namespace crud_park_back.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IngresosController : ControllerBase
    {
        private readonly IParkingService _parkingService;
        private readonly ILogger<IngresosController> _logger;

        public IngresosController(IParkingService parkingService, ILogger<IngresosController> logger)
        {
            _parkingService = parkingService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todos los ingresos
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<IngresoDTO>>> GetIngresos()
        {
            try
            {
                var ingresos = await _parkingService.GetIngresosAsync();
                return Ok(ingresos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo ingresos");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Endpoint de utilidad para corregir tickets con datos inconsistentes
        /// </summary>
        [HttpPost("corregir-tickets-malos")]
        public async Task<ActionResult> CorregirTicketsMalos()
        {
            try
            {
                var result = await _parkingService.CorregirTicketsMalosAsync();
                _logger.LogInformation($"Tickets corregidos: {result.TicketsCorregidos}");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error corrigiendo tickets malos");
                return StatusCode(500, new { message = "Error al corregir tickets", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene un ingreso por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<IngresoDTO>> GetIngreso(int id)
        {
            try
            {
                var ingreso = await _parkingService.GetIngresoByIdAsync(id);
                if (ingreso == null)
                {
                    return NotFound($"Ingreso con ID {id} no encontrado");
                }
                return Ok(ingreso);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo ingreso con ID {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Obtiene todos los ingresos activos (vehículos en el parqueadero)
        /// </summary>
        [HttpGet("activos")]
        public async Task<ActionResult<IEnumerable<IngresoDTO>>> GetIngresosActivos()
        {
            try
            {
                var ingresos = await _parkingService.GetIngresosActivosAsync();
                return Ok(ingresos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo ingresos activos");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Obtiene el ingreso activo de una placa específica
        /// </summary>
        [HttpGet("activo/{placa}")]
        public async Task<ActionResult<IngresoDTO>> GetIngresoActivoPorPlaca(string placa)
        {
            try
            {
                var ingreso = await _parkingService.GetIngresoActivoPorPlacaAsync(placa);
                if (ingreso == null)
                {
                    return NotFound($"No hay ingreso activo para la placa {placa}");
                }
                return Ok(ingreso);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo ingreso activo para placa {Placa}", placa);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Registra un nuevo ingreso de vehículo
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<IngresoDTO>> CreateIngreso([FromBody] CreateIngresoDTO ingresoDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var ingreso = await _parkingService.CreateIngresoAsync(ingresoDto);
                return CreatedAtAction(nameof(GetIngreso), new { id = ingreso.Id }, ingreso);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando ingreso");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Finaliza un ingreso (registra la salida del vehículo)
        /// </summary>
        [HttpPost("finalizar")]
        public async Task<ActionResult<IngresoDTO>> FinalizarIngreso([FromBody] FinalizarIngresoDTO finalizarDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var ingreso = await _parkingService.FinalizarIngresoAsync(finalizarDto);
                if (ingreso == null)
                {
                    return NotFound($"Ingreso con ID {finalizarDto.IngresoId} no encontrado o ya finalizado");
                }
                return Ok(ingreso);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finalizando ingreso");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Calcula el valor a cobrar para un ingreso específico
        /// </summary>
        [HttpGet("{id}/calcular-cobro")]
        public async Task<ActionResult<decimal>> CalcularValorCobro(int id, [FromQuery] int tarifaId)
        {
            try
            {
                var valor = await _parkingService.CalcularValorCobroAsync(id, tarifaId);
                return Ok(new { valorCobro = valor });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculando valor de cobro para ingreso {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
}
