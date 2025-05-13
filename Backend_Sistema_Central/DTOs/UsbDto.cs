namespace Backend_Sistema_Central.DTOs;

public record UsbDto(
    string   Serial,
    string   Thumbprint,
    bool     Revocado,
    DateTime FechaAlta);
