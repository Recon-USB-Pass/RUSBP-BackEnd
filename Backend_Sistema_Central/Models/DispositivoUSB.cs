// Backend_Sistema_Central/Models/DispositivoUSB.cs
namespace Backend_Sistema_Central.Models;

public class DispositivoUSB
{
    public int      Id         { get; set; }
    public string   Serial     { get; set; } = "";
    public string?  Thumbprint { get; set; }
    public DateTime FechaAlta  { get; set; }
    public bool     Revoked    { get; set; }

    // ─── Relaciones ───────────────────────────────────────────────
    public int?     UsuarioId  { get; set; }
    public Usuario? Usuario    { get; set; }

    // ─── Clave envuelta ───────────────────────────────────────────
    public byte[]   RpCipher   { get; set; } = default!;   // AES-GCM ciphertext
    public byte[]   RpTag      { get; set; } = default!;   // AES-GCM auth-tag
    public UsbRole  Rol        { get; set; }               // Root/Admin/Employee
}
