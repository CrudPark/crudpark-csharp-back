using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using crud_park_back.Models;

namespace crud_park_back.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly ParkingDbContext _context;
        private readonly ILogger<HealthController> _logger;

        public HealthController(ParkingDbContext context, ILogger<HealthController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Health check endpoint
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                // Check database connectivity
                await _context.Database.CanConnectAsync();
                
                return Ok(new
                {
                    status = "healthy",
                    timestamp = DateTime.UtcNow,
                    version = "1.0.0",
                    database = "connected"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed");
                return StatusCode(503, new
                {
                    status = "unhealthy",
                    timestamp = DateTime.UtcNow,
                    version = "1.0.0",
                    database = "disconnected",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Simple health check for load balancers
        /// </summary>
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok(new { message = "pong", timestamp = DateTime.UtcNow });
        }
    }
}
