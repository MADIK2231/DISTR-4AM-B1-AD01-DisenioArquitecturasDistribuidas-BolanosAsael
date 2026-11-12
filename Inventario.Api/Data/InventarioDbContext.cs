using Inventario.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Api.Data;

public class InventarioDbContext(DbContextOptions<InventarioDbContext> options) : DbContext(options)
{
    public DbSet<Bodega> Bodegas => Set<Bodega>();
    public DbSet<Existencia> Existencias => Set<Existencia>();
    public DbSet<MovimientoInventario> MovimientosInventario => Set<MovimientoInventario>();
    public DbSet<ReservaStock> ReservasStock => Set<ReservaStock>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Existencia>(entidad =>
        {
            entidad.HasIndex(e => new { e.IdProducto, e.IdBodega }).IsUnique();

            entidad.HasOne(e => e.Bodega)
                .WithMany(b => b.Existencias)
                .HasForeignKey(e => e.IdBodega)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<MovimientoInventario>(entidad =>
        {
            entidad.HasOne(m => m.Bodega)
                .WithMany(b => b.Movimientos)
                .HasForeignKey(m => m.IdBodega)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ReservaStock>(entidad =>
        {
            entidad.HasOne(r => r.Bodega)
                .WithMany(b => b.Reservas)
                .HasForeignKey(r => r.IdBodega)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
