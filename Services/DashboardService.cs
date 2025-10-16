using AutoMapper;
using crud_park_back.DTOs;
using crud_park_back.Models;
using Microsoft.EntityFrameworkCore;

namespace crud_park_back.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly ParkingDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<DashboardService> _logger;

        public DashboardService(ParkingDbContext context, IMapper mapper, ILogger<DashboardService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<DashboardDTO> GetDashboardDataAsync()
        {
            var hoy = DateTime.Today;
            var finDia = hoy.AddDays(1);

            // Vehículos actualmente en el parqueadero
            var vehiculosEnParqueadero = await _context.Ingresos
                .CountAsync(i => i.IsActive && i.FechaSalida == null);

            // Ingresos del día
            var ingresosDelDia = await _context.Ingresos
                .Where(i => i.FechaIngreso >= hoy && i.FechaIngreso < finDia)
                .ToListAsync();

            var totalIngresosDelDia = ingresosDelDia.Count;
            var valorIngresosDelDia = ingresosDelDia.Sum(i => i.ValorCobrado);

            // Mensualidades
            var mensualidadesActivas = await _context.Mensualidades
                .CountAsync(m => m.IsActive && 
                               m.FechaInicio <= DateTime.Now && 
                               m.FechaFin >= DateTime.Now);

            var mensualidadesProximasVencer = await _context.Mensualidades
                .CountAsync(m => m.IsActive && 
                               m.FechaFin >= DateTime.Now && 
                               m.FechaFin <= DateTime.Now.AddDays(3));

            var mensualidadesVencidas = await _context.Mensualidades
                .CountAsync(m => m.IsActive && m.FechaFin < DateTime.Now);

            // Ingresos por hora del día
            var ingresosPorHora = await GetIngresosPorHoraAsync(hoy);

            // Mensualidades próximas a vencer
            var mensualidadesProximas = await GetMensualidadesProximasVencerAsync(3);

            return new DashboardDTO
            {
                VehiculosActualmenteEnParqueadero = vehiculosEnParqueadero,
                IngresosDelDia = valorIngresosDelDia,
                TotalIngresosDelDia = totalIngresosDelDia,
                MensualidadesActivas = mensualidadesActivas,
                MensualidadesProximasVencer = mensualidadesProximasVencer,
                MensualidadesVencidas = mensualidadesVencidas,
                IngresosPorHora = ingresosPorHora,
                MensualidadesProximas = mensualidadesProximas
            };
        }

        public async Task<IngresoResumenDTO> GetResumenIngresosAsync()
        {
            var hoy = DateTime.Today;
            var finDia = hoy.AddDays(1);

            // Vehículos activos
            var vehiculosActivos = await _context.Ingresos
                .CountAsync(i => i.IsActive && i.FechaSalida == null);

            // Ingresos del día
            var ingresosHoy = await _context.Ingresos
                .Where(i => i.FechaIngreso >= hoy && i.FechaIngreso < finDia)
                .ToListAsync();

            var totalIngresosHoy = ingresosHoy.Count;
            var totalIngresosHoyValor = ingresosHoy.Sum(i => i.ValorCobrado);

            // Mensualidades
            var mensualidadesActivas = await _context.Mensualidades
                .CountAsync(m => m.IsActive && 
                               m.FechaInicio <= DateTime.Now && 
                               m.FechaFin >= DateTime.Now);

            var mensualidadesProximasVencer = await _context.Mensualidades
                .CountAsync(m => m.IsActive && 
                               m.FechaFin >= DateTime.Now && 
                               m.FechaFin <= DateTime.Now.AddDays(3));

            var mensualidadesVencidas = await _context.Mensualidades
                .CountAsync(m => m.IsActive && m.FechaFin < DateTime.Now);

            return new IngresoResumenDTO
            {
                TotalVehiculosActivos = vehiculosActivos,
                TotalIngresosHoy = totalIngresosHoy,
                TotalIngresosHoyValor = totalIngresosHoyValor,
                MensualidadesActivas = mensualidadesActivas,
                MensualidadesProximasVencer = mensualidadesProximasVencer,
                MensualidadesVencidas = mensualidadesVencidas
            };
        }

        public async Task<IEnumerable<IngresoDiarioDTO>> GetIngresosPorHoraAsync(DateTime fecha)
        {
            var inicioDia = fecha.Date;
            var finDia = inicioDia.AddDays(1);

            var ingresos = await _context.Ingresos
                .Where(i => i.FechaIngreso >= inicioDia && i.FechaIngreso < finDia)
                .ToListAsync();

            var ingresosPorHora = new List<IngresoDiarioDTO>();

            for (int hora = 0; hora < 24; hora++)
            {
                var inicioHora = inicioDia.AddHours(hora);
                var finHora = inicioHora.AddHours(1);

                var ingresosHora = ingresos
                    .Where(i => i.FechaIngreso >= inicioHora && i.FechaIngreso < finHora)
                    .ToList();

                ingresosPorHora.Add(new IngresoDiarioDTO
                {
                    Hora = hora,
                    CantidadIngresos = ingresosHora.Count,
                    ValorTotal = ingresosHora.Sum(i => i.ValorCobrado)
                });
            }

            return ingresosPorHora;
        }

        public async Task<IEnumerable<MensualidadVencimientoDTO>> GetMensualidadesProximasVencerAsync(int dias = 3)
        {
            var fechaLimite = DateTime.Now.AddDays(dias);

            var mensualidades = await _context.Mensualidades
                .Where(m => m.IsActive && 
                           m.FechaFin <= fechaLimite && 
                           m.FechaFin >= DateTime.Now)
                .OrderBy(m => m.FechaFin)
                .ToListAsync();

            return mensualidades.Select(m => new MensualidadVencimientoDTO
            {
                Id = m.Id,
                Nombre = m.Nombre,
                Placa = m.Placa,
                Email = m.Email,
                FechaFin = m.FechaFin,
                DiasRestantes = (m.FechaFin - DateTime.Now).Days
            });
        }
    }
}
