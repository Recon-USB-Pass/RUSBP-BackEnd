// Backend_Sistema_Central/Models/RootKey.cs
namespace Backend_Sistema_Central.Models;

public class RootKey
{
    public int      Id        { get; set; }
    public byte[]   Cipher    { get; set; } = default!;   // RP_root cifrada (IV fijo)
    public byte[]   Tag       { get; set; } = default!;   // Tag de GCM
    public DateTime FechaAlta { get; set; }
}
