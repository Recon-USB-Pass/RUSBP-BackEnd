namespace Backend_Sistema_Central.Models;

public class DispositivoUSB
{
    public int Id { get; set; }
    public string Serial { get; set; } = default!;
    public string CertThumbprint { get; set; } = default!;
    public DateTime FechaAsignacion { get; set; }
    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = default!;
}
