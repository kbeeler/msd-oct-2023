namespace Portal.Api.UserApi.Entities;

public record UserEntity(Guid Id, DateTimeOffset CreatedOn, string Identifier, List<UserIssueEntity> PendingIssues);



public record UserIssueEntity(Guid Id, Guid SoftwareId, string Description, DateTimeOffset created);


/*export type UserIssue = {
  issueId: string;
  softwareId: string;
  description: string;
  created: string;
};*/