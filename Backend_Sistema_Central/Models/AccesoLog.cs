namespace Backend_Sistema_Central.Models
{
    public class AccesoLog
    {
        public int Id { get; set; }
        public string Rut { get; set; } = "";
        public string SerialUsb { get; set; } = "";
        public string Ip { get; set; } = "";
        public string Mac { get; set; } = "";
        public DateTime Fecha { get; set; }
        public string? PcName { get; set; }
    }
}
