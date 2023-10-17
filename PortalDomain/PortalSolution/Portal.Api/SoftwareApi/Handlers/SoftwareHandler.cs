using Marten;

using Portal.Api.SoftwareApi.Comands;
using Portal.Api.SoftwareApi.Entities;
using Portal.Api.SoftwareApi.Events;
using Wolverine;

namespace Portal.Api.SoftwareApi.Handlers;

public static class SoftwareHandler
{

    public static async Task<SoftwareEntity> Handle(CreateSoftware command, IDocumentSession session, IMessageBus bus, CancellationToken token)
    {
        var software = new SoftwareEntity(Guid.NewGuid(), command.Title, command.SourceId);
        session.Store(software);
        await session.SaveChangesAsync();
        var @event = new SoftwareCreated(software);
        await bus.PublishAsync(@event);
        return software;
    }

    public static async Task<IReadOnlyList<SoftwareEntity>> HandleAsync(GetSoftware _, IDocumentSession session)
    {
        var software = await GetActiveSoftware(session).ToListAsync();
        return software;
    }

    public static async Task<SoftwareEntity?> HandleAsync(GetSoftwareById command, IDocumentSession session)
    {
        var software = await GetActiveSoftware(session).SingleOrDefaultAsync(s => s.Id == command.Id);
        return software;
    }
    public static async Task HandleAsync(RetireSoftware command, IDocumentSession session)
    {
        var myCopyOfTheSoftware = await session.LoadAsync<SoftwareEntity>(command.Id);
        if (myCopyOfTheSoftware is not null)
        {
            myCopyOfTheSoftware = myCopyOfTheSoftware with { Retired = true };
            session.Store(myCopyOfTheSoftware);
            await session.SaveChangesAsync();
        }// TODO: talk about race conditions.



    }

    private static IQueryable<SoftwareEntity> GetActiveSoftware(IDocumentSession session)
    {
        return session.Query<SoftwareEntity>().Where(s => s.Retired == false);
    }

}