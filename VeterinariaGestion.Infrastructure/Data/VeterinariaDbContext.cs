using Microsoft.EntityFrameworkCore;
using VeterinariaGestion.Core.Entities;

namespace VeterinariaGestion.Infrastructure.Data;

/// <summary>
/// Contexto principal EF Core. Mapea todas las entidades
/// respetando exactamente el esquema SQLite del sistema.
/// </summary>
public class VeterinariaDbContext : DbContext
{
    public VeterinariaDbContext(DbContextOptions<VeterinariaDbContext> options)
        : base(options) { }

    public DbSet<Proveedor>       Proveedores       { get; set; }
    public DbSet<Tipo>            Tipos             { get; set; }
    public DbSet<Producto>        Productos         { get; set; }
    public DbSet<Cliente>         Clientes          { get; set; }
    public DbSet<Venta>           Ventas            { get; set; }
    public DbSet<VentaDetalle>    VentasDetalle     { get; set; }
    public DbSet<Cuota>           Cuotas            { get; set; }
    public DbSet<CuentaCorriente> CuentasCorrientes { get; set; }
    public DbSet<Especie>         Especies          { get; set; }
    public DbSet<Mascota>         Mascotas          { get; set; }
    public DbSet<Turno>           Turnos            { get; set; }
    public DbSet<Historial>       Historiales       { get; set; }
    public DbSet<Usuario>         Usuarios          { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── Proveedor ────────────────────────────────────────────────
        modelBuilder.Entity<Proveedor>(e =>
        {
            e.ToTable("Proveedores");
            e.HasKey(p => p.IdProveedor);
            e.Property(p => p.IdProveedor).HasColumnName("idProveedor");
            e.Property(p => p.RazonSocial).HasColumnName("razonSocial").IsRequired();
            e.Property(p => p.CuilCuit).HasColumnName("cuilCuit");
            e.Property(p => p.Telefono).HasColumnName("telefono");
            e.Property(p => p.Email).HasColumnName("email");
            e.Property(p => p.FechaAlta).HasColumnName("fechaAlta");
            e.Property(p => p.Estado).HasColumnName("estado").HasDefaultValue(1);
            e.HasIndex(p => p.CuilCuit).IsUnique();
        });

        // ── Tipo ─────────────────────────────────────────────────────
        modelBuilder.Entity<Tipo>(e =>
        {
            e.ToTable("Tipo");
            e.HasKey(t => t.IdTipo);
            e.Property(t => t.IdTipo).HasColumnName("idTipo");
            e.Property(t => t.Descripcion).HasColumnName("descripcion").IsRequired();
            e.Property(t => t.Estado).HasColumnName("estado").HasDefaultValue(1);
        });

        // ── Producto ─────────────────────────────────────────────────
        modelBuilder.Entity<Producto>(e =>
        {
            e.ToTable("Productos");
            e.HasKey(p => p.IdProducto);
            e.Property(p => p.IdProducto).HasColumnName("idProducto");
            e.Property(p => p.Codigo).HasColumnName("codigo");
            e.Property(p => p.Nombre).HasColumnName("nombre").IsRequired();
            e.Property(p => p.Stock).HasColumnName("stock");
            e.Property(p => p.PrecioMinorista).HasColumnName("precioMinorista");
            e.Property(p => p.PrecioMayorista).HasColumnName("precioMayorista");
            e.Property(p => p.Descripcion).HasColumnName("descripcion");
            e.Property(p => p.IdTipo).HasColumnName("id_tipo");
            e.Property(p => p.IdProveedor).HasColumnName("id_proveedor");
            e.Property(p => p.Estado).HasColumnName("estado").HasDefaultValue(1);

            e.HasOne(p => p.Tipo)
             .WithMany(t => t.Productos)
             .HasForeignKey(p => p.IdTipo)
             .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(p => p.Proveedor)
             .WithMany(pv => pv.Productos)
             .HasForeignKey(p => p.IdProveedor)
             .OnDelete(DeleteBehavior.SetNull);
        });

        // ── Cliente ──────────────────────────────────────────────────
        modelBuilder.Entity<Cliente>(e =>
        {
            e.ToTable("Clientes");
            e.HasKey(c => c.IdCliente);
            e.Property(c => c.IdCliente).HasColumnName("idCliente");
            e.Property(c => c.CuilDni).HasColumnName("cuildni");
            e.Property(c => c.Apellido).HasColumnName("apellido");
            e.Property(c => c.Nombre).HasColumnName("nombre");
            e.Property(c => c.Direccion).HasColumnName("direccion");
            e.Property(c => c.Telefono).HasColumnName("telefono");
            e.Property(c => c.Email).HasColumnName("email");
            e.Property(c => c.FechaAlta).HasColumnName("fechaAlta");
            e.Property(c => c.Estado).HasColumnName("estado").HasDefaultValue(1);
            e.Ignore(c => c.NombreCompleto);
        });

        // ── Venta ────────────────────────────────────────────────────
        modelBuilder.Entity<Venta>(e =>
        {
            e.ToTable("Ventas");
            e.HasKey(v => v.IdVenta);
            e.Property(v => v.IdVenta).HasColumnName("idVenta");
            e.Property(v => v.NumeroVenta).HasColumnName("numeroVenta");
            e.Property(v => v.Fecha).HasColumnName("fecha");
            e.Property(v => v.IdCliente).HasColumnName("id_cliente");
            e.Property(v => v.SubTotal).HasColumnName("subTotal");
            e.Property(v => v.Descuento).HasColumnName("descuento");
            e.Property(v => v.Recargo).HasColumnName("recargo");
            e.Property(v => v.Total).HasColumnName("total");
            e.Property(v => v.FormaPago).HasColumnName("formaPago");
            e.Property(v => v.EstadoPago).HasColumnName("estadoPago");
            e.Property(v => v.Observacion).HasColumnName("observacion");
            e.Property(v => v.Estado).HasColumnName("estado").HasDefaultValue(1);
            e.HasIndex(v => v.NumeroVenta).IsUnique();

            e.HasOne(v => v.Cliente)
             .WithMany(c => c.Ventas)
             .HasForeignKey(v => v.IdCliente)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── VentaDetalle ─────────────────────────────────────────────
        modelBuilder.Entity<VentaDetalle>(e =>
        {
            e.ToTable("Ventas_detalle");
            e.HasKey(vd => vd.IdVentaDetalle);
            e.Property(vd => vd.IdVentaDetalle).HasColumnName("idVentaDetalle");
            e.Property(vd => vd.IdVenta).HasColumnName("id_venta");
            e.Property(vd => vd.IdProducto).HasColumnName("id_producto");
            e.Property(vd => vd.Cantidad).HasColumnName("cantidad");
            e.Property(vd => vd.PrecioUnitario).HasColumnName("precio_unitario");
            e.Property(vd => vd.DescuentoItem).HasColumnName("descuentoItem").HasDefaultValue(0);
            e.Property(vd => vd.SubTotalItem).HasColumnName("subTotalItem");

            e.HasOne(vd => vd.Venta)
             .WithMany(v => v.Detalles)
             .HasForeignKey(vd => vd.IdVenta)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(vd => vd.Producto)
             .WithMany(p => p.VentaDetalles)
             .HasForeignKey(vd => vd.IdProducto)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Cuota ────────────────────────────────────────────────────
        modelBuilder.Entity<Cuota>(e =>
        {
            e.ToTable("Cuotas");
            e.HasKey(c => c.IdCuota);
            e.Property(c => c.IdCuota).HasColumnName("idCuota");
            e.Property(c => c.NumeroCuota).HasColumnName("numeroCuota");
            e.Property(c => c.FechaVencimiento).HasColumnName("fechaVencimiento");
            e.Property(c => c.FechaPago).HasColumnName("fechaPago");
            e.Property(c => c.MontoCuota).HasColumnName("montoCuota");
            e.Property(c => c.MontoPagado).HasColumnName("montoPagado");
            e.Property(c => c.SaldoPendiente).HasColumnName("saldoPendiente");
            e.Property(c => c.IdVenta).HasColumnName("id_venta");
            e.Property(c => c.InteresMora).HasColumnName("interesMora");
            e.Property(c => c.EstadoCuota).HasColumnName("estadoCuota");
            e.Property(c => c.Estado).HasColumnName("estado").HasDefaultValue(1);

            e.HasOne(c => c.Venta)
             .WithMany(v => v.Cuotas)
             .HasForeignKey(c => c.IdVenta)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── CuentaCorriente ──────────────────────────────────────────
        modelBuilder.Entity<CuentaCorriente>(e =>
        {
            e.ToTable("CuentasCorrientes");
            e.HasKey(cc => cc.IdCuentaCorriente);
            e.Property(cc => cc.IdCuentaCorriente).HasColumnName("idCuentaCorriente");
            e.Property(cc => cc.IdCliente).HasColumnName("id_cliente");
            e.Property(cc => cc.IdVenta).HasColumnName("id_venta");
            e.Property(cc => cc.FechaMovimiento).HasColumnName("fechaMovimiento");
            e.Property(cc => cc.TipoMovimiento).HasColumnName("tipo_movimiento");
            e.Property(cc => cc.Concepto).HasColumnName("concepto");
            e.Property(cc => cc.Importe).HasColumnName("importe");
            e.Property(cc => cc.SaldoAnterior).HasColumnName("saldoAnterior");
            e.Property(cc => cc.SaldoNuevo).HasColumnName("saldoNuevo");
            e.Property(cc => cc.FechaVencimiento).HasColumnName("fechaVencimiento");
            e.Property(cc => cc.EstadoCuenta).HasColumnName("estadoCuenta");
            e.Property(cc => cc.Comprobante).HasColumnName("comprobante");
            e.Property(cc => cc.Observacion).HasColumnName("observacion");
            e.Property(cc => cc.Estado).HasColumnName("estado").HasDefaultValue(1);

            e.HasOne(cc => cc.Cliente)
             .WithMany(c => c.CuentasCorrientes)
             .HasForeignKey(cc => cc.IdCliente)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(cc => cc.Venta)
             .WithMany(v => v.CuentasCorrientes)
             .HasForeignKey(cc => cc.IdVenta)
             .OnDelete(DeleteBehavior.SetNull);
        });

        // ── Especie ──────────────────────────────────────────────────
        modelBuilder.Entity<Especie>(e =>
        {
            e.ToTable("Especies");
            e.HasKey(es => es.IdEspecie);
            e.Property(es => es.IdEspecie).HasColumnName("idEspecie");
            e.Property(es => es.Descripcion).HasColumnName("descripcion");
            e.Property(es => es.Estado).HasColumnName("estado").HasDefaultValue(1);
        });

        // ── Mascota ──────────────────────────────────────────────────
        modelBuilder.Entity<Mascota>(e =>
        {
            e.ToTable("Mascotas");
            e.HasKey(m => m.IdMascota);
            e.Property(m => m.IdMascota).HasColumnName("idMascota");
            e.Property(m => m.Nombre).HasColumnName("nombre");
            e.Property(m => m.IdCliente).HasColumnName("id_cliente");
            e.Property(m => m.IdEspecie).HasColumnName("id_especie");
            e.Property(m => m.Sexo).HasColumnName("sexo");
            e.Property(m => m.FechaNaci).HasColumnName("fechaNaci");
            e.Property(m => m.Estado).HasColumnName("estado").HasDefaultValue(1);

            e.HasOne(m => m.Cliente)
             .WithMany(c => c.Mascotas)
             .HasForeignKey(m => m.IdCliente)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(m => m.Especie)
             .WithMany(es => es.Mascotas)
             .HasForeignKey(m => m.IdEspecie)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Turno ────────────────────────────────────────────────────
        modelBuilder.Entity<Turno>(e =>
        {
            e.ToTable("Turnos");
            e.HasKey(t => t.IdTurno);
            e.Property(t => t.IdTurno).HasColumnName("idTurno");
            e.Property(t => t.Fecha).HasColumnName("fecha");
            e.Property(t => t.EstadoTurno).HasColumnName("estadoTurno");
            e.Property(t => t.HoraTurno).HasColumnName("horaTurno");
            e.Property(t => t.Motivo).HasColumnName("motivo");
            e.Property(t => t.IdMascota).HasColumnName("id_mascota");
            e.Property(t => t.Observacion).HasColumnName("observacion");
            e.Property(t => t.Estado).HasColumnName("estado").HasDefaultValue(1);

            e.HasOne(t => t.Mascota)
             .WithMany(m => m.Turnos)
             .HasForeignKey(t => t.IdMascota)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Historial ────────────────────────────────────────────────
        modelBuilder.Entity<Historial>(e =>
        {
            e.ToTable("Historial");
            e.HasKey(h => h.IdHistorial);
            e.Property(h => h.IdHistorial).HasColumnName("idHistorial");
            e.Property(h => h.IdMascota).HasColumnName("id_mascota");
            e.Property(h => h.IdTurno).HasColumnName("id_turno");
            e.Property(h => h.MotivoConsulta).HasColumnName("motivoConsulta");
            e.Property(h => h.Diagnostico).HasColumnName("diagnostico");
            e.Property(h => h.Tratamiento).HasColumnName("tratamiento");
            e.Property(h => h.Indicacion).HasColumnName("indicacion");
            e.Property(h => h.Observacion).HasColumnName("observacion");
            e.Property(h => h.FechaConsulta).HasColumnName("fechaConsulta");
            e.Property(h => h.ProximoControl).HasColumnName("proximoControl");
            e.Property(h => h.Estado).HasColumnName("estado").HasDefaultValue(1);

            e.HasOne(h => h.Mascota)
             .WithMany(m => m.Historiales)
             .HasForeignKey(h => h.IdMascota)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(h => h.Turno)
             .WithOne(t => t.Historial)
             .HasForeignKey<Historial>(h => h.IdTurno)
             .OnDelete(DeleteBehavior.SetNull);
        });

        // ── Usuario ──────────────────────────────────────────────────
        modelBuilder.Entity<Usuario>(e =>
        {
            e.ToTable("Usuarios");
            e.HasKey(u => u.IdUsuario);
            e.Property(u => u.IdUsuario).HasColumnName("idUsuario");
            e.Property(u => u.NombreUsuario).HasColumnName("nombreUsuario").IsRequired();
            e.Property(u => u.Email).HasColumnName("email").IsRequired();
            e.Property(u => u.PasswordHash).HasColumnName("passwordHash");
            e.Property(u => u.FechaCreacion).HasColumnName("fechaCreacion");
            e.Property(u => u.Activo).HasColumnName("activo").HasDefaultValue(1);
            e.HasIndex(u => u.NombreUsuario).IsUnique();
            e.HasIndex(u => u.Email).IsUnique();
        });
    }
}