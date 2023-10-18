using Consul;
using DotNetCore.CAP;
using Marten;
using Portal.Api.SoftwareApi.Entities;
using Portal.Api.UserApi.Entities;
using Portal.Api.UserApi.Events;

namespace Portal.Api.External.Producers;

public class KafkfaUserHandler
{
    public readonly ICapPublisher _publisher;
    public readonly IDocumentSession _session;

    public KafkfaUserHandler(ICapPublisher publisher, IDocumentSession session)
    {
        _publisher = publisher;
        _session = session;
    }

    public async Task ConsumesAsync(UserIssueCreated @event)
    {
       
        var software = await _session.LoadAsync<SoftwareEntity>(@event.SoftwareId);
        var user = await _session.LoadAsync<UserEntity>(@event.UserId);
        if(user is null || software is null) { return; }
        var message = new UserIssueCreatedPublicEvent
        {
            IssueId = @event.Id.ToString(),
            User = user.Identifier,
            SoftwareId = software.SourceId,
            Description = @event.Narrative,
        };
        await _publisher.PublishAsync("company.com.portal.userissuecreated", message);
    }
}


public class UserIssueCreatedPublicEvent
{
    public string IssueId { get; set; } = string.Empty;
    public string User { get; set; } = string.Empty; // sub
    public string SoftwareId { get; set; } = string.Empty; // the id that the "world" or owner knows this as.

    public string Description { get; set; } = string.Empty;
}