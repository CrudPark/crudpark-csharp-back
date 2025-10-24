using AutoMapper;
using crud_park_back.DTOs;
using crud_park_back.Models;
using Microsoft.EntityFrameworkCore;
using CsvHelper;
using OfficeOpenXml;
using System.Globalization;
using System.Text;

namespace crud_park_back.Services
{
    public class ReporteService : IReporteService
    {
        private readonly ParkingDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<ReporteService> _logger;

        public ReporteService(ParkingDbContext context, IMapper mapper, ILogger<ReporteService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ReporteIngresosDTO> GenerarReporteIngresosAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            var ingresos = await _context.Ingresos
                .Include(i => i.OperadorIngreso)
                .Include(i => i.Mensualidad)
                .Include(i => i.Tarifa)
                .Where(i => i.FechaIngreso >= fechaInicio && i.FechaIngreso <= fechaFin)
                .ToListAsync();

            var totalIngresos = ingresos.Sum(i => i.MontoCobrado);
            var totalVehiculos = ingresos.Count;
            var promedioOcupacion = await GetPromedioOcupacionAsync(fechaInicio, fechaFin);

            var mensualidadesVsInvitados = ingresos
                .GroupBy(i => i.TipoIngreso)
                .ToDictionary(g => g.Key, g => g.Count());

            var detalleIngresos = await GetIngresosDiariosAsync(fechaInicio, fechaFin);

            return new ReporteIngresosDTO
            {
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                TotalIngresos = totalIngresos,
                TotalVehiculos = totalVehiculos,
                PromedioOcupacion = promedioOcupacion,
                MensualidadesVsInvitados = mensualidadesVsInvitados.GetValueOrDefault("Mensualidad", 0),
                DetalleIngresos = detalleIngresos.ToList()
            };
        }

        public async Task<byte[]> ExportarReporteCSVAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            var ingresos = await _context.Ingresos
                .Include(i => i.OperadorIngreso)
                .Include(i => i.Mensualidad)
                .Include(i => i.Tarifa)
                .Where(i => i.FechaIngreso >= fechaInicio && i.FechaIngreso <= fechaFin)
                .OrderBy(i => i.FechaIngreso)
                .ToListAsync();

            using var memoryStream = new MemoryStream();
            using var writer = new StreamWriter(memoryStream, Encoding.UTF8);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

            // Escribir encabezados
            csv.WriteField("ID");
            csv.WriteField("Placa");
            csv.WriteField("Fecha Ingreso");
            csv.WriteField("Fecha Salida");
            csv.WriteField("Tipo Ingreso");
            csv.WriteField("Estado");
            csv.WriteField("Valor Cobrado");
            csv.WriteField("Tiempo Estadia (min)");
            csv.WriteField("Operador");
            csv.WriteField("Mensualidad");
            csv.WriteField("Tarifa");
            csv.NextRecord();

            // Escribir datos
            foreach (var ingreso in ingresos)
            {
                csv.WriteField(ingreso.Id);
                csv.WriteField(ingreso.Placa);
                csv.WriteField(ingreso.FechaIngreso.ToString("yyyy-MM-dd HH:mm:ss"));
                csv.WriteField(ingreso.FechaSalida?.ToString("yyyy-MM-dd HH:mm:ss") ?? "");
                csv.WriteField(ingreso.TipoIngreso.ToString());
                csv.WriteField(ingreso.Estado.ToString());
                csv.WriteField(ingreso.MontoCobrado.ToString("F2"));
                csv.WriteField(ingreso.TiempoEstadiaMinutos?.ToString() ?? "");
                csv.WriteField(ingreso.OperadorIngreso?.Nombre ?? "");
                csv.WriteField(ingreso.Mensualidad?.NombrePropietario ?? "");
                csv.WriteField(ingreso.Tarifa?.Nombre ?? "");
                csv.NextRecord();
            }

            writer.Flush();
            return memoryStream.ToArray();
        }

        public async Task<byte[]> ExportarReporteExcelAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var ingresos = await _context.Ingresos
                .Include(i => i.OperadorIngreso)
                .Include(i => i.Mensualidad)
                .Include(i => i.Tarifa)
                .Where(i => i.FechaIngreso >= fechaInicio && i.FechaIngreso <= fechaFin)
                .OrderBy(i => i.FechaIngreso)
                .ToListAsync();

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Reporte Ingresos");

            // Encabezados
            worksheet.Cells[1, 1].Value = "ID";
            worksheet.Cells[1, 2].Value = "Placa";
            worksheet.Cells[1, 3].Value = "Fecha Ingreso";
            worksheet.Cells[1, 4].Value = "Fecha Salida";
            worksheet.Cells[1, 5].Value = "Tipo Ingreso";
            worksheet.Cells[1, 6].Value = "Estado";
            worksheet.Cells[1, 7].Value = "Valor Cobrado";
            worksheet.Cells[1, 8].Value = "Tiempo Estadia (min)";
            worksheet.Cells[1, 9].Value = "Operador";
            worksheet.Cells[1, 10].Value = "Mensualidad";
            worksheet.Cells[1, 11].Value = "Tarifa";

            // Formatear encabezados
            using (var range = worksheet.Cells[1, 1, 1, 11])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
            }

            // Datos
            for (int i = 0; i < ingresos.Count; i++)
            {
                var ingreso = ingresos[i];
                var row = i + 2;

                worksheet.Cells[row, 1].Value = ingreso.Id;
                worksheet.Cells[row, 2].Value = ingreso.Placa;
                worksheet.Cells[row, 3].Value = ingreso.FechaIngreso;
                worksheet.Cells[row, 4].Value = ingreso.FechaSalida;
                worksheet.Cells[row, 5].Value = ingreso.TipoIngreso.ToString();
                worksheet.Cells[row, 6].Value = ingreso.Estado.ToString();
                worksheet.Cells[row, 7].Value = ingreso.MontoCobrado;
                worksheet.Cells[row, 8].Value = ingreso.TiempoEstadiaMinutos;
                worksheet.Cells[row, 9].Value = ingreso.OperadorIngreso?.Nombre ?? "";
                worksheet.Cells[row, 10].Value = ingreso.Mensualidad?.NombrePropietario ?? "";
                worksheet.Cells[row, 11].Value = ingreso.Tarifa?.Nombre ?? "";
            }

            // Autoajustar columnas
            worksheet.Cells.AutoFitColumns();

            return package.GetAsByteArray();
        }

        public async Task<IEnumerable<IngresoDetalleDTO>> GetIngresosDiariosAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            var ingresos = await _context.Ingresos
                .Where(i => i.FechaIngreso >= fechaInicio && i.FechaIngreso <= fechaFin)
                .ToListAsync();

            var ingresosPorDia = ingresos
                .GroupBy(i => i.FechaIngreso.Date)
                .Select(g => new IngresoDetalleDTO
                {
                    Fecha = g.Key,
                    CantidadIngresos = g.Count(),
                    ValorTotal = g.Sum(i => i.MontoCobrado),
                    Mensualidades = g.Count(i => i.TipoIngreso == "Mensualidad"),
                    Invitados = g.Count(i => i.TipoIngreso == "Invitado")
                })
                .OrderBy(x => x.Fecha)
                .ToList();

            return ingresosPorDia;
        }

        public async Task<decimal> GetPromedioOcupacionAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            // Calcular el promedio de ocupación basado en los ingresos diarios
            var ingresosDiarios = await GetIngresosDiariosAsync(fechaInicio, fechaFin);
            
            if (!ingresosDiarios.Any())
                return 0;

            // Asumir una capacidad máxima del parqueadero (esto debería ser configurable)
            const int capacidadMaxima = 100; // Ejemplo: 100 espacios
            
            var promedioOcupacion = ingresosDiarios
                .Average(d => (decimal)d.CantidadIngresos / capacidadMaxima * 100);

            return Math.Round(promedioOcupacion, 2);
        }
    }
}
