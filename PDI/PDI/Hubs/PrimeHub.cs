using Microsoft.AspNetCore.SignalR;


namespace PDI.Hubs;
public class PrimeHub : Hub
{
    public async Task JoinGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        Console.WriteLine($"Connection {Context.ConnectionId} joined group {groupName}");
    }
}