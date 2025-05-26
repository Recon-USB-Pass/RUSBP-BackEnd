// Backend_Sistema_Central/Program.cs
using Backend_Sistema_Central;
using Backend_Sistema_Central.Models;
using Backend_Sistema_Central.Hubs;
using Backend_Sistema_Central.Services;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var pfxPass = Environment.GetEnvironmentVariable("PFX_PASSWORD") ?? "changeit";

/*──────────────────── 1. Kestrel ───────────────────*/
builder.WebHost.ConfigureKestrel(k =>
{
    k.ListenAnyIP(8080);    // HTTP dev

    var pfx = Path.Combine(AppContext.BaseDirectory, "certs", "server.pfx");
    if (File.Exists(pfx) && !string.IsNullOrWhiteSpace(pfxPass))
    {
        k.ListenAnyIP(8443, l =>
        {
            l.UseHttps(pfx, pfxPass, https =>
            {
                https.ClientCertificateMode = ClientCertificateMode.AllowCertificate;
            });
        });
    }
});

/*──────────────────── 2. Servicios ─────────────────*/
builder.Services.AddDbContext<ApplicationDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddSignalR();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<ICertificateValidator, CertificateValidator>();
builder.Services.AddScoped<IUsbStatusService, UsbStatusService>();
builder.Services.AddSingleton<IChallengeService, ChallengeService>();

builder.Services.AddControllers()
       .AddJsonOptions(o =>
           o.JsonSerializerOptions.ReferenceHandler =
               System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

/*──────────────────── 3. Middleware ────────────────*/
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();
app.MapHub<AdminHub>("/hub/admin");

/*──────────────────── 4. Migración automática ──────*/
if (args.Contains("--migrate-and-run"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

/*──────────────────── Info consola ─────────────────*/
var containerIp = System.Net.Dns.GetHostName();
var hostIp      = Environment.GetEnvironmentVariable("HOST_IP") ?? "localhost";

Console.WriteLine("───────────────────────────────────────────────");
Console.WriteLine("🟢  RUSBP-API listo:");
Console.WriteLine($"   • IP contenedor : {containerIp}");
Console.WriteLine($"   • IP host       : {hostIp}");
Console.WriteLine($"   • URL externa   : http://{hostIp}:8080   (swagger)");
Console.WriteLine($"                    https://{hostIp}:8443");
Console.WriteLine("───────────────────────────────────────────────");

app.Run();
