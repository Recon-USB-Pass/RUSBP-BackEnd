public record UsuarioDto(
    int Id,
    string Rut,
    string Nombre,
    string? Ip,
    string? Mac,
    string? Depto,
    string? Email,
    string? Rol,
    string? Serial     // <--- Nuevo campo, separado por coma
);
