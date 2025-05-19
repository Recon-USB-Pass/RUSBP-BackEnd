using Backend_Sistema_Central.Models;
using System.Linq;

namespace Backend_Sistema_Central.Services
{
    public interface IUsbStatusService
    {
        bool IsUsbOnline(string serial);
    }

    public class UsbStatusService(ApplicationDbContext db) : IUsbStatusService
    {
        private readonly ApplicationDbContext _db = db;

        public bool IsUsbOnline(string serial)
        {
            string? last = _db.Logs
                            .Where(l => l.UsbSerial == serial)
                            .OrderByDescending(l => l.Timestamp)
                            .Select(l => l.EventType)
                            .FirstOrDefault();

            return last == "conexión";   // usa tu string real
        }
    }

}

