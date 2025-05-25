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
    public DbSet<RootKey>        RootKeys        => Set<RootKey>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        // ─── DispositivoUSB ────────────────────────────────────────
        mb.Entity<DispositivoUSB>()
          .HasIndex(u => u.Serial)
          .IsUnique();

        mb.Entity<DispositivoUSB>()
          .HasOne(d => d.Usuario)
          .WithMany(u => u.USBs)
          .HasForeignKey(d => d.UsuarioId)
          .OnDelete(DeleteBehavior.Cascade)
          .IsRequired(false);

        // byte[] → bytea (implícito)
        mb.Entity<DispositivoUSB>()
          .Property(u => u.RpCipher)
          .IsRequired();

        mb.Entity<DispositivoUSB>()
          .Property(u => u.RpTag)
          .IsRequired();

        mb.Entity<DispositivoUSB>()
          .Property(u => u.Rol)
          .HasConversion<byte>()   // enum → smallint
          .IsRequired();

        // ─── RootKey ───────────────────────────────────────────────
        mb.Entity<RootKey>()
          .Property(r => r.Cipher)
          .IsRequired();           // bytea

        mb.Entity<RootKey>()
          .Property(r => r.Tag)
          .IsRequired();           // bytea

        // ─── Usuario ───────────────────────────────────────────────
        mb.Entity<Usuario>()
          .HasIndex(u => u.Rut)
          .IsUnique();
    }
}
