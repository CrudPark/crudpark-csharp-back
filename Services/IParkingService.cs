using crud_park_back.DTOs;
using crud_park_back.Models;

namespace crud_park_back.Services
{
    public class CorreccionTicketsResult
    {
        public int TicketsCorregidos { get; set; }
        public int TicketsEliminados { get; set; }
        public int TicketsPagadosMarcados { get; set; }
        public string Mensaje { get; set; } = string.Empty;
    }

    public interface IParkingService
    {
        // Operadores
        Task<IEnumerable<OperadorDTO>> GetOperadoresAsync();
        Task<OperadorDTO?> GetOperadorByIdAsync(int id);
        Task<OperadorDTO> CreateOperadorAsync(CreateOperadorDTO operadorDto);
        Task<OperadorDTO?> UpdateOperadorAsync(int id, UpdateOperadorDTO operadorDto);
        Task<bool> DeleteOperadorAsync(int id);
        Task<OperadorDTO?> ToggleEstadoOperadorAsync(int id);

        // Mensualidades
        Task<IEnumerable<MensualidadDTO>> GetMensualidadesAsync();
        Task<MensualidadDTO?> GetMensualidadByIdAsync(int id);
        Task<MensualidadDTO> CreateMensualidadAsync(CreateMensualidadDTO mensualidadDto);
        Task<MensualidadDTO?> UpdateMensualidadAsync(int id, UpdateMensualidadDTO mensualidadDto);
        Task<bool> DeleteMensualidadAsync(int id);
        Task<MensualidadDTO?> ToggleMensualidadEstadoAsync(int id);
        Task<bool> ExisteMensualidadVigenteAsync(string placa);
        Task<IEnumerable<MensualidadDTO>> GetMensualidadesProximasVencerAsync(int dias = 3);

        // Tarifas
        Task<IEnumerable<TarifaDTO>> GetTarifasAsync();
        Task<TarifaDTO?> GetTarifaByIdAsync(int id);
        Task<TarifaDTO> CreateTarifaAsync(CreateTarifaDTO tarifaDto);
        Task<TarifaDTO?> UpdateTarifaAsync(int id, UpdateTarifaDTO tarifaDto);
        Task<bool> DeleteTarifaAsync(int id);
        Task<TarifaDTO?> ToggleTarifaEstadoAsync(int id);
        Task<TarifaDTO?> GetTarifaActivaAsync();

        // Ingresos
        Task<IEnumerable<IngresoDTO>> GetIngresosAsync();
        Task<IngresoDTO?> GetIngresoByIdAsync(int id);
        Task<IngresoDTO> CreateIngresoAsync(CreateIngresoDTO ingresoDto);
        Task<IngresoDTO?> FinalizarIngresoAsync(FinalizarIngresoDTO finalizarDto);
        Task<IEnumerable<IngresoDTO>> GetIngresosActivosAsync();
        Task<IngresoDTO?> GetIngresoActivoPorPlacaAsync(string placa);
        Task<decimal> CalcularValorCobroAsync(int ingresoId, int tarifaId);
        Task<CorreccionTicketsResult> CorregirTicketsMalosAsync();

        // Turnos
        Task<IEnumerable<TurnoDTO>> GetTurnosAsync();
        Task<TurnoDTO?> GetTurnoByIdAsync(int id);
        Task<TurnoDTO> CrearTurnoAsync(CreateTurnoDTO turnoDto);
        Task<TurnoDTO?> CerrarTurnoAsync(CerrarTurnoDTO cerrarDto);
        Task<TurnoDTO?> ToggleEstadoTurnoAsync(int turnoId);
        Task<TurnoDTO?> GetTurnoAbiertoPorOperadorAsync(int operadorId);
    }
}
