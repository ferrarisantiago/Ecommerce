using Ecommerce_Modelos;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce_Datos.Datos
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options):base(options)
        {

        }

        //Se implementa todos los modelos que se crean
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<TipoAplicacion> TiposAplicaciones { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<UsuarioAplicacion> UsuariosAplicaciones { get; set; }
        public DbSet<Venta> Ventas { get; set; }
        public DbSet<VentaDetalle> VentaDetalles { get; set; }
        public DbSet<Orden> Ordenes { get; set; }
        public DbSet<OrdenDetalle> OrdenDetalles { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Orden>().ToTable("Orden");
            builder.Entity<OrdenDetalle>().ToTable("OrdenDetalle");
            builder.Entity<VentaDetalle>().ToTable("ventaDetalles");

            builder.Entity<Producto>()
                .HasIndex(p => new { p.IsDeleted, p.IsActive, p.CreatedAt });

            builder.Entity<Producto>()
                .Property(p => p.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Entity<Producto>()
                .Property(p => p.IsActive)
                .HasDefaultValue(true);

            builder.Entity<Producto>()
                .Property(p => p.MinimumStockLevel)
                .HasDefaultValue(0);

            builder.Entity<Orden>()
                .Property(o => o.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");
        }
    }
}
