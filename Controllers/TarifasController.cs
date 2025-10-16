using Microsoft.AspNetCore.Mvc;
using crud_park_back.DTOs;
using crud_park_back.Services;

namespace crud_park_back.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TarifasController : ControllerBase
    {
        private readonly IParkingService _parkingService;
        private readonly ILogger<TarifasController> _logger;

        public TarifasController(IParkingService parkingService, ILogger<TarifasController> logger)
        {
            _parkingService = parkingService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todas las tarifas activas
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TarifaDTO>>> GetTarifas()
        {
            try
            {
                var tarifas = await _parkingService.GetTarifasAsync();
                return Ok(tarifas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo tarifas");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Obtiene una tarifa por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<TarifaDTO>> GetTarifa(int id)
        {
            try
            {
                var tarifa = await _parkingService.GetTarifaByIdAsync(id);
                if (tarifa == null)
                {
                    return NotFound($"Tarifa con ID {id} no encontrada");
                }
                return Ok(tarifa);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo tarifa con ID {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Obtiene la tarifa activa actual
        /// </summary>
        [HttpGet("activa")]
        public async Task<ActionResult<TarifaDTO>> GetTarifaActiva()
        {
            try
            {
                var tarifa = await _parkingService.GetTarifaActivaAsync();
                if (tarifa == null)
                {
                    return NotFound("No hay tarifa activa configurada");
                }
                return Ok(tarifa);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo tarifa activa");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Crea una nueva tarifa
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<TarifaDTO>> CreateTarifa([FromBody] CreateTarifaDTO tarifaDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var tarifa = await _parkingService.CreateTarifaAsync(tarifaDto);
                return CreatedAtAction(nameof(GetTarifa), new { id = tarifa.Id }, tarifa);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando tarifa");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Actualiza una tarifa existente
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<TarifaDTO>> UpdateTarifa(int id, [FromBody] UpdateTarifaDTO tarifaDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var tarifa = await _parkingService.UpdateTarifaAsync(id, tarifaDto);
                if (tarifa == null)
                {
                    return NotFound($"Tarifa con ID {id} no encontrada");
                }
                return Ok(tarifa);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando tarifa con ID {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Elimina una tarifa (marcándola como inactiva)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteTarifa(int id)
        {
            try
            {
                var resultado = await _parkingService.DeleteTarifaAsync(id);
                if (!resultado)
                {
                    return NotFound($"Tarifa con ID {id} no encontrada");
                }
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando tarifa con ID {Id}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
}
