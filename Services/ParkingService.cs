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
            // Devolver TODOS los operadores (activos e inactivos)
            var operadores = await _context.Operadores
                .OrderByDescending(o => o.IsActive) // Activos primero
                .ThenBy(o => o.Nombre)
                .ToListAsync();

            return _mapper.Map<IEnumerable<OperadorDTO>>(operadores);
        }

        public async Task<OperadorDTO?> GetOperadorByIdAsync(int id)
        {
            // Buscar operador sin importar si está activo o no
            var operador = await _context.Operadores
                .FirstOrDefaultAsync(o => o.Id == id);

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
                .FirstOrDefaultAsync(o => o.Id == id);

            if (operador == null) return false;

            // Eliminar primero los turnos asociados
            var turnosAsociados = await _context.Turnos
                .Where(t => t.OperadorId == id)
                .ToListAsync();
            
            _context.Turnos.RemoveRange(turnosAsociados);

            // Eliminar definitivamente de la base de datos
            _context.Operadores.Remove(operador);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Operador eliminado definitivamente: {operador.Nombre} (ID: {operador.Id})");
            return true;
        }

        public async Task<OperadorDTO?> ToggleEstadoOperadorAsync(int id)
        {
            var operador = await _context.Operadores
                .FirstOrDefaultAsync(o => o.Id == id);

            if (operador == null) return null;

            // Si se está desactivando, verificar que no tenga turnos abiertos
            if (operador.IsActive)
            {
                var tieneTurnosAbiertos = await _context.Turnos
                    .AnyAsync(t => t.OperadorId == id && t.FechaCierre == null);

                if (tieneTurnosAbiertos)
                {
                    throw new InvalidOperationException("No se puede desactivar un operador que tiene turnos abiertos");
                }
            }

            // Cambiar el estado
            operador.IsActive = !operador.IsActive;
            operador.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Operador {(operador.IsActive ? "activado" : "desactivado")}: {operador.Nombre} (ID: {operador.Id})");
            
            return _mapper.Map<OperadorDTO>(operador);
        }

        #endregion

        #region Mensualidades

        public async Task<IEnumerable<MensualidadDTO>> GetMensualidadesAsync()
        {
            // Retornar TODAS las mensualidades (activas e inactivas) para gestión administrativa
            var mensualidades = await _context.Mensualidades
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
            
            // Calcular valor basado en días de vigencia (precio fijo por mes)
            var dias = (mensualidad.FechaFin - mensualidad.FechaInicio).Days;
            var meses = (decimal)Math.Ceiling(dias / 30.0);
            mensualidad.Valor = meses * 50000m; // $50,000 por mes (no se guarda en BD actual)
            
            _context.Mensualidades.Add(mensualidad);
            await _context.SaveChangesAsync();

            // Enviar email de confirmación solo si hay email
            if (!string.IsNullOrEmpty(mensualidad.Email))
            {
                await _emailService.SendMensualidadCreadaNotificationAsync(
                    mensualidad.Email, 
                    mensualidad.NombrePropietario, 
                    mensualidad.Placa, 
                    mensualidad.FechaInicio, 
                    mensualidad.FechaFin);
            }

            _logger.LogInformation($"Mensualidad creada: {mensualidad.NombrePropietario} - {mensualidad.Placa} (ID: {mensualidad.Id})");
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

            _logger.LogInformation($"Mensualidad actualizada: {mensualidad.NombrePropietario} - {mensualidad.Placa} (ID: {mensualidad.Id})");
            return _mapper.Map<MensualidadDTO>(mensualidad);
        }

        public async Task<bool> DeleteMensualidadAsync(int id)
        {
            var mensualidad = await _context.Mensualidades
                .FirstOrDefaultAsync(m => m.Id == id);

            if (mensualidad == null) return false;

            // Verificar si tiene ingresos asociados
            var tieneIngresos = await _context.Ingresos
                .AnyAsync(i => i.MensualidadId == id);

            if (tieneIngresos)
            {
                throw new InvalidOperationException("No se puede eliminar una mensualidad que tiene ingresos asociados");
            }

            // Eliminación permanente (hard delete)
            _context.Mensualidades.Remove(mensualidad);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Mensualidad eliminada permanentemente: {mensualidad.NombrePropietario} - {mensualidad.Placa} (ID: {mensualidad.Id})");
            return true;
        }

        public async Task<MensualidadDTO?> ToggleMensualidadEstadoAsync(int id)
        {
            var mensualidad = await _context.Mensualidades
                .FirstOrDefaultAsync(m => m.Id == id);

            if (mensualidad == null) return null;

            mensualidad.IsActive = !mensualidad.IsActive;
            mensualidad.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var estadoTexto = mensualidad.IsActive ? "activada" : "desactivada";
            _logger.LogInformation($"Mensualidad {estadoTexto}: {mensualidad.NombrePropietario} - {mensualidad.Placa} (ID: {mensualidad.Id})");

            return _mapper.Map<MensualidadDTO>(mensualidad);
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
            // Devolver TODAS las tarifas (activas e inactivas) como se hace con operadores
            var tarifas = await _context.Tarifas
                .OrderByDescending(t => t.IsActive) // Activas primero
                .ThenBy(t => t.Nombre)
                .ToListAsync();

            return _mapper.Map<IEnumerable<TarifaDTO>>(tarifas);
        }

        public async Task<TarifaDTO?> GetTarifaByIdAsync(int id)
        {
            // Buscar tarifa sin importar si está activa o no
            var tarifa = await _context.Tarifas
                .FirstOrDefaultAsync(t => t.Id == id);

            return tarifa != null ? _mapper.Map<TarifaDTO>(tarifa) : null;
        }

        public async Task<TarifaDTO> CreateTarifaAsync(CreateTarifaDTO tarifaDto)
        {
            var tarifa = _mapper.Map<Tarifa>(tarifaDto);
            
            try
            {
                _context.Tarifas.Add(tarifa);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Tarifa creada: {tarifa.Nombre} (ID: {tarifa.Id})");
            }
            catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("duplicate key") == true)
            {
                // Si hay error de clave duplicada, resetear la secuencia y reintentar
                _logger.LogWarning("Error de clave duplicada detectado. Reseteando secuencia...");
                
                await _context.Database.ExecuteSqlRawAsync(
                    "SELECT setval('tarifas_id_seq', (SELECT COALESCE(MAX(id), 0) FROM tarifas) + 1, false);"
                );
                
                // Limpiar el contexto y reintentar
                _context.Entry(tarifa).State = EntityState.Detached;
                tarifa = _mapper.Map<Tarifa>(tarifaDto);
                _context.Tarifas.Add(tarifa);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation($"Tarifa creada después de resetear secuencia: {tarifa.Nombre} (ID: {tarifa.Id})");
            }

            return _mapper.Map<TarifaDTO>(tarifa);
        }

        public async Task<TarifaDTO?> UpdateTarifaAsync(int id, UpdateTarifaDTO tarifaDto)
        {
            var tarifa = await _context.Tarifas
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tarifa == null) return null;

            _mapper.Map(tarifaDto, tarifa);
            tarifa.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Tarifa actualizada: {tarifa.Nombre} (ID: {tarifa.Id}) - Estado: {tarifa.IsActive}");
            return _mapper.Map<TarifaDTO>(tarifa);
        }

        public async Task<bool> DeleteTarifaAsync(int id)
        {
            var tarifa = await _context.Tarifas
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tarifa == null) return false;

            // Verificar si tiene ingresos asociados
            var tieneIngresos = await _context.Ingresos
                .AnyAsync(i => i.TarifaId == id);

            if (tieneIngresos)
            {
                throw new InvalidOperationException("No se puede eliminar una tarifa que tiene ingresos asociados. Puede desactivarla en su lugar.");
            }

            // ELIMINACIÓN PERMANENTE (Hard Delete)
            _context.Tarifas.Remove(tarifa);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Tarifa eliminada permanentemente: {tarifa.Nombre} (ID: {tarifa.Id})");
            return true;
        }

        public async Task<TarifaDTO?> ToggleTarifaEstadoAsync(int id)
        {
            var tarifa = await _context.Tarifas
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tarifa == null) return null;

            tarifa.IsActive = !tarifa.IsActive;
            tarifa.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Tarifa estado cambiado: {tarifa.Nombre} (ID: {tarifa.Id}) - Nuevo estado: {tarifa.IsActive}");
            return _mapper.Map<TarifaDTO>(tarifa);
        }

        public async Task<TarifaDTO?> GetTarifaActivaAsync()
        {
            var tarifa = await _context.Tarifas
                .FirstOrDefaultAsync(t => t.IsActive);

            return tarifa != null ? _mapper.Map<TarifaDTO>(tarifa) : null;
        }

        #endregion

        #region Ingresos

        public async Task<IEnumerable<IngresoDTO>> GetIngresosAsync()
        {
            // Obtener la tarifa activa para recálculo automático
            var tarifaActiva = await GetTarifaActivaAsync();
            
            var ingresos = await _context.Ingresos
                .Include(i => i.OperadorIngreso)
                .Include(i => i.Mensualidad)
                .Include(i => i.Tarifa)
                .Where(i => i.Estado != "Anulado") // Excluir tickets anulados
                .OrderByDescending(i => i.FechaIngreso)
                .ToListAsync();

            // RECÁLCULO AUTOMÁTICO: Corregir tickets con monto $0 al momento de cargar
            if (tarifaActiva != null)
            {
                var ticketsConMontoCero = ingresos
                    .Where(i => i.FechaSalida != null && 
                               i.MontoCobrado == 0 && 
                               i.TipoIngreso == "Invitado" &&
                               i.FechaIngreso != null)
                    .ToList();

                foreach (var ticket in ticketsConMontoCero)
                {
                    try
                    {
                        // Calcular tiempo de estadía si no está calculado
                        if (ticket.TiempoEstadiaMinutos == 0)
                        {
                            ticket.TiempoEstadiaMinutos = (int)(ticket.FechaSalida.Value - ticket.FechaIngreso).TotalMinutes;
                        }

                        // Recalcular el monto
                        var montoCalculado = await CalcularValorCobroAsync(ticket.Id, tarifaActiva.Id);
                        
                        if (montoCalculado > 0)
                        {
                            ticket.MontoCobrado = montoCalculado;
                            ticket.Estado = "Finalizado";
                            ticket.Pagado = false;
                            
                            _logger.LogInformation($"✅ Auto-recalculado: {ticket.NumeroFolio} = ${montoCalculado}");
                        }
                        else
                        {
                            // Anular si el monto sigue siendo 0 (tiempo de gracia)
                            ticket.Activo = false;
                            ticket.Estado = "Anulado";
                            
                            _logger.LogWarning($"⚠️ Auto-anulado (tiempo de gracia): {ticket.NumeroFolio}");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"❌ Error auto-calculando {ticket.NumeroFolio}");
                    }
                }

                // Guardar cambios si se hicieron recálculos
                if (ticketsConMontoCero.Any())
                {
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"✅ Auto-recalculados {ticketsConMontoCero.Count(t => t.MontoCobrado > 0)} tickets al cargar");
                }
            }

            // Filtrar tickets anulados después del recálculo
            var ingresosValidos = ingresos.Where(i => i.Estado != "Anulado").ToList();

            return _mapper.Map<IEnumerable<IngresoDTO>>(ingresosValidos);
        }

        public async Task<IngresoDTO?> GetIngresoByIdAsync(int id)
        {
            var ingreso = await _context.Ingresos
                .Include(i => i.OperadorIngreso)
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
            if (ingresoDto.TipoIngreso == "Mensualidad")
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
            if (ingreso.TipoIngreso == "Invitado")
            {
                var tarifaActiva = await GetTarifaActivaAsync();
                if (tarifaActiva != null)
                {
                    ingreso.MontoCobrado = await CalcularValorCobroAsync(ingreso.Id, tarifaActiva.Id);
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Ingreso finalizado: {ingreso.Placa} - Valor: ${ingreso.MontoCobrado} (ID: {ingreso.Id})");
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
            var valorFraccion = fraccionMinutos > 0 ? tarifa.ValorFraccion : 0;

            var valorTotal = valorBase + valorFraccion;

            // Aplicar tope diario
            return Math.Min(valorTotal, tarifa.TopeDiario ?? decimal.MaxValue);
        }

        public async Task<CorreccionTicketsResult> CorregirTicketsMalosAsync()
        {
            var result = new CorreccionTicketsResult();

            // 1. Encontrar tickets con monto $0 y fecha de salida (están finalizados pero sin cobro)
            var ticketsConMontoCero = await _context.Ingresos
                .Where(i => i.FechaSalida != null && 
                           i.MontoCobrado == 0 && 
                           i.TipoIngreso == "Invitado" &&
                           i.FechaIngreso != null)
                .ToListAsync();

            _logger.LogInformation($"Encontrados {ticketsConMontoCero.Count} tickets con monto $0");

            // 2. Intentar recalcular el monto de cada ticket
            var tarifaActiva = await GetTarifaActivaAsync();
            
            if (tarifaActiva != null)
            {
                foreach (var ticket in ticketsConMontoCero)
                {
                    try
                    {
                        // Calcular el tiempo de estadía si no está calculado
                        if (ticket.TiempoEstadiaMinutos == 0 && ticket.FechaSalida != null)
                        {
                            ticket.TiempoEstadiaMinutos = (int)(ticket.FechaSalida.Value - ticket.FechaIngreso).TotalMinutes;
                        }

                        // Recalcular el monto usando la tarifa activa
                        var montoCalculado = await CalcularValorCobroAsync(ticket.Id, tarifaActiva.Id);
                        
                        if (montoCalculado > 0)
                        {
                            ticket.MontoCobrado = montoCalculado;
                            ticket.Estado = "Finalizado";
                            ticket.Pagado = false; // Asegurar que no esté marcado como pagado
                            result.TicketsCorregidos++;
                            
                            _logger.LogInformation($"✅ Ticket {ticket.NumeroFolio} recalculado: ${montoCalculado}");
                        }
                        else
                        {
                            // Si el monto sigue siendo 0 (por tiempo de gracia), anular
                            ticket.Activo = false;
                            ticket.Estado = "Anulado";
                            result.TicketsEliminados++;
                            
                            _logger.LogWarning($"⚠️ Ticket {ticket.NumeroFolio} anulado (monto $0 después del cálculo)");
                        }
                    }
                    catch (Exception ex)
                    {
                        // Si falla el cálculo, anular el ticket
                        _logger.LogError(ex, $"❌ Error calculando ticket {ticket.NumeroFolio}");
                        ticket.Activo = false;
                        ticket.Estado = "Anulado";
                        result.TicketsEliminados++;
                    }
                }
            }
            else
            {
                // Si no hay tarifa activa, anular todos los tickets con monto $0
                _logger.LogWarning("⚠️ No hay tarifa activa, anulando todos los tickets con monto $0");
                foreach (var ticket in ticketsConMontoCero)
                {
                    ticket.Activo = false;
                    ticket.Estado = "Anulado";
                    result.TicketsEliminados++;
                }
            }

            // 3. Desmarcar como pagados los tickets con monto $0 que no se pudieron recalcular
            var ticketsPagadosInvalidos = await _context.Ingresos
                .Where(i => i.Pagado && (i.MontoCobrado == 0 || i.Estado == "Anulado"))
                .ToListAsync();
            
            foreach (var ticket in ticketsPagadosInvalidos)
            {
                ticket.Pagado = false;
                result.TicketsPagadosMarcados++;
            }

            // 4. Actualizar estado de tickets finalizados sin pagar (que ya tienen monto)
            var ticketsFinalizadosSinPagar = await _context.Ingresos
                .Where(i => i.FechaSalida != null && 
                           !i.Pagado && 
                           i.MontoCobrado > 0 && 
                           i.Estado != "Finalizado" &&
                           i.Estado != "Anulado")
                .ToListAsync();

            foreach (var ticket in ticketsFinalizadosSinPagar)
            {
                ticket.Estado = "Finalizado";
                result.TicketsCorregidos++;
            }

            await _context.SaveChangesAsync();

            result.Mensaje = $"Recalculados y corregidos: {result.TicketsCorregidos} tickets. " +
                           $"Anulados (sin datos válidos): {result.TicketsEliminados} tickets. " +
                           $"Desmarcados como pagados: {result.TicketsPagadosMarcados} tickets.";

            _logger.LogInformation(result.Mensaje);

            return result;
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
                .FirstOrDefaultAsync(t => t.Id == cerrarDto.TurnoId && t.IsActive);

            if (turno == null) return null;

            turno.FechaCierre = DateTime.UtcNow;
            turno.IsActive = false;

            // Calcular totales del turno
            var ingresosTurno = await _context.Ingresos
                .Where(i => i.OperadorIngresoId == turno.OperadorId && 
                           i.FechaIngreso >= turno.FechaApertura && 
                           i.FechaIngreso <= turno.FechaCierre &&
                           i.Estado == "Finalizado")
                .ToListAsync();

            turno.TotalCobros = ingresosTurno.Sum(i => i.MontoCobrado);
            turno.TotalIngresos = ingresosTurno.Count();

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Turno cerrado: Operador {turno.Operador.Nombre} - Total: ${turno.TotalIngresos} (ID: {turno.Id})");
            return _mapper.Map<TurnoDTO>(turno);
        }

        public async Task<TurnoDTO?> ToggleEstadoTurnoAsync(int turnoId)
        {
            var turno = await _context.Turnos
                .Include(t => t.Operador)
                .FirstOrDefaultAsync(t => t.Id == turnoId);

            if (turno == null) return null;

            // Cambiar el estado activo/inactivo
            turno.IsActive = !turno.IsActive;
            turno.UpdatedAt = DateTime.UtcNow;

            // Si se está reactivando, limpiar la fecha de cierre
            if (turno.IsActive)
            {
                turno.FechaCierre = null;
                _logger.LogInformation($"Turno reactivado: ID {turno.Id} - Operador {turno.Operador.Nombre}");
            }
            else
            {
                // Si se está desactivando, establecer fecha de cierre si no existe
                if (turno.FechaCierre == null)
                {
                    turno.FechaCierre = DateTime.UtcNow;
                }
                _logger.LogInformation($"Turno desactivado: ID {turno.Id} - Operador {turno.Operador.Nombre}");
            }

            await _context.SaveChangesAsync();

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
