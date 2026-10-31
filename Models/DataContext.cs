using Microsoft.EntityFrameworkCore;

namespace VeterinariaULP.Models
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        public DbSet<Especie> Especies { get; set; }
        public DbSet<Dueño> Dueños { get; set; }
        public DbSet<Servicio> Servicios { get; set; }
        public DbSet<Mascota> Mascotas { get; set; }
        public DbSet<Turno> Turnos { get; set; }
        public DbSet<AtencionClinica> AtencionesClinicas { get; set; }
        public DbSet<Pago> Pagos { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
    }
}
