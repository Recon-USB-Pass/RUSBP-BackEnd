namespace Backend_Sistema_Central.DTOs;

// 1. Envío de certificado del USB al servidor
public record UsbVerificationDto(string Serial, string CertPem);

// 2. Login basado en desafío y firma
public record LoginDto(string Serial, string SignatureBase64, string Pin, string MacAddress);

// 3. Lote de logs enviados por un agente
public record LogEntryDto(string Serial, string TipoEvento, DateTime FechaHora, string? Detalle);

/*

namespace Backend_Sistema_Central.DTOs;
public record LoginDto(string Pin, string MacAddress);


*/