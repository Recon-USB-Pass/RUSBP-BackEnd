public record UsuarioDto(
    int Id,
    string Rut,
    string Nombre,
    string? Ip,
    string? Mac,
    string? SerialUsb,
    string? Depto,
    string? Email,
    string? Rol
);
