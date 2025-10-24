using Microsoft.AspNetCore.Mvc;
using crud_park_back.DTOs;
using crud_park_back.Services;

namespace crud_park_back.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OperadoresController : ControllerBase
    {
        private readonly IParkingService _parkingService;
        private readonly ILogger<OperadoresController> _logger;

        public OperadoresController(IParkingService parkingService, ILogger<OperadoresController> logger)
        {
            _parkingService = parkingService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todos los operadores activos
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OperadorDTO>>> GetOperadores()
        {
            try
            {
                var operadores = await _parkingService.GetOperadoresAsync();
                return Ok(operadores);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo operadores");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Obtiene un operador por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<OperadorDTO>> GetOperador(int id)
        {
            try
            {
                var operador = await _parkingService.GetOperadorByIdAsync(id);
                if (operador == null)
                {
                    return NotFound($"Operador con ID {id} no encontrado");
                }
                return Ok(operador);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo operador con ID {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Crea un nuevo operador
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<OperadorDTO>> CreateOperador([FromBody] CreateOperadorDTO operadorDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var operador = await _parkingService.CreateOperadorAsync(operadorDto);
                return CreatedAtAction(nameof(GetOperador), new { id = operador.Id }, operador);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando operador");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Actualiza un operador existente
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<OperadorDTO>> UpdateOperador(int id, [FromBody] UpdateOperadorDTO operadorDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var operador = await _parkingService.UpdateOperadorAsync(id, operadorDto);
                if (operador == null)
                {
                    return NotFound($"Operador con ID {id} no encontrado");
                }
                return Ok(operador);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando operador con ID {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Elimina un operador definitivamente de la base de datos
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteOperador(int id)
        {
            try
            {
                var resultado = await _parkingService.DeleteOperadorAsync(id);
                if (!resultado)
                {
                    return NotFound($"Operador con ID {id} no encontrado");
                }
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando operador con ID {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Activa o desactiva un operador
        /// </summary>
        [HttpPatch("{id}/toggle-estado")]
        public async Task<ActionResult<OperadorDTO>> ToggleEstadoOperador(int id)
        {
            try
            {
                var operador = await _parkingService.ToggleEstadoOperadorAsync(id);
                if (operador == null)
                {
                    return NotFound($"Operador con ID {id} no encontrado");
                }
                return Ok(operador);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cambiando estado del operador {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
}
