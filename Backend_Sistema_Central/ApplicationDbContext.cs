// Backend_Sistema_Central/ApplicationDbContext.cs
using Backend_Sistema_Central.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend_Sistema_Central;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> opt)
    : DbContext(opt)
{
    public DbSet<Usuario>        Usuarios        => Set<Usuario>();
    public DbSet<DispositivoUSB> DispositivosUSB => Set<DispositivoUSB>();
    public DbSet<LogActividad>   Logs            => Set<LogActividad>();

    // <<<<<<<<<<<<<<<< NUEVO: DbSet para registros de acceso >>>>>>>>>>>>>>>>
    public DbSet<AccesoLog>      Accesos         => Set<AccesoLog>();
    // <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<

    protected override void OnModelCreating(ModelBuilder mb)
    {
        // ─── DispositivoUSB ──────────────────────────────
        mb.Entity<DispositivoUSB>()
          .HasIndex(u => u.Serial)
          .IsUnique();

        mb.Entity<DispositivoUSB>()
          .HasOne(d => d.Usuario)
          .WithMany(u => u.USBs)
          .HasForeignKey(d => d.UsuarioId)
          .OnDelete(DeleteBehavior.Cascade)
          .IsRequired(false);

        mb.Entity<DispositivoUSB>()
          .Property(u => u.RpCipher)
          .IsRequired();

        mb.Entity<DispositivoUSB>()
          .Property(u => u.RpTag)
          .IsRequired();

        mb.Entity<DispositivoUSB>()
          .Property(u => u.Rol)
          .HasConversion<byte>()
          .IsRequired();

        // ─── Usuario ─────────────────────────────────────
        mb.Entity<Usuario>()
          .HasIndex(u => u.Rut)
          .IsUnique();

        // ─── AccesoLog ──────────────────────────────────
        mb.Entity<AccesoLog>()
          .HasIndex(a => a.SerialUsb);
        mb.Entity<AccesoLog>()
          .HasIndex(a => a.Rut);

        mb.Entity<AccesoLog>()
          .Property(a => a.Rut).IsRequired();
        mb.Entity<AccesoLog>()
          .Property(a => a.SerialUsb).IsRequired();
        mb.Entity<AccesoLog>()
          .Property(a => a.Ip).IsRequired();
        mb.Entity<AccesoLog>()
          .Property(a => a.Mac).IsRequired();
        mb.Entity<AccesoLog>()
          .Property(a => a.Fecha).IsRequired();
    }
}
