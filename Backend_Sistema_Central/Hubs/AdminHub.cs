using Microsoft.AspNetCore.SignalR;

namespace Backend_Sistema_Central.Hubs;

public class AdminHub : Hub
{
    public async Task NotificarEstadoEmpleado(object data) =>
        await Clients.Group("Administradores").SendAsync("EmpleadoEstadoActualizado", data);

    public override Task OnConnectedAsync()
    {
        Groups.AddToGroupAsync(Context.ConnectionId, "Administradores");
        return base.OnConnectedAsync();
    }
}
