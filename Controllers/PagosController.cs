using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using crud_park_back.Models;
using System.ComponentModel.DataAnnotations;

namespace crud_park_back.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PagosController : ControllerBase
    {
        private readonly ParkingDbContext _context;
        private readonly ILogger<PagosController> _logger;

        public PagosController(ParkingDbContext context, ILogger<PagosController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/pagos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetPagos()
        {
            try
            {
                var pagos = await _context.Pagos
                    .Include(p => p.Operador)
                    .Include(p => p.Ticket)
                    .OrderByDescending(p => p.CreatedAt)
                    .Select(p => new
                    {
                        Id = p.Id,
                        TicketId = p.TicketId,
                        Monto = p.Monto,
                        MetodoPago = p.MetodoPago,
                        OperadorId = p.OperadorId,
                        Operador = p.Operador != null ? new
                        {
                            Nombre = p.Operador.Nombre
                        } : null,
                        FechaPago = p.CreatedAt,
                        Observaciones = p.Observaciones,
                        Ticket = p.Ticket != null ? new
                        {
                            NumeroFolio = p.Ticket.NumeroFolio,
                            Placa = p.Ticket.Placa
                        } : null
                    })
                    .ToListAsync();

                return Ok(pagos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener pagos");
                return StatusCode(500, new { message = "Error al obtener los pagos", error = ex.Message });
            }
        }

        // GET: api/pagos/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetPago(int id)
        {
            try
            {
                var pago = await _context.Pagos
                    .Include(p => p.Operador)
                    .Include(p => p.Ticket)
                    .Where(p => p.Id == id)
                    .Select(p => new
                    {
                        Id = p.Id,
                        TicketId = p.TicketId,
                        Monto = p.Monto,
                        MetodoPago = p.MetodoPago,
                        OperadorId = p.OperadorId,
                        Operador = p.Operador != null ? new
                        {
                            Nombre = p.Operador.Nombre
                        } : null,
                        FechaPago = p.CreatedAt,
                        Observaciones = p.Observaciones,
                        Ticket = p.Ticket != null ? new
                        {
                            NumeroFolio = p.Ticket.NumeroFolio,
                            Placa = p.Ticket.Placa
                        } : null
                    })
                    .FirstOrDefaultAsync();

                if (pago == null)
                {
                    return NotFound(new { message = "Pago no encontrado" });
                }

                return Ok(pago);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el pago {Id}", id);
                return StatusCode(500, new { message = "Error al obtener el pago", error = ex.Message });
            }
        }

        // POST: api/pagos
        [HttpPost]
        public async Task<ActionResult<object>> CreatePago([FromBody] CreatePagoDTO dto)
        {
            try
            {
                // Validar que el ticket existe
                var ticket = await _context.Ingresos.FindAsync(dto.TicketId);
                if (ticket == null)
                {
                    return BadRequest(new { message = "El ticket especificado no existe" });
                }

                // Validar que el ticket no esté ya pagado
                if (ticket.Pagado)
                {
                    return BadRequest(new { message = "El ticket ya está pagado" });
                }

                // Validar que el ticket tenga salida
                if (ticket.FechaSalida == null)
                {
                    return BadRequest(new { message = "El ticket debe tener salida antes de registrar el pago" });
                }

                // Validar que el ticket esté finalizado
                if (ticket.Estado != "Finalizado")
                {
                    return BadRequest(new { message = "El ticket debe estar en estado 'Finalizado' para registrar el pago" });
                }

                // Validar que el monto sea mayor a 0
                if (ticket.MontoCobrado <= 0)
                {
                    return BadRequest(new { message = "El ticket no tiene un monto válido para cobrar" });
                }

                // Validar que el monto del pago coincida con el monto del ticket
                if (dto.Monto != ticket.MontoCobrado)
                {
                    return BadRequest(new { message = $"El monto del pago debe ser {ticket.MontoCobrado:C}" });
                }

                // Validar que el operador existe y esté activo
                var operador = await _context.Operadores.FindAsync(dto.OperadorId);
                if (operador == null)
                {
                    return BadRequest(new { message = "El operador especificado no existe" });
                }
                
                if (!operador.IsActive)
                {
                    return BadRequest(new { message = "El operador especificado no está activo" });
                }

                // Validar método de pago
                var metodosValidos = new[] { "Efectivo", "Tarjeta", "Transferencia" };
                if (!metodosValidos.Contains(dto.MetodoPago))
                {
                    return BadRequest(new { message = "Método de pago no válido. Use: Efectivo, Tarjeta o Transferencia" });
                }

                var pago = new Pago
                {
                    TicketId = dto.TicketId,
                    Monto = dto.Monto,
                    MetodoPago = dto.MetodoPago,
                    OperadorId = dto.OperadorId,
                    Observaciones = dto.Observaciones,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Pagos.Add(pago);
                await _context.SaveChangesAsync();

                // Marcar el ticket como pagado
                ticket.Pagado = true;
                ticket.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // Cargar relaciones para respuesta
                await _context.Entry(pago).Reference(p => p.Operador).LoadAsync();
                await _context.Entry(pago).Reference(p => p.Ticket).LoadAsync();

                var response = new
                {
                    Id = pago.Id,
                    TicketId = pago.TicketId,
                    Monto = pago.Monto,
                    MetodoPago = pago.MetodoPago,
                    OperadorId = pago.OperadorId,
                    Operador = pago.Operador != null ? new
                    {
                        Nombre = pago.Operador.Nombre
                    } : null,
                    FechaPago = pago.CreatedAt,
                    Observaciones = pago.Observaciones,
                    Ticket = pago.Ticket != null ? new
                    {
                        NumeroFolio = pago.Ticket.NumeroFolio,
                        Placa = pago.Ticket.Placa
                    } : null
                };

                return CreatedAtAction(nameof(GetPago), new { id = pago.Id }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear pago");
                return StatusCode(500, new { message = "Error al crear el pago", error = ex.Message });
            }
        }

        // DELETE: api/pagos/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePago(int id)
        {
            try
            {
                var pago = await _context.Pagos.FindAsync(id);
                if (pago == null)
                {
                    return NotFound(new { message = "Pago no encontrado" });
                }

                // Obtener el ticket antes de eliminar el pago
                var ticket = await _context.Ingresos.FindAsync(pago.TicketId);

                _context.Pagos.Remove(pago);
                await _context.SaveChangesAsync();

                // Si el ticket existe, marcar como no pagado
                if (ticket != null)
                {
                    ticket.Pagado = false;
                    ticket.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar pago {Id}", id);
                return StatusCode(500, new { message = "Error al eliminar el pago", error = ex.Message });
            }
        }

        // GET: api/pagos/tickets-no-pagados
        [HttpGet("tickets-no-pagados")]
        public async Task<ActionResult<IEnumerable<object>>> GetTicketsNoPagados()
        {
            try
            {
                var tickets = await _context.Ingresos
                    .Where(t => !t.Pagado && 
                                t.FechaSalida != null && 
                                t.MontoCobrado > 0 &&
                                (t.Estado == "Finalizado" || t.Estado == null || t.Estado == "") &&
                                t.TipoIngreso == "Invitado")
                    .OrderByDescending(t => t.FechaSalida)
                    .Select(t => new
                    {
                        Id = t.Id,
                        NumeroFolio = t.NumeroFolio,
                        Placa = t.Placa,
                        FechaIngreso = t.FechaIngreso,
                        FechaSalida = t.FechaSalida,
                        MontoCobrado = t.MontoCobrado
                    })
                    .ToListAsync();

                _logger.LogInformation($"Encontrados {tickets.Count} tickets disponibles para pago");
                return Ok(tickets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener tickets no pagados");
                return StatusCode(500, new { message = "Error al obtener tickets no pagados", error = ex.Message });
            }
        }

        // GET: api/pagos/ticket/{ticketId}
        [HttpGet("ticket/{ticketId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetPagosByTicket(int ticketId)
        {
            try
            {
                var pagos = await _context.Pagos
                    .Include(p => p.Operador)
                    .Where(p => p.TicketId == ticketId)
                    .OrderByDescending(p => p.CreatedAt)
                    .Select(p => new
                    {
                        Id = p.Id,
                        TicketId = p.TicketId,
                        Monto = p.Monto,
                        MetodoPago = p.MetodoPago,
                        OperadorId = p.OperadorId,
                        Operador = p.Operador != null ? new
                        {
                            Nombre = p.Operador.Nombre
                        } : null,
                        FechaPago = p.CreatedAt,
                        Observaciones = p.Observaciones
                    })
                    .ToListAsync();

                return Ok(pagos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener pagos del ticket {TicketId}", ticketId);
                return StatusCode(500, new { message = "Error al obtener los pagos", error = ex.Message });
            }
        }
    }

    // DTO para crear pagos
    public class CreatePagoDTO
    {
        [Required(ErrorMessage = "El ID del ticket es requerido")]
        public int TicketId { get; set; }

        [Required(ErrorMessage = "El monto es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        public decimal Monto { get; set; }

        [Required(ErrorMessage = "El método de pago es requerido")]
        [StringLength(20)]
        public string MetodoPago { get; set; } = string.Empty;

        [Required(ErrorMessage = "El ID del operador es requerido")]
        public int OperadorId { get; set; }

        [StringLength(500)]
        public string? Observaciones { get; set; }
    }
}

