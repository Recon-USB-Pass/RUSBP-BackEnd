namespace Backend_Sistema_Central.DTOs;

public class LogEventDto
{
    public string EventId { get; set; } = default!;
    public string UserRut { get; set; } = default!;
    public string UsbSerial { get; set; } = default!;
    public string EventType { get; set; } = default!;
    public string Ip { get; set; } = default!;
    public string Mac { get; set; } = default!;
    public DateTime Timestamp { get; set; }
}