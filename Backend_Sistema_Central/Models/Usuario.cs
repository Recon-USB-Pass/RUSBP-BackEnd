namespace Backend_Sistema_Central.Models;

public class Usuario
{
    public int Id { get; set; }
    public string Rut { get; set; } = default!;
    public string Nombre { get; set; } = default!;
    public string PinHash { get; set; } = default!;
    public ICollection<DispositivoUSB> USBs { get; set; } = [];

    public string Ip { get; set; } = "";
    public string Mac { get; set; } = "";
}
