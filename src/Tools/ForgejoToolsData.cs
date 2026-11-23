using System.ComponentModel;
using System.Text.Json.Serialization;

namespace mcp_peek_forgejo.Tools;

[JsonConverter(typeof(JsonStringEnumConverter))]
[Description("User search sort type")]
public enum UserSearchSort
{
    Oldest,
    Newest,
    Name,
    NameReverse,
    RecentUpdate,
    LeastUpdate,
}

[Description("User summary")]
public class UserSummary
{
    [Description("User ID")]
    public required long? Id { get; init; }

    [Description("Username identifier")]
    public required string? Name { get; init; }

    [Description("User full name")]
    public required string? FullName { get; init; }
}

[Description("User info")]
public class UserInfo : UserSummary
{
    [Description("User description")]
    public required string? Description { get; init; }

    [Description("Whether the user is active")]
    public required bool? IsActive { get; init; }

    [Description("Whether the user is an administrator")]
    public required bool? IsAdmin { get; init; }

    [Description("Whether login is prohibited")]
    public required bool? IsProhibit { get; init; }
}

/// <summary></summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
[Description("Repository search sort target")]
public enum RepoSearchSort
{
    Name,
    Created,
    Updated,
    ContentSize,
    GitSize,
    LfsSize,
    Id,
    Stars,
    Forks,
};


[JsonConverter(typeof(JsonStringEnumConverter))]
[Description("Search sort order")]
public enum RepoSearchOrder
{
    Asc,
    Desc,
};

[Description("Repository info")]
public class RepositoryInfo
{
    [Description("Repository ID")]
    public required long? Id { get; init; }

    [Description("Repository owner name")]
    public required string? OwnerName { get; init; }

    [Description("Repository name. Does not include the owner name.")]
    public required string? Name { get; init; }

    [Description("Repository full name. Includes the owner name and repository name.")]
    public required string? FullName { get; init; }

    [Description("Repository description")]
    public required string? Description { get; init; }

    [Description("Default branch")]
    public required string? DefaultBranch { get; init; }

    [Description("Whether the repository is private")]
    public required bool? IsPrivate { get; init; }

    [Description("Whether the repository is archived")]
    public required bool? IsArchived { get; init; }

    [Description("Whether the repository is forked")]
    public required bool? IsForked { get; init; }

    [Description("Repository size [KiB]")]
    public required long? Size { get; init; }

    [Description("Number of open issues")]
    public required long? IssuesCount { get; init; }

    [Description("Number of open PRs")]
    public required long? PullRequestCount { get; init; }

    [Description("Star count")]
    public required long? StarsCount { get; init; }

    [Description("Fork count")]
    public required long? ForksCount { get; init; }

    [Description("Web interface URL")]
    public required string? Url { get; init; }

    [Description("Content language")]
    public required string? Language { get; init; }

    [Description("Configured topics")]
    public required ICollection<string>? Topics { get; init; }

    [Description("Created timestamp")]
    public required DateTimeOffset? CreatedAt { get; init; }

    [Description("Updated timestamp")]
    public required DateTimeOffset? UpdatedAt { get; init; }
}

[Description("Branch info")]
public class BranchInfo
{
    [Description("Branch name")]
    public required string? Name { get; init; }

    [Description("Updated timestamp")]
    public required DateTimeOffset? Updated { get; init; }
}

[Description("Commit info")]
public class CommitInfo
{
    [Description("Author name")]
    public required string? AuthorName { get; init; }

    [Description("Committer name")]
    public required string? ComitterName { get; init; }

    [Description("Commit message")]
    public required string? Message { get; init; }

    [Description("Commit hash")]
    public required string Hash { get; init; }

    [Description("Changed file info")]
    public required CommitFileInfo[]? Files { get; init; }

    [Description("Commit timestamp")]
    public required DateTimeOffset Created { get; init; }
}

[Description("Commit file info")]
public class CommitFileInfo
{
    [Description("File path")]
    public required string? Path { get; init; }

    [Description("Update status")]
    public required string? Status { get; init; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
[Description("Commit diff type")]
public enum GitDiffType
{
    Diff,
    Patch,
}

[Description("Repository file info (entry)")]
public class RepoFileEntry
{
    [Description("File path")]
    public required string? Path { get; init; }

    [Description("File hash")]
    public required string? Sha { get; init; }

    [Description("File size")]
    public required long? Size { get; init; }
}

[Description("Repository file info (with file contents)")]
public class RepoFileInfo : RepoFileEntry
{
    [Description("File name")]
    public required string? Name { get; init; }

    [Description("File contents")]
    public required string? Contents { get; init; }
}

[Description("Repository file list info")]
public class RepoFileList
{
    [Description("List of file entries")]
    public required RepoFileEntry[]? Entries { get; init; }

    [Description("Number of file entries included in this list")]
    public required long? EntryCount { get; init; }

    [Description("Commit hash")]
    public required string? Hash { get; init; }

    [Description("Page number")]
    public required long? Page { get; init; }

    [Description("Whether the file list was truncated (due to limits, etc.). true indicates that a next page exists.")]
    public required bool? Truncated { get; init; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
[Description("Issue state")]
public enum RepoIssueState
{
    All,
    Open,
    Closed,
}

[JsonConverter(typeof(JsonStringEnumConverter))]
[Description("Issue type")]
public enum RepoIssueType
{
    Issue,
    PullRequest,
}

[JsonConverter(typeof(JsonStringEnumConverter))]
[Description("Issue sort type")]
public enum RepoIssueSort
{
    Latest,
    Relevance,
    Oldest,
    RecentUpdate,
    LeastUpdate,
    MostComment,
    LeastComment,
    NearDueDate,
    FarDueDate,
}

[Description("Issue/pull request info")]
public class IssueInfo
{
    [Description("Issue/pull request ID")]
    public required long? Id { get; init; }

    [Description("Issue/pull request number")]
    public required long? Number { get; init; }

    [Description("Issue type")]
    public required string? Type { get; init; }

    [Description("Title")]
    public required string? Title { get; init; }

    [Description("Author")]
    public required UserSummary? Author { get; init; }

    [Description("Assignees")]
    public required UserSummary[]? Assignees { get; init; }

    [Description("State")]
    public required string? State { get; init; }

    [Description("Body")]
    public required string? Body { get; init; }

    [Description("Comments count")]
    public required long? CommentsCount { get; init; }

    [Description("Created timestamp")]
    public required DateTimeOffset? CreatedAt { get; init; }

    [Description("Updated timestamp")]
    public required DateTimeOffset? UpdatedAt { get; init; }

    [Description("Due date")]
    public required DateTimeOffset? DueDate { get; init; }

    [Description("Closed timestamp")]
    public required DateTimeOffset? ClosedAt { get; init; }

    [Description("Labels")]
    public required IssueLabelInfo[]? Labels { get; init; }

    [Description("[Pull request info] Whether it is merged")]
    public required bool? IsMerged { get; init; }
}

[Description("Issue label info")]
public class IssueLabelInfo
{
    [Description("Label name")]
    public required string? Name { get; init; }

    [Description("Label description")]
    public required string? Description { get; init; }
}

[Description("Pull request info")]
public class PullRequestInfo
{
    [Description("Pull request ID")]
    public required long? Id { get; init; }

    [Description("Pull request number")]
    public required long? Number { get; init; }

    [Description("State")]
    public required string? State { get; init; }

    [Description("Title")]
    public required string? Title { get; init; }

    [Description("Body")]
    public required string? Body { get; init; }

    [Description("Author")]
    public required UserSummary? Author { get; init; }

    [Description("Assignees")]
    public required UserSummary[]? Assignees { get; init; }

    [Description("Changed file count")]
    public required long? ChangedFileCount { get; init; }

    [Description("Additions count")]
    public required long? Additions { get; init; }

    [Description("Deletions count")]
    public required long? Deletions { get; init; }

    [Description("Created timestamp")]
    public required DateTimeOffset? CreatedAt { get; init; }

    [Description("Updated timestamp")]
    public required DateTimeOffset? UpdatedAt { get; init; }

    [Description("Due date")]
    public required DateTimeOffset? DueDate { get; init; }

    [Description("Closed timestamp")]
    public required DateTimeOffset? ClosedAt { get; init; }

    [Description("Whether it is a draft")]
    public required bool? IsDraft { get; init; }

    [Description("Whether it is merged")]
    public required bool? IsMerged { get; init; }

    [Description("Whether it is mergeable")]
    public required bool? IsMergeable { get; init; }

    [Description("Merge base")]
    public required string? MergeBase { get; init; }

    [Description("Merge commit hash")]
    public required string? MergeCommitHash { get; init; }

    [Description("Base repository ID")]
    public required long? BaseRepoId { get; init; }

    [Description("Base branch ref")]
    public required string? BaseRef { get; init; }

    [Description("Base hash")]
    public required string? BaseHash { get; init; }

    [Description("Head repository ID")]
    public required long? HeadRepoId { get; init; }

    [Description("Head branch ref")]
    public required string? HeadRef { get; init; }

    [Description("Head hash")]
    public required string? HeadHash { get; init; }
}

[Description("Organization info")]
public class OrgInfo
{
    [Description("Organization ID")]
    public required long? Id { get; init; }

    [Description("Organization identifier")]
    public required string? Name { get; init; }

    [Description("Organization full name")]
    public required string? FullName { get; init; }

    [Description("Organization description")]
    public required string? Description { get; init; }
}
