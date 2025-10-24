using Microsoft.EntityFrameworkCore;

namespace crud_park_back.Models
{
    public class ParkingDbContext : DbContext
    {
        public ParkingDbContext(DbContextOptions<ParkingDbContext> options) : base(options)
        {
        }
        
        public DbSet<Operador> Operadores { get; set; }
        public DbSet<Mensualidad> Mensualidades { get; set; }
        public DbSet<Tarifa> Tarifas { get; set; }
        public DbSet<Ingreso> Ingresos { get; set; }
        public DbSet<Turno> Turnos { get; set; }
        public DbSet<Pago> Pagos { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configurar nombres de tablas según el esquema PostgreSQL
            modelBuilder.Entity<Operador>().ToTable("operadores");
            modelBuilder.Entity<Mensualidad>().ToTable("mensualidades");
            modelBuilder.Entity<Tarifa>().ToTable("tarifas");
            modelBuilder.Entity<Ingreso>().ToTable("tickets");
            modelBuilder.Entity<Turno>().ToTable("turnos");
            modelBuilder.Entity<Pago>().ToTable("pagos");
            
            // Configuración de Operador
            modelBuilder.Entity<Operador>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Nombre).HasColumnName("nombre");
                entity.Property(e => e.Email).HasColumnName("email");
                entity.Property(e => e.IsActive).HasColumnName("activo");
                entity.Property(e => e.CreatedAt).HasColumnName("fecha_creacion");
                entity.Property(e => e.UpdatedAt).HasColumnName("fecha_actualizacion");
            });
            
            // Configuración de Mensualidad
            modelBuilder.Entity<Mensualidad>(entity =>
            {
                entity.Ignore(e => e.Valor); // La columna no existe en la BD actual
                
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.NombrePropietario).HasColumnName("nombre_propietario");
                entity.Property(e => e.Email).HasColumnName("email");
                entity.Property(e => e.Placa).HasColumnName("placa");
                entity.Property(e => e.FechaInicio).HasColumnName("fecha_inicio");
                entity.Property(e => e.FechaFin).HasColumnName("fecha_fin");
                entity.Property(e => e.IsActive).HasColumnName("activa");
                entity.Property(e => e.NotificacionEnviada).HasColumnName("notificacion_enviada");
                entity.Property(e => e.CreatedAt).HasColumnName("fecha_creacion");
                entity.Property(e => e.UpdatedAt).HasColumnName("fecha_actualizacion");
                
                // Permitir múltiples mensualidades por placa (historial)
                entity.HasIndex(e => e.Placa);
            });
            
            // Configuración de Tarifa
            modelBuilder.Entity<Tarifa>(entity =>
            {
                entity.Property(e => e.Id)
                      .HasColumnName("id")
                      .ValueGeneratedOnAdd();
                      
                entity.Property(e => e.Nombre).HasColumnName("nombre");
                entity.Property(e => e.ValorBaseHora).HasColumnName("valor_base_hora");
                entity.Property(e => e.ValorFraccion).HasColumnName("valor_fraccion");
                entity.Property(e => e.TopeDiario).HasColumnName("tope_diario");
                entity.Property(e => e.TiempoGraciaMinutos).HasColumnName("tiempo_gracia_minutos");
                entity.Property(e => e.IsActive).HasColumnName("activa");
                entity.Property(e => e.CreatedAt).HasColumnName("fecha_creacion");
                entity.Property(e => e.UpdatedAt).HasColumnName("fecha_actualizacion");
            });
            
            // Configuración de Ingreso (Tickets)
            modelBuilder.Entity<Ingreso>(entity =>
            {
                entity.Ignore(e => e.IsActive); // Ignorar IsActive de BaseEntity, usamos Activo en su lugar
                
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.NumeroFolio).HasColumnName("numero_folio");
                entity.Property(e => e.Placa).HasColumnName("placa");
                entity.Property(e => e.FechaIngreso).HasColumnName("fecha_ingreso");
                entity.Property(e => e.FechaSalida).HasColumnName("fecha_salida");
                entity.Property(e => e.TipoIngreso).HasColumnName("tipo_ingreso");
                entity.Property(e => e.MontoCobrado).HasColumnName("monto_cobrado");
                entity.Property(e => e.TiempoEstadiaMinutos).HasColumnName("tiempo_estadia_minutos");
                entity.Property(e => e.Pagado).HasColumnName("pagado");
                entity.Property(e => e.Activo).HasColumnName("activo");
                entity.Property(e => e.QrCode).HasColumnName("qr_code");
                entity.Property(e => e.Estado).HasColumnName("estado");
                entity.Property(e => e.OperadorIngresoId).HasColumnName("operador_ingreso_id");
                entity.Property(e => e.OperadorSalidaId).HasColumnName("operador_salida_id");
                entity.Property(e => e.MensualidadId).HasColumnName("mensualidad_id");
                entity.Property(e => e.TarifaId).HasColumnName("tarifa_id");
                entity.Property(e => e.CreatedAt).HasColumnName("fecha_creacion");
                entity.Property(e => e.UpdatedAt).HasColumnName("fecha_actualizacion");
                
                entity.HasIndex(e => e.NumeroFolio).IsUnique();
                
                entity.HasOne(e => e.OperadorIngreso)
                      .WithMany(o => o.IngresosIngreso)
                      .HasForeignKey(e => e.OperadorIngresoId)
                      .OnDelete(DeleteBehavior.SetNull);
                      
                entity.HasOne(e => e.OperadorSalida)
                      .WithMany(o => o.IngresosSalida)
                      .HasForeignKey(e => e.OperadorSalidaId)
                      .OnDelete(DeleteBehavior.SetNull);
                      
                entity.HasOne(e => e.Mensualidad)
                      .WithMany(m => m.Ingresos)
                      .HasForeignKey(e => e.MensualidadId)
                      .OnDelete(DeleteBehavior.SetNull);
                      
                entity.HasOne(e => e.Tarifa)
                      .WithMany(t => t.Ingresos)
                      .HasForeignKey(e => e.TarifaId)
                      .OnDelete(DeleteBehavior.SetNull);
            });
            
            // Configuración de Turno
            modelBuilder.Entity<Turno>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.FechaApertura).HasColumnName("fecha_apertura");
                entity.Property(e => e.FechaCierre).HasColumnName("fecha_cierre");
                entity.Property(e => e.TotalIngresos).HasColumnName("total_ingresos");
                entity.Property(e => e.TotalCobros).HasColumnName("total_cobros");
                entity.Property(e => e.Observaciones).HasColumnName("observaciones");
                entity.Property(e => e.OperadorId).HasColumnName("operador_id");
                entity.Property(e => e.IsActive).HasColumnName("activo");
                entity.Property(e => e.CreatedAt).HasColumnName("fecha_creacion");
                entity.Property(e => e.UpdatedAt).HasColumnName("fecha_actualizacion");
                
                entity.HasOne(e => e.Operador)
                      .WithMany(o => o.Turnos)
                      .HasForeignKey(e => e.OperadorId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
            
            // Configuración de Pago
            modelBuilder.Entity<Pago>(entity =>
            {
                entity.Ignore(e => e.UpdatedAt); // La tabla pagos no tiene esta columna
                entity.Ignore(e => e.IsActive);  // La tabla pagos no tiene esta columna
                
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.TicketId).HasColumnName("ticket_id");
                entity.Property(e => e.Monto).HasColumnName("monto");
                entity.Property(e => e.MetodoPago).HasColumnName("metodo_pago");
                entity.Property(e => e.OperadorId).HasColumnName("operador_id");
                entity.Property(e => e.CreatedAt).HasColumnName("fecha_pago");
                entity.Property(e => e.Observaciones).HasColumnName("observaciones");
                
                entity.HasOne(e => e.Ticket)
                      .WithMany(i => i.Pagos)
                      .HasForeignKey(e => e.TicketId)
                      .OnDelete(DeleteBehavior.Restrict);
                      
                entity.HasOne(e => e.Operador)
                      .WithMany(o => o.Pagos)
                      .HasForeignKey(e => e.OperadorId)
                      .OnDelete(DeleteBehavior.SetNull);
            });
        }
        
        private void SeedData(ModelBuilder modelBuilder)
        {
            // Tarifa por defecto
            modelBuilder.Entity<Tarifa>().HasData(
                new Tarifa
                {
                    Id = 1,
                    Nombre = "Tarifa Estándar",
                    ValorBaseHora = 2000,
                    ValorFraccion = 500,
                    TopeDiario = 15000,
                    TiempoGraciaMinutos = 30,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                }
            );
        }
    }
}

