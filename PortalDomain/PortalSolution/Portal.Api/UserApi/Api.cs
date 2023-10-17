using Portal.Api.UserApi.Commands;
using Portal.Api.UserApi.Entities;
using Wolverine;

namespace Portal.Api.UserApi;

public static class Api
{
    public static RouteGroupBuilder MapUserApi(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/user");
        group.RequireCors("cors");
        group.RequireAuthorization();

        group.MapGet("/", async (IMessageBus bus) =>
        {

            // "When a user hits this endpoint, we need to return the aggregate (what we have about the user)
            // If the user exists, we will just return the saved UserEntity in the response.
            // But what if they DON'T exist? (Well, the DO exist, because we have their token, but we've
            // never met them.)
            // In that case, we will create a UserEntity for them, brand new, and just return that.
            // We should also publish some (local) events about this. Could be usesful.
            // Small Brain Version - Just do this all in this anonymous function (or your controller method)
            // Big brain version - create a "UserManager Service" and inject that, and call that.
            // Galaxy Brain Version - Stick with commands, events, all that stuff. 
            // var response = userManager.GetUserAsync();
            var response = await bus.InvokeAsync<UserEntity>(new GetUser());
            return TypedResults.Ok(response);
        });

        group.MapPost("/issues", async (CreateUserIssue command, IMessageBus bus) =>
        {
            var response = await bus.InvokeAsync<UserIssueEntity>(command);
            return TypedResults.Ok(response);
        });


        return group;
    }
}


