namespace Backend_Sistema_Central.Models;

public class DispositivoUSB
{
    public int Id             { get; set; }
    public string Serial      { get; set; } = default!;          // PK lógica
    public string Thumbprint  { get; set; } = default!;          // Huella del certificado X509
    public DateTime FechaAlta { get; set; }
    public bool   Revoked     { get; set; }                      // Si se revoca el USB

    // Relación
    public int     UsuarioId  { get; set; }
    public Usuario Usuario    { get; set; } = default!;
}

