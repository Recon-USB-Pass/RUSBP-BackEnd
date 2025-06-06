namespace Backend_Sistema_Central.DTOs
{
    public class AccesoLogDto
    {
        public string Rut { get; set; } = "";
        public string SerialUsb { get; set; } = "";
        public string Ip { get; set; } = "";
        public string Mac { get; set; } = "";
        public string? PcName { get; set; }
    }
}
