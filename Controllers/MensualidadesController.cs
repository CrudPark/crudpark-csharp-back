using Microsoft.AspNetCore.Mvc;
using crud_park_back.DTOs;
using crud_park_back.Services;

namespace crud_park_back.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MensualidadesController : ControllerBase
    {
        private readonly IParkingService _parkingService;
        private readonly ILogger<MensualidadesController> _logger;

        public MensualidadesController(IParkingService parkingService, ILogger<MensualidadesController> logger)
        {
            _parkingService = parkingService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todas las mensualidades activas
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MensualidadDTO>>> GetMensualidades()
        {
            try
            {
                var mensualidades = await _parkingService.GetMensualidadesAsync();
                return Ok(mensualidades);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo mensualidades");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Obtiene una mensualidad por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<MensualidadDTO>> GetMensualidad(int id)
        {
            try
            {
                var mensualidad = await _parkingService.GetMensualidadByIdAsync(id);
                if (mensualidad == null)
                {
                    return NotFound($"Mensualidad con ID {id} no encontrada");
                }
                return Ok(mensualidad);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo mensualidad con ID {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Crea una nueva mensualidad
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<MensualidadDTO>> CreateMensualidad([FromBody] CreateMensualidadDTO mensualidadDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var mensualidad = await _parkingService.CreateMensualidadAsync(mensualidadDto);
                return CreatedAtAction(nameof(GetMensualidad), new { id = mensualidad.Id }, mensualidad);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando mensualidad");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Actualiza una mensualidad existente
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<MensualidadDTO>> UpdateMensualidad(int id, [FromBody] UpdateMensualidadDTO mensualidadDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var mensualidad = await _parkingService.UpdateMensualidadAsync(id, mensualidadDto);
                if (mensualidad == null)
                {
                    return NotFound($"Mensualidad con ID {id} no encontrada");
                }
                return Ok(mensualidad);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando mensualidad con ID {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Elimina una mensualidad (marcándola como inactiva)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMensualidad(int id)
        {
            try
            {
                var resultado = await _parkingService.DeleteMensualidadAsync(id);
                if (!resultado)
                {
                    return NotFound($"Mensualidad con ID {id} no encontrada");
                }
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando mensualidad con ID {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Activa o desactiva una mensualidad
        /// </summary>
        [HttpPatch("{id}/toggle-estado")]
        public async Task<ActionResult<MensualidadDTO>> ToggleMensualidadEstado(int id)
        {
            try
            {
                var mensualidad = await _parkingService.ToggleMensualidadEstadoAsync(id);
                if (mensualidad == null)
                {
                    return NotFound(new { message = $"Mensualidad con ID {id} no encontrada" });
                }
                return Ok(mensualidad);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cambiando estado de mensualidad {Id}", id);
                return StatusCode(500, new { message = "Error al cambiar estado de la mensualidad", details = ex.Message });
            }
        }

        /// <summary>
        /// Verifica si existe una mensualidad vigente para una placa
        /// </summary>
        [HttpGet("verificar-vigencia/{placa}")]
        public async Task<ActionResult<bool>> VerificarMensualidadVigente(string placa)
        {
            try
            {
                var existeVigente = await _parkingService.ExisteMensualidadVigenteAsync(placa);
                return Ok(existeVigente);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verificando mensualidad vigente para placa {Placa}", placa);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Obtiene las mensualidades próximas a vencer
        /// </summary>
        [HttpGet("proximas-vencer")]
        public async Task<ActionResult<IEnumerable<MensualidadDTO>>> GetMensualidadesProximasVencer([FromQuery] int dias = 3)
        {
            try
            {
                var mensualidades = await _parkingService.GetMensualidadesProximasVencerAsync(dias);
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
