using Backend_Sistema_Central;
using Backend_Sistema_Central.Hubs;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Server.Kestrel.Https;  // ← AÑADIR
using Backend_Sistema_Central.Services; 

var builder = WebApplication.CreateBuilder(args);
var pfxPass = Environment.GetEnvironmentVariable("PFX_PASSWORD") ?? "changeit";

// ──────────────────────────────────────────────
// 1. Kestrel:     SOLO HTTP 8080 (sin TLS)
// ──────────────────────────────────────────────
builder.WebHost.ConfigureKestrel(k =>
{
    k.ListenAnyIP(8080);                                          // Dev
    k.ListenAnyIP(8443, l =>                                      // Prod
    {
        l.UseHttps("certs/server.pfx", pfxPass, https =>
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
builder.Services.AddSingleton<IChallengeService, ChallengeService>();
//new_end
builder.Services.AddControllers();
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

app.Run();
