using Marten;
using Microsoft.AspNetCore.SignalR;
using Portal.Api.SoftwareApi.Comands;
using Portal.Api.SoftwareApi.Entities;
using Portal.Api.SoftwareApi.Events;

namespace Portal.Api.SoftwareApi.Handlers;

public class SoftwareSignalREventHandler
{

    public static async Task ConsumesAsync(SoftwareCreated @event, IHubContext<SoftwareHub> hub)
    {
        await hub.Clients.All.SendAsync("software-added", @event.Software);
    }

    public static async Task ConsumesAsync(RetireSoftware @event, IHubContext<SoftwareHub> hub, IDocumentSession session)
    {
        var software = await session.LoadAsync<SoftwareEntity>(@event.Id);
        await hub.Clients.All.SendAsync("software-retired", software);
    }
}
