namespace Backend_Sistema_Central.DTOs;

public record UsuarioDto
(
    int    Id,
    string Rut,
    string Nombre,
    string? Ip,
    string? Mac,
    string? SerialUsb
);
