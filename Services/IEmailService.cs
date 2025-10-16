namespace crud_park_back.Services
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true);
        Task<bool> SendMensualidadVencimientoNotificationAsync(string to, string nombre, string placa, DateTime fechaVencimiento);
        Task<bool> SendMensualidadCreadaNotificationAsync(string to, string nombre, string placa, DateTime fechaInicio, DateTime fechaFin);
    }
}

