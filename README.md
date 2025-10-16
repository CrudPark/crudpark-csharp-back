# CrudPark - Sistema de Gestión de Parqueadero

Backend desarrollado en C# con ASP.NET Core para un sistema de gestión de parqueadero con funcionalidades completas de administración, reportes y notificaciones.

## 🚀 Características

### Funcionalidades Principales
- **Dashboard en tiempo real** con métricas del parqueadero
- **Gestión de Operadores** (crear, editar, inactivar)
- **Gestión de Mensualidades** con notificaciones automáticas
- **Gestión de Tarifas** configurables
- **Control de Ingresos y Salidas** de vehículos
- **Sistema de Turnos** para operadores
- **Reportes y Exportación** (CSV, Excel)
- **Notificaciones por Email** automáticas

### Reglas de Negocio
- ✅ Tiempo de gracia de 30 minutos (configurable)
- ✅ Un solo ticket abierto por placa
- ✅ Mensualidades vigentes sin cobro
- ✅ Cálculo automático de tarifas
- ✅ Validaciones de negocio completas

## 🛠️ Tecnologías

- **.NET 8.0** - Framework principal
- **ASP.NET Core** - API Web
- **Entity Framework Core** - ORM
- **SQL Server** - Base de datos
- **AutoMapper** - Mapeo de objetos
- **FluentValidation** - Validaciones
- **MailKit** - Envío de emails
- **CsvHelper** - Exportación CSV
- **EPPlus** - Exportación Excel
- **Docker** - Contenedorización

## 📋 Requisitos

- .NET 8.0 SDK
- SQL Server (LocalDB para desarrollo)
- Docker (opcional, para contenedores)

## 🚀 Instalación y Configuración

### Desarrollo Local

1. **Clonar el repositorio**
   ```bash
   git clone <repository-url>
   cd crudpark-csharp-back
   ```

2. **Configurar la base de datos**
   - Asegúrate de tener SQL Server LocalDB instalado
   - La aplicación creará automáticamente la base de datos

3. **Configurar Email (Opcional)**
   - Edita `appsettings.Development.json`
   - Configura las credenciales de Gmail SMTP o tu proveedor de email

4. **Ejecutar la aplicación**
   ```bash
   cd crud-park-back
   dotnet restore
   dotnet run
   ```

5. **Acceder a la API**
   - API: `https://localhost:7000` o `http://localhost:5000`
   - Swagger UI: `https://localhost:7000/swagger`

### Docker

1. **Configurar variables de entorno**
   ```bash
   export EMAIL_USERNAME=tu-email@gmail.com
   export EMAIL_PASSWORD=tu-app-password
   ```

2. **Ejecutar con Docker Compose**
   ```bash
   docker-compose up -d
   ```

3. **Acceder a la aplicación**
   - API: `http://localhost:8080`
   - Swagger UI: `http://localhost:8080/swagger`
   - Con Nginx: `https://localhost` (requiere certificados SSL)

## 📚 API Endpoints

### Dashboard
- `GET /api/dashboard` - Datos del dashboard
- `GET /api/dashboard/resumen` - Resumen de ingresos
- `GET /api/dashboard/ingresos-por-hora` - Ingresos por hora
- `GET /api/dashboard/mensualidades-proximas` - Mensualidades próximas a vencer

### Operadores
- `GET /api/operadores` - Listar operadores
- `POST /api/operadores` - Crear operador
- `PUT /api/operadores/{id}` - Actualizar operador
- `DELETE /api/operadores/{id}` - Eliminar operador

### Mensualidades
- `GET /api/mensualidades` - Listar mensualidades
- `POST /api/mensualidades` - Crear mensualidad
- `PUT /api/mensualidades/{id}` - Actualizar mensualidad
- `DELETE /api/mensualidades/{id}` - Eliminar mensualidad
- `GET /api/mensualidades/verificar-vigencia/{placa}` - Verificar mensualidad vigente

### Tarifas
- `GET /api/tarifas` - Listar tarifas
- `GET /api/tarifas/activa` - Obtener tarifa activa
- `POST /api/tarifas` - Crear tarifa
- `PUT /api/tarifas/{id}` - Actualizar tarifa
- `DELETE /api/tarifas/{id}` - Eliminar tarifa

### Ingresos
- `GET /api/ingresos` - Listar ingresos
- `GET /api/ingresos/activos` - Ingresos activos (vehículos en parqueadero)
- `POST /api/ingresos` - Registrar ingreso
- `POST /api/ingresos/finalizar` - Finalizar ingreso
- `GET /api/ingresos/activo/{placa}` - Ingreso activo por placa

### Turnos
- `GET /api/turnos` - Listar turnos
- `POST /api/turnos` - Abrir turno
- `POST /api/turnos/cerrar` - Cerrar turno
- `GET /api/turnos/abierto/{operadorId}` - Turno abierto por operador

### Reportes
- `GET /api/reportes/ingresos` - Reporte de ingresos
- `GET /api/reportes/exportar-csv` - Exportar CSV
- `GET /api/reportes/exportar-excel` - Exportar Excel
- `GET /api/reportes/ingresos-diarios` - Ingresos diarios
- `GET /api/reportes/promedio-ocupacion` - Promedio de ocupación

### Health Check
- `GET /health` - Health check simple
- `GET /api/health` - Health check detallado

## 🔧 Configuración

### Variables de Entorno

```bash
# Base de datos
ConnectionStrings__DefaultConnection=Server=localhost;Database=CrudParkDB;Trusted_Connection=true

# Email
EmailSettings__SmtpServer=smtp.gmail.com
EmailSettings__SmtpPort=587
EmailSettings__SmtpUsername=tu-email@gmail.com
EmailSettings__SmtpPassword=tu-app-password

# Configuración del parqueadero
ParkingSettings__GraceTimeMinutes=30
ParkingSettings__DefaultHourlyRate=2000
ParkingSettings__DefaultFractionRate=500
ParkingSettings__DefaultDailyCap=20000
```

### Configuración de Email

Para usar Gmail SMTP:
1. Habilita la verificación en 2 pasos
2. Genera una contraseña de aplicación
3. Usa esa contraseña en la configuración

## 🧪 Testing

```bash
# Ejecutar tests
dotnet test

# Ejecutar con cobertura
dotnet test --collect:"XPlat Code Coverage"
```

## 📊 Base de Datos

### Entidades Principales
- **Operador** - Personal del parqueadero
- **Mensualidad** - Suscripciones mensuales
- **Tarifa** - Reglas de cobro
- **Ingreso** - Registro de vehículos
- **Turno** - Control de turnos de operadores

### Migraciones
```bash
# Crear migración
dotnet ef migrations add NombreMigracion

# Aplicar migraciones
dotnet ef database update
```

## 🚀 Despliegue

### Docker Production
```bash
# Build de producción
docker build -t crudpark-api .

# Ejecutar con variables de entorno
docker run -d \
  -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="Server=host.docker.internal;Database=CrudParkDB;User Id=sa;Password=Password123!;TrustServerCertificate=true" \
  -e EmailSettings__SmtpUsername="tu-email@gmail.com" \
  -e EmailSettings__SmtpPassword="tu-app-password" \
  crudpark-api
```

### Docker Compose Production
```bash
# Con variables de entorno
EMAIL_USERNAME=tu-email@gmail.com EMAIL_PASSWORD=tu-app-password docker-compose up -d
```

## 📝 Notas de Desarrollo

- La aplicación usa soft delete (IsActive) para mantener integridad referencial
- Las validaciones se realizan tanto en el cliente como en el servidor
- Los emails se envían de forma asíncrona
- La aplicación es compatible con CORS para desarrollo frontend
- Incluye logging estructurado para monitoreo

## 🤝 Contribución

1. Fork el proyecto
2. Crea una rama para tu feature (`git checkout -b feature/AmazingFeature`)
3. Commit tus cambios (`git commit -m 'Add some AmazingFeature'`)
4. Push a la rama (`git push origin feature/AmazingFeature`)
5. Abre un Pull Request

