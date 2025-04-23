using Backend_Sistema_Central.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend_Sistema_Central;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> opt)
    : DbContext(opt)
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<DispositivoUSB> DispositivosUSB => Set<DispositivoUSB>();
    public DbSet<LogActividad> Logs => Set<LogActividad>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<Usuario>().HasIndex(u => u.Rut).IsUnique();
        mb.Entity<DispositivoUSB>().HasIndex(u => u.Serial).IsUnique();
    }
}
