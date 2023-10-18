using FluentValidation;

namespace Portal.Api.UserApi.Commands;

public record GetUser();
public record CreateUserIssue(Guid SoftwareId, string Narrative);

public class CreateUserIssueValidator : AbstractValidator<CreateUserIssue>
{
    public CreateUserIssueValidator()
    {
        RuleFor(m => m.SoftwareId).NotEmpty().NotNull().NotEqual(Guid.Empty);
        RuleFor(m => m.Narrative).NotEmpty().NotNull().MinimumLength(5).MaximumLength(255);
    }
}

