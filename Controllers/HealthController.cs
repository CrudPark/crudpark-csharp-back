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
        public IActionResult Get()
        {
            return Ok(new
            {
                status = "healthy",
                timestamp = DateTime.UtcNow,
                version = "1.0.0",
                database = "connected"
            });
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
