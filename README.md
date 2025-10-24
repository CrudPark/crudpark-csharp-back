# CrudPark Backend API

Sistema de gestión de parqueadero - API REST desarrollada en ASP.NET Core 8.0 con Entity Framework Core y PostgreSQL.

## Descripción

API REST completa para la gestión de un sistema de parqueadero que incluye operadores, tarifas, mensualidades, tickets, pagos y turnos. Desarrollada con arquitectura limpia y patrones de diseño modernos.

## Tecnologías

- **Framework**: ASP.NET Core 8.0
- **Base de Datos**: PostgreSQL con Entity Framework Core
- **ORM**: Entity Framework Core 8.0
- **Validación**: FluentValidation
- **Mapeo**: AutoMapper
- **Documentación**: Swagger/OpenAPI
- **Email**: MailKit y MimeKit
- **Exportación**: CsvHelper y EPPlus

## Estructura del Proyecto

```
crudpark-backend/
├── Controllers/           # Controladores de la API
│   ├── DashboardController.cs
│   ├── HealthController.cs
│   ├── IngresosController.cs
│   ├── MensualidadesController.cs
│   ├── OperadoresController.cs
│   ├── PagosController.cs
│   ├── ReportesController.cs
│   ├── TarifasController.cs
│   └── TurnosController.cs
├── Models/               # Modelos de datos
│   ├── BaseEntity.cs
│   ├── Ingreso.cs
│   ├── Mensualidad.cs
│   ├── Operador.cs
│   ├── Pago.cs
│   ├── ParkingDbContext.cs
│   ├── Tarifa.cs
│   └── Turno.cs
├── DTOs/                 # Data Transfer Objects
│   ├── DashboardDTO.cs
│   ├── IngresoDTO.cs
│   ├── MensualidadDTO.cs
│   ├── OperadorDTO.cs
│   ├── TarifaDTO.cs
│   └── TurnoDTO.cs
├── Services/             # Lógica de negocio
│   ├── DashboardService.cs
│   ├── EmailService.cs
│   ├── IDashboardService.cs
│   ├── IEmailService.cs
│   ├── IParkingService.cs
│   ├── IReporteService.cs
│   ├── ParkingService.cs
│   └── ReporteService.cs
├── Mappings/             # Configuración de AutoMapper
│   └── MappingProfile.cs
├── Validators/           # Validaciones con FluentValidation
│   ├── CerrarTurnoValidator.cs
│   ├── CreateIngresoValidator.cs
│   ├── CreateMensualidadValidator.cs
│   ├── CreateOperadorValidator.cs
│   ├── CreateTarifaValidator.cs
│   ├── CreateTurnoValidator.cs
│   ├── FinalizarIngresoValidator.cs
│   ├── UpdateMensualidadValidator.cs
│   ├── UpdateOperadorValidator.cs
│   └── UpdateTarifaValidator.cs
├── Properties/           # Configuraciones de lanzamiento
│   └── launchSettings.json
├── appsettings.json      # Configuración de la aplicación
├── appsettings.Development.json
├── Program.cs            # Punto de entrada de la aplicación
├── crud-park-back.csproj # Archivo de proyecto
├── Dockerfile           # Configuración de Docker
└── .dockerignore        # Archivos a ignorar en Docker
```

## Modelos de Datos

### Operador
- Gestión de operadores del parqueadero
- Campos: Id, Nombre, Email, IsActive, CreatedAt, UpdatedAt

### Tarifa
- Configuración de tarifas de estacionamiento
- Campos: Id, Nombre, ValorBaseHora, ValorFraccion, TopeDiario, TiempoGraciaMinutos, Activa

### Mensualidad
- Gestión de abonos mensuales
- Campos: Id, NombrePropietario, Email, Placa, FechaInicio, FechaFin, Activa

### Ingreso (Ticket)
- Registro de entrada y salida de vehículos
- Campos: Id, NumeroFolio, Placa, TipoIngreso, FechaIngreso, FechaSalida, OperadorIngresoId, OperadorSalidaId, TiempoEstadiaMinutos, MontoCobrado, Pagado, Activo, Estado

### Pago
- Registro de pagos realizados
- Campos: Id, TicketId, Monto, MetodoPago, OperadorId, FechaPago, Observaciones

### Turno
- Gestión de turnos de trabajo
- Campos: Id, OperadorId, FechaApertura, FechaCierre, TotalIngresos, TotalCobros, Activo, Observaciones

## Endpoints de la API

### Operadores
- `GET /api/operadores` - Listar operadores
- `GET /api/operadores/{id}` - Obtener operador por ID
- `POST /api/operadores` - Crear operador
- `PUT /api/operadores/{id}` - Actualizar operador
- `DELETE /api/operadores/{id}` - Eliminar operador (hard delete)
- `PATCH /api/operadores/{id}/toggle-estado` - Activar/desactivar operador

### Tarifas
- `GET /api/tarifas` - Listar tarifas
- `GET /api/tarifas/{id}` - Obtener tarifa por ID
- `POST /api/tarifas` - Crear tarifa
- `PUT /api/tarifas/{id}` - Actualizar tarifa
- `DELETE /api/tarifas/{id}` - Eliminar tarifa (hard delete)
- `PATCH /api/tarifas/{id}/toggle-estado` - Activar/desactivar tarifa

### Mensualidades
- `GET /api/mensualidades` - Listar mensualidades
- `GET /api/mensualidades/{id}` - Obtener mensualidad por ID
- `POST /api/mensualidades` - Crear mensualidad
- `PUT /api/mensualidades/{id}` - Actualizar mensualidad
- `DELETE /api/mensualidades/{id}` - Eliminar mensualidad (hard delete)
- `PATCH /api/mensualidades/{id}/toggle-estado` - Activar/desactivar mensualidad

### Ingresos/Tickets
- `GET /api/ingresos` - Listar ingresos
- `GET /api/ingresos/{id}` - Obtener ingreso por ID
- `POST /api/ingresos` - Crear ingreso
- `PUT /api/ingresos/{id}` - Actualizar ingreso
- `PATCH /api/ingresos/{id}/finalizar` - Finalizar ingreso
- `PATCH /api/ingresos/{id}/anular` - Anular ingreso

### Pagos
- `GET /api/pagos` - Listar pagos
- `GET /api/pagos/{id}` - Obtener pago por ID
- `POST /api/pagos` - Crear pago
- `DELETE /api/pagos/{id}` - Eliminar pago
- `GET /api/pagos/tickets-no-pagados` - Obtener tickets pendientes de pago

### Turnos
- `GET /api/turnos` - Listar turnos
- `GET /api/turnos/{id}` - Obtener turno por ID
- `POST /api/turnos` - Crear turno
- `PUT /api/turnos/{id}` - Actualizar turno
- `PATCH /api/turnos/{id}/cerrar` - Cerrar turno

### Dashboard
- `GET /api/dashboard` - Obtener datos del dashboard

### Reportes
- `GET /api/reportes/ingresos-por-hora` - Reporte de ingresos por hora
- `GET /api/reportes/ingresos-por-dia` - Reporte de ingresos por día
- `GET /api/reportes/ingresos-por-mes` - Reporte de ingresos por mes

### Health Check
- `GET /health` - Verificar estado de la API

## Configuración

### Variables de Entorno

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=crudpark_db;Username=crudpark_user;Password=CrudPark2025!;"
  },
  "AllowedOrigins": [
    "http://localhost:3000",
    "http://localhost:5173"
  ],
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SmtpUsername": "your-email@gmail.com",
    "SmtpPassword": "your-app-password",
    "FromEmail": "your-email@gmail.com",
    "FromName": "CrudPark System"
  }
}
```

## Instalación y Ejecución

### Prerrequisitos
- .NET 8.0 SDK
- PostgreSQL 13+
- Docker (opcional)

### Desarrollo Local

1. Clonar el repositorio
2. Restaurar paquetes NuGet:
   ```bash
   dotnet restore
   ```
3. Configurar la cadena de conexión en `appsettings.json`
4. Ejecutar migraciones:
   ```bash
   dotnet ef database update
   ```
5. Ejecutar la aplicación:
   ```bash
   dotnet run
   ```

### Docker

1. Construir la imagen:
   ```bash
   docker build -t crudpark-api .
   ```
2. Ejecutar el contenedor:
   ```bash
   docker run -p 8080:8080 crudpark-api
   ```

### Docker Compose

```bash
docker compose up --build
```

## Características Principales

### Validación de Datos
- Validación robusta con FluentValidation
- Validaciones personalizadas para cada entidad
- Mensajes de error en español

### Mapeo de Objetos
- AutoMapper para conversión entre entidades y DTOs
- Configuración centralizada en MappingProfile

### Manejo de Errores
- Middleware global de manejo de excepciones
- Logging estructurado con ILogger
- Respuestas HTTP consistentes

### CORS
- Configuración flexible para diferentes entornos
- Políticas de CORS específicas para desarrollo y producción

### Base de Datos
- Entity Framework Core con PostgreSQL
- Migraciones automáticas
- Configuración de índices para optimización
- Soft delete para entidades principales

### Exportación de Datos
- Exportación a CSV con CsvHelper
- Exportación a Excel con EPPlus
- Funcionalidad de reportes avanzados

## Patrones de Diseño

- **Repository Pattern**: Implementado a través de Entity Framework
- **Service Layer**: Lógica de negocio encapsulada en servicios
- **DTO Pattern**: Transferencia de datos con DTOs
- **Dependency Injection**: Inyección de dependencias nativa de ASP.NET Core
- **CQRS**: Separación de comandos y consultas en servicios

## Seguridad

- Validación de entrada en todos los endpoints
- Sanitización de datos de entrada
- Configuración CORS restrictiva en producción
- Logging de operaciones sensibles

## Monitoreo

- Health check endpoint
- Logging estructurado
- Métricas de rendimiento
- Trazabilidad de operaciones

## Contribución

1. Fork del proyecto
2. Crear rama para feature (`git checkout -b feature/AmazingFeature`)
3. Commit de cambios (`git commit -m 'Add some AmazingFeature'`)
4. Push a la rama (`git push origin feature/AmazingFeature`)
5. Abrir Pull Request

## Licencia

Este proyecto está bajo la Licencia MIT - ver el archivo [LICENSE](LICENSE) para detalles.
