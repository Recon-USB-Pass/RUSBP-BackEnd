using Backend_Sistema_Central;
using Backend_Sistema_Central.Hubs;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Server.Kestrel.Https;  // ← AÑADIR
using Backend_Sistema_Central.Services; 

var builder = WebApplication.CreateBuilder(args);
var pfxPass = Environment.GetEnvironmentVariable("PFX_PASSWORD") ?? "changeit";

// ──────────────────────────────────────────────
// 1. Kestrel:     HTTP 8080 + HTTPS 8443
// ──────────────────────────────────────────────
builder.WebHost.ConfigureKestrel(k =>
{
    // HTTP Dev
    k.ListenAnyIP(8080);

    // HTTPS Prod
    var pfxPath = Path.Combine(AppContext.BaseDirectory, "certs", "server.pfx");

    if (!File.Exists(pfxPath))
    {
        Console.WriteLine($"❌ No se encontró el certificado: {pfxPath}");
        return; // 🔕 Kestrel sigue solo con HTTP
    }

    if (string.IsNullOrWhiteSpace(pfxPass))
    {
        Console.WriteLine("❌ Contraseña del .pfx vacía o no definida (PFX_PASSWORD)");
        return;
    }

    k.ListenAnyIP(8443, l =>
    {
        l.UseHttps(pfxPath, pfxPass, https =>
        {
            https.ClientCertificateMode = ClientCertificateMode.AllowCertificate;
        });
    });
});

// ──────────────────────────────────────────────
// 2. Servicios
// ──────────────────────────────────────────────
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddSignalR();
//new
builder.Services.AddMemoryCache();
builder.Services.AddScoped<ICertificateValidator, CertificateValidator>();
builder.Services.AddScoped<IUsbStatusService, UsbStatusService>();


builder.Services.AddSingleton<IChallengeService, ChallengeService>();
//new_end
builder.Services
       .AddControllers()
       .AddJsonOptions(o =>
       {
           o.JsonSerializerOptions.ReferenceHandler =
               System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
       });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ──────────────────────────────────────────────
// 3. Middleware
// ──────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
//new
app.UseHttpsRedirection();
//new_end
app.UseAuthorization();
app.Use(async (ctx, next) =>
{
    var logPath = Path.Combine(AppContext.BaseDirectory, "route_hits.log");
    File.AppendAllText(logPath, $"{DateTime.UtcNow:u}  {ctx.Request.Method} {ctx.Request.Path}\n");
    await next();
});

app.MapControllers();
app.MapHub<AdminHub>("/hub/admin");

// ──────────────────────────────────────────────
// 4. Ejecutar migraciones al arrancar
//    (se llama con  --migrate-and-run  desde entrypoint.sh)
// ──────────────────────────────────────────────
if (args.Contains("--migrate-and-run"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}
Console.WriteLine("Hash aqui CTM");
Console.WriteLine(BCrypt.Net.BCrypt.HashPassword("1234"));
Console.WriteLine("Hash aqui CTM");

app.Run();
