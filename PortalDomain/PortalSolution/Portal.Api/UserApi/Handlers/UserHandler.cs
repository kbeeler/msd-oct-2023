using Marten;
using Portal.Api.UserApi.Commands;
using Portal.Api.UserApi.Entities;
using Portal.Api.UserApi.Events;
using System.Security.Claims;
using Wolverine;
using Wolverine.Runtime;

namespace Portal.Api.UserApi.Handlers;


// Handlers are classes that handle commands or events.
public class UserHandler
{
    private readonly IDocumentSession _session;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly IMessageBus _bus;
    public UserHandler(IDocumentSession session, IHttpContextAccessor contextAccessor, IMessageBus bus)
    {
        _session = session;
        _contextAccessor = contextAccessor;
        _bus = bus;
    }





    public async Task<UserEntity> HandleAsync(GetUser _)
    {
        var user = await LoadOrCreateUser();
        await _bus.PublishAsync(new UserLoggedIn(user.Id, DateTimeOffset.Now));
        return user;

    }

    public async Task<UserIssueEntity> HandleAsync(CreateUserIssue command)
    {
            var user = await LoadOrCreateUser();
            var issue = new UserIssueEntity(Guid.NewGuid(), command.SoftwareId, command.Narrative, DateTimeOffset.Now);

            user.PendingIssues.Add(issue);

            // TODO: Use the outbox. 
            await _bus.PublishAsync(new UserIssueCreated(issue.Id, issue.SoftwareId, user.Id, issue.Description, issue.created));

            _session.Store(user);

            await _session.SaveChangesAsync();
            return issue;
    }

    private async Task<UserEntity> LoadOrCreateUser()
    {
        var claim = _contextAccessor.HttpContext?.User.Claims.SingleOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        if (claim is null)
        {
            throw new ArgumentNullException("Somehow a bad user got here - Command can only handle an authenticated user");
        }

        var sub = claim.Value;
        var user = await _session.Query<UserEntity>().Where(u => u.Identifier == sub).SingleOrDefaultAsync();
        if (user is null)
        {
            // New User

            var newUser = new UserEntity(Guid.NewGuid(), DateTimeOffset.Now, sub, new List<UserIssueEntity>());
            _session.Store(newUser);
            await _session.SaveChangesAsync();
            await _bus.PublishAsync(new UserCreated(newUser.Id, DateTimeOffset.Now));
            return newUser;
        }
        else
        {
            // Returning User

            return user;
        }
    }
}
