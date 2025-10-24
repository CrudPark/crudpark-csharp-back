using Microsoft.AspNetCore.Mvc;
using crud_park_back.DTOs;
using crud_park_back.Services;

namespace crud_park_back.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TurnosController : ControllerBase
    {
        private readonly IParkingService _parkingService;
        private readonly ILogger<TurnosController> _logger;

        public TurnosController(IParkingService parkingService, ILogger<TurnosController> logger)
        {
            _parkingService = parkingService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todos los turnos
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TurnoDTO>>> GetTurnos()
        {
            try
            {
                var turnos = await _parkingService.GetTurnosAsync();
                return Ok(turnos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo turnos");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Obtiene un turno por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<TurnoDTO>> GetTurno(int id)
        {
            try
            {
                var turno = await _parkingService.GetTurnoByIdAsync(id);
                if (turno == null)
                {
                    return NotFound($"Turno con ID {id} no encontrado");
                }
                return Ok(turno);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo turno con ID {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Obtiene el turno abierto de un operador específico
        /// </summary>
        [HttpGet("abierto/{operadorId}")]
        public async Task<ActionResult<TurnoDTO>> GetTurnoAbiertoPorOperador(int operadorId)
        {
            try
            {
                var turno = await _parkingService.GetTurnoAbiertoPorOperadorAsync(operadorId);
                if (turno == null)
                {
                    return NotFound($"No hay turno abierto para el operador con ID {operadorId}");
                }
                return Ok(turno);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo turno abierto para operador {OperadorId}", operadorId);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Crea un nuevo turno (abre turno)
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<TurnoDTO>> CrearTurno([FromBody] CreateTurnoDTO turnoDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var turno = await _parkingService.CrearTurnoAsync(turnoDto);
                return CreatedAtAction(nameof(GetTurno), new { id = turno.Id }, turno);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando turno");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Cierra un turno existente
        /// </summary>
        [HttpPost("cerrar")]
        public async Task<ActionResult<TurnoDTO>> CerrarTurno([FromBody] CerrarTurnoDTO cerrarDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var turno = await _parkingService.CerrarTurnoAsync(cerrarDto);
                if (turno == null)
                {
                    return NotFound($"Turno con ID {cerrarDto.TurnoId} no encontrado o ya cerrado");
                }
                return Ok(turno);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cerrando turno");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Activa o desactiva un turno
        /// </summary>
        [HttpPatch("{id}/toggle-estado")]
        public async Task<ActionResult<TurnoDTO>> ToggleEstadoTurno(int id)
        {
            try
            {
                var turno = await _parkingService.ToggleEstadoTurnoAsync(id);
                if (turno == null)
                {
                    return NotFound($"Turno con ID {id} no encontrado");
                }
                return Ok(turno);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cambiando estado del turno {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
}
