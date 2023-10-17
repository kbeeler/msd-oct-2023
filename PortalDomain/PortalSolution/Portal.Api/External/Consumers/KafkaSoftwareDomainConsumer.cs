using DotNetCore.CAP;
using Marten;
using Portal.Api.SoftwareApi.Comands;
using Portal.Api.SoftwareApi.Entities;
using Wolverine;

namespace Portal.Api.External.Consumers;

public interface IConsumeMessagesFromTheSoftwareDomain
{
    Task ProcessNewSoftware(SoftwareItemCreatedMessage message, CancellationToken token);
    Task ProcessRetiredSoftware(SoftwareItemRetiredMessage message, CancellationToken token);
}

[CapSubscribe("company.com.software")]
public class KafkaSoftwareDomainConsumer : IConsumeMessagesFromTheSoftwareDomain, ICapSubscribe
{
    private readonly IMessageBus _messageBus;
    private readonly IDocumentSession _session;

    public KafkaSoftwareDomainConsumer(IMessageBus messageBus, IDocumentSession session)
    {
        _messageBus = messageBus;
        _session = session;
    }

    [CapSubscribe("added", isPartial: true)]
    public async Task ProcessNewSoftware(SoftwareItemCreatedMessage message, CancellationToken token)
    {
        // create the thing you want to save from the message 

        // add it to the database.
        // party on.
        var command = new CreateSoftware($"{message.TitleName} by {message.Publisher}", message.Id);
        await _messageBus.SendAsync(command); // Just tell my other code to do it's thing.
    }

    [CapSubscribe("retired", isPartial: true)]
    public async Task ProcessRetiredSoftware(SoftwareItemRetiredMessage message, CancellationToken token)
    {
        var software = await _session.Query<SoftwareEntity>().Where(s => s.SourceId == message.Id).SingleOrDefaultAsync();
        if (software is not null)
        {
            var command = new RetireSoftware(software.Id);
            await _messageBus.SendAsync(command);
        }
        // TODO: Later.
    }
}


public class SoftwareItemCreatedMessage
{

    public string Id { get; set; } = string.Empty;

    public string TitleName { get; set; } = string.Empty;

    public string Publisher { get; set; } = string.Empty;

    public string SupportTech { get; set; } = string.Empty;

}

public class SoftwareItemRetiredMessage
{


    public string Id { get; set; } = string.Empty;

}