using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;
using ServiceDeskNg.Server.Models;

namespace ServiceDeskNg.Server.Data;

public partial class ServiceDeskContext : DbContext
{
    public ServiceDeskContext()
    {
    }

    public ServiceDeskContext(DbContextOptions<ServiceDeskContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Administrador> Administradores { get; set; }

    public virtual DbSet<Agente> Agentes { get; set; }

    public virtual DbSet<Auditoria> Auditoria { get; set; }

    public virtual DbSet<EndUser> Clientes { get; set; }

    public virtual DbSet<Integracion> Integraciones { get; set; }

    public virtual DbSet<NivelesAcceso> NivelesAccesos { get; set; }

    public virtual DbSet<Sesion> Sesiones { get; set; }

    public virtual DbSet<Supervisor> Supervisores { get; set; }

    public virtual DbSet<Ticket> Tickets { get; set; }

    public virtual DbSet<TicketArchivo> TicketArchivos { get; set; }

    public virtual DbSet<TicketMensaje> TicketMensajes { get; set; }

    public virtual DbSet<TicketsCategoria> TicketsCategorias { get; set; }

    public virtual DbSet<TicketsEstado> TicketsEstados { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySql("server=localhost;port=3306;database=servicedesk;user=root;password=2852", Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.41-mysql"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Administrador>(entity =>
        {
            entity.HasKey(e => e.IdAdmin).HasName("PRIMARY");

            entity.ToTable("administradores");

            entity.HasIndex(e => e.IdNivel, "id_nivel");

            entity.HasIndex(e => e.IdUsuario, "id_usuario");

            entity.Property(e => e.IdAdmin).HasColumnName("id_admin");
            entity.Property(e => e.AreaResponsabilidadAdmin)
                .HasMaxLength(100)
                .HasColumnName("area_responsabilidad_admin");
            entity.Property(e => e.IdNivel).HasColumnName("id_nivel");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");

            entity.HasOne(d => d.IdNivelNavigation).WithMany(p => p.Administradores)
                .HasForeignKey(d => d.IdNivel)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("administradores_ibfk_2");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Administradores)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("administradores_ibfk_1");
        });

        modelBuilder.Entity<Agente>(entity =>
        {
            entity.HasKey(e => e.IdAgente).HasName("PRIMARY");

            entity.ToTable("agentes");

            entity.HasIndex(e => e.IdNivel, "id_nivel");

            entity.HasIndex(e => e.IdUsuario, "id_usuario");

            entity.Property(e => e.IdAgente).HasColumnName("id_agente");
            entity.Property(e => e.DisponibilidadAgente)
                .HasDefaultValueSql("'1'")
                .HasColumnName("disponibilidad_agente");
            entity.Property(e => e.EspecialidadAgente)
                .HasMaxLength(100)
                .HasColumnName("especialidad_agente");
            entity.Property(e => e.IdNivel).HasColumnName("id_nivel");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");

            entity.HasOne(d => d.IdNivelNavigation).WithMany(p => p.Agentes)
                .HasForeignKey(d => d.IdNivel)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("agentes_ibfk_2");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Agentes)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("agentes_ibfk_1");
        });

        modelBuilder.Entity<Auditoria>(entity =>
        {
            entity.HasKey(e => e.IdAuditoria).HasName("PRIMARY");

            entity.ToTable("auditoria");

            entity.HasIndex(e => e.IdUsuario, "id_usuario");

            entity.Property(e => e.IdAuditoria).HasColumnName("id_auditoria");
            entity.Property(e => e.AccionAuditoria)
                .HasMaxLength(200)
                .HasColumnName("accion_auditoria");
            entity.Property(e => e.DetalleAuditoria)
                .HasColumnType("text")
                .HasColumnName("detalle_auditoria");
            entity.Property(e => e.FechaAuditoria)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("fecha_auditoria");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");

        });

        modelBuilder.Entity<EndUser>(entity =>
        {
            entity.HasKey(e => e.IdCliente).HasName("PRIMARY");

            entity.ToTable("clientes");

            entity.HasIndex(e => e.IdNivel, "id_nivel");

            entity.HasIndex(e => e.IdUsuario, "id_usuario");

            entity.Property(e => e.IdCliente).HasColumnName("id_cliente");
            entity.Property(e => e.IdNivel).HasColumnName("id_nivel");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");

            entity.HasOne(d => d.IdNivelNavigation).WithMany(p => p.EndUsers)
                .HasForeignKey(d => d.IdNivel)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("clientes_ibfk_2");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Clientes)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("clientes_ibfk_1");
        });

        modelBuilder.Entity<Integracion>(entity =>
        {
            entity.HasKey(e => e.IdIntegracion).HasName("PRIMARY");

            entity.ToTable("integraciones");

            entity.HasIndex(e => e.IdTicket, "id_ticket");

            entity.Property(e => e.IdIntegracion).HasColumnName("id_integracion");
            entity.Property(e => e.EstadoIntegracion)
                .HasDefaultValueSql("'recibido'")
                .HasColumnType("enum('recibido','procesado','error')")
                .HasColumnName("estado_integracion");
            entity.Property(e => e.FechaHoraCreacionIntegracion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("fecha_hora_creacion_integracion");
            entity.Property(e => e.IdTicket).HasColumnName("id_ticket");
            entity.Property(e => e.OrigenIntegracion)
                .HasColumnType("enum('correo','teams','slack','chatbot','api')")
                .HasColumnName("origen_integracion");
            entity.Property(e => e.ReferenciaIntegracion)
                .HasMaxLength(255)
                .HasColumnName("referencia_integracion");

            entity.HasOne(d => d.IdTicketNavigation).WithMany(p => p.Integraciones)
                .HasForeignKey(d => d.IdTicket)
                .HasConstraintName("integraciones_ibfk_1");
        });

        modelBuilder.Entity<NivelesAcceso>(entity =>
        {
            entity.HasKey(e => e.IdNivel).HasName("PRIMARY");

            entity.ToTable("niveles_acceso");

            entity.Property(e => e.IdNivel).HasColumnName("id_nivel");
            entity.Property(e => e.Nivel).HasColumnName("nivel");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<Sesion>(entity =>
        {
            entity.HasKey(e => e.IdSesion).HasName("PRIMARY");

            entity.ToTable("sesiones");

            entity.HasIndex(e => e.IdUsuario, "id_usuario");

            entity.Property(e => e.IdSesion).HasColumnName("id_sesion");
            entity.Property(e => e.FechaHoraFinSesion)
                .HasColumnType("timestamp")
                .HasColumnName("fecha_hora_fin_sesion");
            entity.Property(e => e.FechaHoraInicioSesion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("fecha_hora_inicio_sesion");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.SesionActiva)
                .HasDefaultValueSql("'1'")
                .HasColumnName("sesion_activa");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Sesiones)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("sesiones_ibfk_1");
        });

        modelBuilder.Entity<Supervisor>(entity =>
        {
            entity.HasKey(e => e.IdSupervisor).HasName("PRIMARY");

            entity.ToTable("supervisores");

            entity.HasIndex(e => e.IdNivel, "id_nivel");

            entity.HasIndex(e => e.IdUsuario, "id_usuario");

            entity.Property(e => e.IdSupervisor).HasColumnName("id_supervisor");
            entity.Property(e => e.AreaResponsabilidadSupervisor)
                .HasMaxLength(100)
                .HasColumnName("area_responsabilidad_supervisor");
            entity.Property(e => e.IdNivel).HasColumnName("id_nivel");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");

            entity.HasOne(d => d.IdNivelNavigation).WithMany(p => p.Supervisores)
                .HasForeignKey(d => d.IdNivel)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("supervisores_ibfk_2");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Supervisores)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("supervisores_ibfk_1");
        });

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasKey(e => e.IdTicket).HasName("PRIMARY");

            entity.ToTable("tickets");

            entity.HasIndex(e => e.IdAgenteAsignado, "id_agente_asignado");

            entity.HasIndex(e => e.IdCategoriaTicket, "id_categoria_ticket");

            entity.HasIndex(e => e.IdCliente, "id_cliente");

            entity.HasIndex(e => e.IdEstadoTicket, "id_estado_ticket");

            entity.Property(e => e.IdTicket).HasColumnName("id_ticket");
            entity.Property(e => e.DepartamentoTicket)
                .HasMaxLength(100)
                .HasColumnName("departamento_ticket");
            entity.Property(e => e.DescripcionTicket)
                .HasColumnType("text")
                .HasColumnName("descripcion_ticket");
            entity.Property(e => e.FechaHoraActualizacionTicket)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("fecha_hora_actualizacion_ticket");
            entity.Property(e => e.FechaHoraCreacionTicket)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("fecha_hora_creacion_ticket");
            entity.Property(e => e.IdAgenteAsignado).HasColumnName("id_agente_asignado");
            entity.Property(e => e.IdCategoriaTicket).HasColumnName("id_categoria_ticket");
            entity.Property(e => e.IdCliente).HasColumnName("id_cliente");
            entity.Property(e => e.IdEstadoTicket).HasColumnName("id_estado_ticket");
            entity.Property(e => e.PrioridadTicket)
                .HasDefaultValueSql("'media'")
                .HasColumnType("enum('baja','media','alta','urgente')")
                .HasColumnName("prioridad_ticket");
            entity.Property(e => e.TituloTicket)
                .HasMaxLength(200)
                .HasColumnName("titulo_ticket");
            entity.Property(e => e.UbicacionTicket)
                .HasMaxLength(100)
                .HasColumnName("ubicacion_ticket");

            entity.HasOne(d => d.IdAgenteAsignadoNavigation).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.IdAgenteAsignado)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("tickets_ibfk_2");

            entity.HasOne(d => d.IdCategoriaTicketNavigation).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.IdCategoriaTicket)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tickets_ibfk_4");

            entity.HasOne(d => d.IdClienteNavigation).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.IdCliente)
                .HasConstraintName("tickets_ibfk_1");

            entity.HasOne(d => d.IdEstadoTicketNavigation).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.IdEstadoTicket)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tickets_ibfk_3");
        });

        modelBuilder.Entity<TicketArchivo>(entity =>
        {
            entity.HasKey(e => e.IdArchivo).HasName("PRIMARY");

            entity.ToTable("ticket_archivos");

            entity.HasIndex(e => e.IdTicket, "id_ticket");

            entity.HasIndex(e => e.IdUsuario, "id_usuario");

            entity.Property(e => e.IdArchivo).HasColumnName("id_archivo");
            entity.Property(e => e.FechaHoraCreacionMensaje)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("fecha_hora_creacion_mensaje");
            entity.Property(e => e.IdTicket).HasColumnName("id_ticket");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.RutaArchivoTicket)
                .HasMaxLength(255)
                .HasColumnName("ruta_archivo_ticket");

            entity.HasOne(d => d.IdTicketNavigation).WithMany(p => p.TicketArchivos)
                .HasForeignKey(d => d.IdTicket)
                .HasConstraintName("ticket_archivos_ibfk_1");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.TicketArchivos)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("ticket_archivos_ibfk_2");
        });

        modelBuilder.Entity<TicketMensaje>(entity =>
        {
            entity.HasKey(e => e.IdMensaje).HasName("PRIMARY");

            entity.ToTable("ticket_mensajes");

            entity.HasIndex(e => e.IdTicket, "id_ticket");

            entity.HasIndex(e => e.IdUsuario, "id_usuario");

            entity.Property(e => e.IdMensaje).HasColumnName("id_mensaje");
            entity.Property(e => e.FechaHoraCreacionMensaje)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("fecha_hora_creacion_mensaje");
            entity.Property(e => e.IdTicket).HasColumnName("id_ticket");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.MensajeTicket)
                .HasColumnType("text")
                .HasColumnName("mensaje_ticket");

            entity.HasOne(d => d.IdTicketNavigation).WithMany(p => p.TicketMensajes)
                .HasForeignKey(d => d.IdTicket)
                .HasConstraintName("ticket_mensajes_ibfk_1");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.TicketMensajes)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("ticket_mensajes_ibfk_2");
        });

        modelBuilder.Entity<TicketsCategoria>(entity =>
        {
            entity.HasKey(e => e.IdCategoria).HasName("PRIMARY");

            entity.ToTable("tickets_categorias");

            entity.Property(e => e.IdCategoria).HasColumnName("id_categoria");
            entity.Property(e => e.NombreCategoria)
                .HasMaxLength(50)
                .HasColumnName("nombre_categoria");
        });

        modelBuilder.Entity<TicketsEstado>(entity =>
        {
            entity.HasKey(e => e.IdEstado).HasName("PRIMARY");

            entity.ToTable("tickets_estados");

            entity.Property(e => e.IdEstado).HasColumnName("id_estado");
            entity.Property(e => e.NombreEstado)
                .HasMaxLength(50)
                .HasColumnName("nombre_estado");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario).HasName("PRIMARY");

            entity.ToTable("usuarios");

            entity.HasIndex(e => e.CorreoUsuario, "correo_usuario").IsUnique();

            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.ContrasenaUsuario)
                .HasMaxLength(255)
                .HasColumnName("contrasena_usuario");
            entity.Property(e => e.CorreoUsuario)
                .HasMaxLength(150)
                .HasColumnName("correo_usuario");
            entity.Property(e => e.DepartamentoUsuario)
                .HasMaxLength(100)
                .HasColumnName("departamento_usuario");
            entity.Property(e => e.EstadoUsuario)
                .HasDefaultValueSql("'activo'")
                .HasColumnType("enum('activo','inactivo')")
                .HasColumnName("estado_usuario");
            entity.Property(e => e.FechaHoraCreacionUsuario)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("fecha_hora_creacion_usuario");
            entity.Property(e => e.NombreUsuario)
                .HasMaxLength(100)
                .HasColumnName("nombre_usuario");
            entity.Property(e => e.UbicacionUsuario)
                .HasMaxLength(100)
                .HasColumnName("ubicacion_usuario");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
