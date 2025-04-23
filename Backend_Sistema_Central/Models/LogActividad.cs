namespace Backend_Sistema_Central.Models;

public class LogActividad
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string TipoEvento { get; set; } = default!;
    public string IP { get; set; } = default!;
    public string MAC { get; set; } = default!;
    public DateTime FechaHora { get; set; }
    public string? Detalle { get; set; }
}
