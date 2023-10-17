namespace Portal.Api.UserApi.Events;


public record UserIssueCreated(Guid IssueId, Guid SoftwareId, Guid UserId, string Narrative, DateTimeOffset CreatedOn);


public record UserLoggedIn(Guid UserId, DateTimeOffset When);

public record UserCreated(Guid UserId, DateTimeOffset When);

