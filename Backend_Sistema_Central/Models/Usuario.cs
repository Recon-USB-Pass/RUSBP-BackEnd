namespace Backend_Sistema_Central.Models;

public class Usuario
{
    public int Id { get; set; }
    public string Rut { get; set; } = default!;
    public string Nombre { get; set; } = default!;
    public string PinHash { get; set; } = default!;
    public string? Depto { get; set; }
    public string? Email { get; set; }
    public string? Rol { get; set; }
    public ICollection<DispositivoUSB> USBs { get; set; } = new List<DispositivoUSB>();
    public string? Ip { get; set; }
    public string? Mac { get; set; }
    
}
