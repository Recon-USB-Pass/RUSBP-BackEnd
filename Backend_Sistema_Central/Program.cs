using Backend_Sistema_Central;
using Backend_Sistema_Central.Hubs;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ──────────────────────────────────────────────
// 1. Kestrel:     SOLO HTTP 8080 (sin TLS)
// ──────────────────────────────────────────────
builder.WebHost.ConfigureKestrel(k =>
{
    k.ListenAnyIP(8080);      // http://localhost:8080
});

// ──────────────────────────────────────────────
// 2. Servicios
// ──────────────────────────────────────────────
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddSignalR();
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
