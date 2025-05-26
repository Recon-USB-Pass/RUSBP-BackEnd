// Dtos/UsbRecoverDto.cs
namespace Backend_Sistema_Central.Dtos;
using Backend_Sistema_Central.Models;

public class UsbRecoverRequestDto
{
    public string Serial     { get; set; } = "";
    public UsbRole AgentType { get; set; }          // 0-root / 1-admin / 2-employee
}

public class UsbRecoverResponseDto
{
    public string Cipher { get; set; } = "";        // Base64-cipher (RP_serial)
    public string Tag    { get; set; } = "";        // Base64-tag
    public UsbRole Rol   { get; set; }              // Rol real del USB
}
