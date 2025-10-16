using AutoMapper;
using crud_park_back.DTOs;
using crud_park_back.Models;
using Microsoft.EntityFrameworkCore;

namespace crud_park_back.Services
{
    public class ParkingService : IParkingService
    {
        private readonly ParkingDbContext _context;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;
        private readonly ILogger<ParkingService> _logger;

        public ParkingService(
            ParkingDbContext context, 
            IMapper mapper, 
            IEmailService emailService,
            ILogger<ParkingService> logger)
        {
            _context = context;
            _mapper = mapper;
            _emailService = emailService;
            _logger = logger;
        }

        #region Operadores

        public async Task<IEnumerable<OperadorDTO>> GetOperadoresAsync()
        {
            var operadores = await _context.Operadores
                .Where(o => o.IsActive)
                .OrderBy(o => o.Nombre)
                .ToListAsync();

            return _mapper.Map<IEnumerable<OperadorDTO>>(operadores);
        }

        public async Task<OperadorDTO?> GetOperadorByIdAsync(int id)
        {
            var operador = await _context.Operadores
                .FirstOrDefaultAsync(o => o.Id == id && o.IsActive);

            return operador != null ? _mapper.Map<OperadorDTO>(operador) : null;
        }

        public async Task<OperadorDTO> CreateOperadorAsync(CreateOperadorDTO operadorDto)
        {
            var operador = _mapper.Map<Operador>(operadorDto);
            _context.Operadores.Add(operador);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Operador creado: {operador.Nombre} (ID: {operador.Id})");
            return _mapper.Map<OperadorDTO>(operador);
        }

        public async Task<OperadorDTO?> UpdateOperadorAsync(int id, UpdateOperadorDTO operadorDto)
        {
            var operador = await _context.Operadores
                .FirstOrDefaultAsync(o => o.Id == id && o.IsActive);

            if (operador == null) return null;

            _mapper.Map(operadorDto, operador);
            operador.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Operador actualizado: {operador.Nombre} (ID: {operador.Id})");
            return _mapper.Map<OperadorDTO>(operador);
        }

        public async Task<bool> DeleteOperadorAsync(int id)
        {
            var operador = await _context.Operadores
                .FirstOrDefaultAsync(o => o.Id == id && o.IsActive);

            if (operador == null) return false;

            // Verificar si tiene turnos abiertos
            var tieneTurnosAbiertos = await _context.Turnos
                .AnyAsync(t => t.OperadorId == id && t.FechaCierre == null);

            if (tieneTurnosAbiertos)
            {
                throw new InvalidOperationException("No se puede eliminar un operador que tiene turnos abiertos");
            }

            operador.IsActive = false;
            operador.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Operador eliminado: {operador.Nombre} (ID: {operador.Id})");
            return true;
        }

        #endregion

        #region Mensualidades

        public async Task<IEnumerable<MensualidadDTO>> GetMensualidadesAsync()
        {
            var mensualidades = await _context.Mensualidades
                .Where(m => m.IsActive)
                .OrderBy(m => m.FechaFin)
                .ToListAsync();

            return _mapper.Map<IEnumerable<MensualidadDTO>>(mensualidades);
        }

        public async Task<MensualidadDTO?> GetMensualidadByIdAsync(int id)
        {
            var mensualidad = await _context.Mensualidades
                .FirstOrDefaultAsync(m => m.Id == id && m.IsActive);

            return mensualidad != null ? _mapper.Map<MensualidadDTO>(mensualidad) : null;
        }

        public async Task<MensualidadDTO> CreateMensualidadAsync(CreateMensualidadDTO mensualidadDto)
        {
            // Verificar que no exista una mensualidad vigente para la placa
            var mensualidadVigente = await ExisteMensualidadVigenteAsync(mensualidadDto.Placa);
            if (mensualidadVigente)
            {
                throw new InvalidOperationException($"Ya existe una mensualidad vigente para la placa {mensualidadDto.Placa}");
            }

            // Validar fechas
            if (mensualidadDto.FechaInicio >= mensualidadDto.FechaFin)
            {
                throw new InvalidOperationException("La fecha de inicio debe ser anterior a la fecha de fin");
            }

            var mensualidad = _mapper.Map<Mensualidad>(mensualidadDto);
            _context.Mensualidades.Add(mensualidad);
            await _context.SaveChangesAsync();

            // Enviar email de confirmación
            await _emailService.SendMensualidadCreadaNotificationAsync(
                mensualidad.Email, 
                mensualidad.Nombre, 
                mensualidad.Placa, 
                mensualidad.FechaInicio, 
                mensualidad.FechaFin);

            _logger.LogInformation($"Mensualidad creada: {mensualidad.Nombre} - {mensualidad.Placa} (ID: {mensualidad.Id})");
            return _mapper.Map<MensualidadDTO>(mensualidad);
        }

        public async Task<MensualidadDTO?> UpdateMensualidadAsync(int id, UpdateMensualidadDTO mensualidadDto)
        {
            var mensualidad = await _context.Mensualidades
                .FirstOrDefaultAsync(m => m.Id == id && m.IsActive);

            if (mensualidad == null) return null;

            // Verificar que no exista otra mensualidad vigente para la placa
            var mensualidadVigente = await _context.Mensualidades
                .AnyAsync(m => m.Placa == mensualidadDto.Placa && 
                              m.Id != id && 
                              m.IsActive && 
                              m.FechaInicio <= DateTime.Now && 
                              m.FechaFin >= DateTime.Now);

            if (mensualidadVigente)
            {
                throw new InvalidOperationException($"Ya existe una mensualidad vigente para la placa {mensualidadDto.Placa}");
            }

            // Validar fechas
            if (mensualidadDto.FechaInicio >= mensualidadDto.FechaFin)
            {
                throw new InvalidOperationException("La fecha de inicio debe ser anterior a la fecha de fin");
            }

            _mapper.Map(mensualidadDto, mensualidad);
            mensualidad.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Mensualidad actualizada: {mensualidad.Nombre} - {mensualidad.Placa} (ID: {mensualidad.Id})");
            return _mapper.Map<MensualidadDTO>(mensualidad);
        }

        public async Task<bool> DeleteMensualidadAsync(int id)
        {
            var mensualidad = await _context.Mensualidades
                .FirstOrDefaultAsync(m => m.Id == id && m.IsActive);

            if (mensualidad == null) return false;

            // Verificar si tiene ingresos asociados
            var tieneIngresos = await _context.Ingresos
                .AnyAsync(i => i.MensualidadId == id);

            if (tieneIngresos)
            {
                throw new InvalidOperationException("No se puede eliminar una mensualidad que tiene ingresos asociados");
            }

            mensualidad.IsActive = false;
            mensualidad.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Mensualidad eliminada: {mensualidad.Nombre} - {mensualidad.Placa} (ID: {mensualidad.Id})");
            return true;
        }

        public async Task<bool> ExisteMensualidadVigenteAsync(string placa)
        {
            return await _context.Mensualidades
                .AnyAsync(m => m.Placa == placa && 
                              m.IsActive && 
                              m.FechaInicio <= DateTime.Now && 
                              m.FechaFin >= DateTime.Now);
        }

        public async Task<IEnumerable<MensualidadDTO>> GetMensualidadesProximasVencerAsync(int dias = 3)
        {
            var fechaLimite = DateTime.Now.AddDays(dias);
            
            var mensualidades = await _context.Mensualidades
                .Where(m => m.IsActive && 
                           m.FechaFin <= fechaLimite && 
                           m.FechaFin >= DateTime.Now)
                .OrderBy(m => m.FechaFin)
                .ToListAsync();

            return _mapper.Map<IEnumerable<MensualidadDTO>>(mensualidades);
        }

        #endregion

        #region Tarifas

        public async Task<IEnumerable<TarifaDTO>> GetTarifasAsync()
        {
            var tarifas = await _context.Tarifas
                .Where(t => t.IsActive)
                .OrderBy(t => t.Nombre)
                .ToListAsync();

            return _mapper.Map<IEnumerable<TarifaDTO>>(tarifas);
        }

        public async Task<TarifaDTO?> GetTarifaByIdAsync(int id)
        {
            var tarifa = await _context.Tarifas
                .FirstOrDefaultAsync(t => t.Id == id && t.IsActive);

            return tarifa != null ? _mapper.Map<TarifaDTO>(tarifa) : null;
        }

        public async Task<TarifaDTO> CreateTarifaAsync(CreateTarifaDTO tarifaDto)
        {
            var tarifa = _mapper.Map<Tarifa>(tarifaDto);
            _context.Tarifas.Add(tarifa);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Tarifa creada: {tarifa.Nombre} (ID: {tarifa.Id})");
            return _mapper.Map<TarifaDTO>(tarifa);
        }

        public async Task<TarifaDTO?> UpdateTarifaAsync(int id, UpdateTarifaDTO tarifaDto)
        {
            var tarifa = await _context.Tarifas
                .FirstOrDefaultAsync(t => t.Id == id && t.IsActive);

            if (tarifa == null) return null;

            _mapper.Map(tarifaDto, tarifa);
            tarifa.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Tarifa actualizada: {tarifa.Nombre} (ID: {tarifa.Id})");
            return _mapper.Map<TarifaDTO>(tarifa);
        }

        public async Task<bool> DeleteTarifaAsync(int id)
        {
            var tarifa = await _context.Tarifas
                .FirstOrDefaultAsync(t => t.Id == id && t.IsActive);

            if (tarifa == null) return false;

            // Verificar si tiene ingresos asociados
            var tieneIngresos = await _context.Ingresos
                .AnyAsync(i => i.TarifaId == id);

            if (tieneIngresos)
            {
                throw new InvalidOperationException("No se puede eliminar una tarifa que tiene ingresos asociados");
            }

            tarifa.IsActive = false;
            tarifa.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Tarifa eliminada: {tarifa.Nombre} (ID: {tarifa.Id})");
            return true;
        }

        public async Task<TarifaDTO?> GetTarifaActivaAsync()
        {
            var tarifa = await _context.Tarifas
                .FirstOrDefaultAsync(t => t.EsActiva && t.IsActive);

            return tarifa != null ? _mapper.Map<TarifaDTO>(tarifa) : null;
        }

        #endregion

        #region Ingresos

        public async Task<IEnumerable<IngresoDTO>> GetIngresosAsync()
        {
            var ingresos = await _context.Ingresos
                .Include(i => i.Operador)
                .Include(i => i.Mensualidad)
                .Include(i => i.Tarifa)
                .OrderByDescending(i => i.FechaIngreso)
                .ToListAsync();

            return _mapper.Map<IEnumerable<IngresoDTO>>(ingresos);
        }

        public async Task<IngresoDTO?> GetIngresoByIdAsync(int id)
        {
            var ingreso = await _context.Ingresos
                .Include(i => i.Operador)
                .Include(i => i.Mensualidad)
                .Include(i => i.Tarifa)
                .FirstOrDefaultAsync(i => i.Id == id);

            return ingreso != null ? _mapper.Map<IngresoDTO>(ingreso) : null;
        }

        public async Task<IngresoDTO> CreateIngresoAsync(CreateIngresoDTO ingresoDto)
        {
            // Verificar que no exista un ingreso activo para la placa
            var ingresoActivo = await GetIngresoActivoPorPlacaAsync(ingresoDto.Placa);
            if (ingresoActivo != null)
            {
                throw new InvalidOperationException($"Ya existe un ingreso activo para la placa {ingresoDto.Placa}");
            }

            // Verificar que el operador existe
            var operador = await _context.Operadores
                .FirstOrDefaultAsync(o => o.Id == ingresoDto.OperadorIngresoId && o.IsActive);

            if (operador == null)
            {
                throw new InvalidOperationException("El operador especificado no existe o está inactivo");
            }

            var ingreso = _mapper.Map<Ingreso>(ingresoDto);

            // Generar número de folio único
            ingreso.NumeroFolio = await GenerarNumeroFolioAsync();

            // Si es mensualidad, verificar que existe y está vigente
            if (ingresoDto.TipoIngreso == TipoIngreso.Mensualidad)
            {
                var mensualidadVigente = await ExisteMensualidadVigenteAsync(ingresoDto.Placa);
                if (!mensualidadVigente)
                {
                    throw new InvalidOperationException("No existe una mensualidad vigente para esta placa");
                }
            }

            _context.Ingresos.Add(ingreso);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Ingreso creado: {ingreso.Placa} - {ingreso.TipoIngreso} - Folio: {ingreso.NumeroFolio} (ID: {ingreso.Id})");
            return _mapper.Map<IngresoDTO>(ingreso);
        }

        public async Task<IngresoDTO?> FinalizarIngresoAsync(FinalizarIngresoDTO finalizarDto)
        {
            var ingreso = await _context.Ingresos
                .Include(i => i.OperadorIngreso)
                .Include(i => i.OperadorSalida)
                .FirstOrDefaultAsync(i => i.Id == finalizarDto.IngresoId && i.IsActive && i.FechaSalida == null);

            if (ingreso == null) return null;

            // Verificar que el operador existe
            var operador = await _context.Operadores
                .FirstOrDefaultAsync(o => o.Id == finalizarDto.OperadorSalidaId && o.IsActive);

            if (operador == null)
            {
                throw new InvalidOperationException("El operador especificado no existe o está inactivo");
            }

            ingreso.FechaSalida = DateTime.UtcNow;
            ingreso.OperadorSalidaId = finalizarDto.OperadorSalidaId;
            ingreso.TiempoEstadiaMinutos = (int)(ingreso.FechaSalida.Value - ingreso.FechaIngreso).TotalMinutes;

            // Calcular valor cobrado solo para invitados
            if (ingreso.TipoIngreso == TipoIngreso.Invitado)
            {
                var tarifaActiva = await GetTarifaActivaAsync();
                if (tarifaActiva != null)
                {
                    ingreso.ValorCobrado = await CalcularValorCobroAsync(ingreso.Id, tarifaActiva.Id);
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Ingreso finalizado: {ingreso.Placa} - Valor: ${ingreso.ValorCobrado} (ID: {ingreso.Id})");
            return _mapper.Map<IngresoDTO>(ingreso);
        }

        public async Task<IEnumerable<IngresoDTO>> GetIngresosActivosAsync()
        {
            var ingresos = await _context.Ingresos
                .Include(i => i.OperadorIngreso)
                .Include(i => i.OperadorSalida)
                .Where(i => i.IsActive && i.FechaSalida == null)
                .OrderBy(i => i.FechaIngreso)
                .ToListAsync();

            return _mapper.Map<IEnumerable<IngresoDTO>>(ingresos);
        }

        public async Task<IngresoDTO?> GetIngresoActivoPorPlacaAsync(string placa)
        {
            var ingreso = await _context.Ingresos
                .Include(i => i.OperadorIngreso)
                .Include(i => i.OperadorSalida)
                .FirstOrDefaultAsync(i => i.Placa == placa && i.IsActive && i.FechaSalida == null);

            return ingreso != null ? _mapper.Map<IngresoDTO>(ingreso) : null;
        }

        public async Task<decimal> CalcularValorCobroAsync(int ingresoId, int tarifaId)
        {
            var ingreso = await _context.Ingresos
                .FirstOrDefaultAsync(i => i.Id == ingresoId);

            var tarifa = await _context.Tarifas
                .FirstOrDefaultAsync(t => t.Id == tarifaId);

            if (ingreso == null || tarifa == null || ingreso.FechaSalida == null)
            {
                return 0;
            }

            var tiempoEstadia = (int)(ingreso.FechaSalida.Value - ingreso.FechaIngreso).TotalMinutes;

            // Aplicar tiempo de gracia
            if (tiempoEstadia <= tarifa.TiempoGraciaMinutos)
            {
                return 0;
            }

            // Calcular horas completas y fracción
            var tiempoCobrable = tiempoEstadia - tarifa.TiempoGraciaMinutos;
            var horasCompletas = tiempoCobrable / 60;
            var fraccionMinutos = tiempoCobrable % 60;

            var valorBase = horasCompletas * tarifa.ValorBaseHora;
            var valorFraccion = fraccionMinutos > 0 ? tarifa.ValorAdicionalFraccion : 0;

            var valorTotal = valorBase + valorFraccion;

            // Aplicar tope diario
            return Math.Min(valorTotal, tarifa.TopeDiario);
        }

        #endregion

        #region Turnos

        public async Task<IEnumerable<TurnoDTO>> GetTurnosAsync()
        {
            var turnos = await _context.Turnos
                .Include(t => t.Operador)
                .OrderByDescending(t => t.FechaApertura)
                .ToListAsync();

            return _mapper.Map<IEnumerable<TurnoDTO>>(turnos);
        }

        public async Task<TurnoDTO?> GetTurnoByIdAsync(int id)
        {
            var turno = await _context.Turnos
                .Include(t => t.Operador)
                .FirstOrDefaultAsync(t => t.Id == id);

            return turno != null ? _mapper.Map<TurnoDTO>(turno) : null;
        }

        public async Task<TurnoDTO> CrearTurnoAsync(CreateTurnoDTO turnoDto)
        {
            // Verificar que el operador existe
            var operador = await _context.Operadores
                .FirstOrDefaultAsync(o => o.Id == turnoDto.OperadorId && o.IsActive);

            if (operador == null)
            {
                throw new InvalidOperationException("El operador especificado no existe o está inactivo");
            }

            // Verificar que no tenga un turno abierto
            var turnoAbierto = await GetTurnoAbiertoPorOperadorAsync(turnoDto.OperadorId);
            if (turnoAbierto != null)
            {
                throw new InvalidOperationException("El operador ya tiene un turno abierto");
            }

            var turno = _mapper.Map<Turno>(turnoDto);
            _context.Turnos.Add(turno);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Turno creado: Operador {operador.Nombre} (ID: {turno.Id})");
            return _mapper.Map<TurnoDTO>(turno);
        }

        public async Task<TurnoDTO?> CerrarTurnoAsync(CerrarTurnoDTO cerrarDto)
        {
            var turno = await _context.Turnos
                .Include(t => t.Operador)
                .FirstOrDefaultAsync(t => t.Id == cerrarDto.TurnoId && t.Estado == EstadoTurno.Abierto);

            if (turno == null) return null;

            turno.FechaCierre = DateTime.UtcNow;
            turno.Estado = EstadoTurno.Cerrado;

            // Calcular totales del turno
            var ingresosTurno = await _context.Ingresos
                .Where(i => i.OperadorId == turno.OperadorId && 
                           i.FechaIngreso >= turno.FechaApertura && 
                           i.FechaIngreso <= turno.FechaCierre &&
                           i.Estado == EstadoIngreso.Finalizado)
                .ToListAsync();

            turno.TotalIngresos = ingresosTurno.Sum(i => i.ValorCobrado);
            turno.TotalVehiculos = ingresosTurno.Count;

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Turno cerrado: Operador {turno.Operador.Nombre} - Total: ${turno.TotalIngresos} (ID: {turno.Id})");
            return _mapper.Map<TurnoDTO>(turno);
        }

        public async Task<TurnoDTO?> GetTurnoAbiertoPorOperadorAsync(int operadorId)
        {
            var turno = await _context.Turnos
                .Include(t => t.Operador)
                .FirstOrDefaultAsync(t => t.OperadorId == operadorId && t.FechaCierre == null);

            return turno != null ? _mapper.Map<TurnoDTO>(turno) : null;
        }

        #endregion

        #region Métodos Auxiliares

        private async Task<string> GenerarNumeroFolioAsync()
        {
            // Usar la función PostgreSQL para generar folio único
            var folio = await _context.Database.SqlQueryRaw<string>("SELECT generar_folio()").FirstOrDefaultAsync();
            return folio ?? $"T{DateTime.Now:yyyyMMddHHmmss}";
        }

        #endregion
    }
}
