using System.ComponentModel;
using ForgejoApiClient;
using ForgejoApiClient.Api;
using mcp_peek_forgejo.Services;
using Microsoft.AspNetCore.Authorization;
using ModelContextProtocol;
using ModelContextProtocol.Server;

namespace mcp_peek_forgejo.Tools;

/// <summary>Forgejo 参照ツール</summary>
[McpServerToolType]
internal class ForgejoTools
{
    public ForgejoTools(IAppConfigProvider configProvider)
    {
        var config = configProvider.McpConfig.Forgejo;
        this.client = new(new Uri(config.Url), config.ApiToken);
        this.configProvider = configProvider;
    }

    [McpServerTool(UseStructuredContent = ForgejoToolsConfigConstants.ToolStructuredOutput)]
    [Description($"Get the server version of {ForgejoToolsConfigConstants.InstanceNameTag}.")]
    public async Task<string?> GetServerVersionAsync(CancellationToken cancelToken = default)
    {
        try
        {
            var info = await this.client.Miscellaneous.GetVersionAsync(cancelToken);
            return info.version;
        }
        catch (ForgejoClientException ex)
        {
            throw new McpException(ex.Message);
        }
    }

    [McpServerTool(UseStructuredContent = ForgejoToolsConfigConstants.ToolStructuredOutput)]
    [Description($"Search users in {ForgejoToolsConfigConstants.InstanceNameTag}.")]
    [Authorize(Roles = $"{AppAuthRoles.Level1},{AppAuthRoles.Level2}")]
    public async Task<IEnumerable<UserInfo>> SearchUsersAsync(
        [Description("Search keyword")]
        string? query = default,
        [Description("Sort order for search results")]
        UserSearchSort? sort = default,
        [Description("Whether to target only active users")]
        bool onlyActive = true,
        [Description("Maximum number of items to return per request")]
        int? limit = default,
        [Description("Page number for pagination (1-based index; defaults to 1 if omitted).")]
        int? page = default,
        CancellationToken cancelToken = default)
    {
        var paging = limit.HasValue || page.HasValue ? new PagingOptions(limit: limit, page: page) : default;
        var result = await this.client.User.SearchAsync(
            q: query,
            sort: sort switch
            {
                UserSearchSort.Oldest => "oldest",
                UserSearchSort.Newest => "newest",
                UserSearchSort.Name => "alphabetically",
                UserSearchSort.NameReverse => "reversealphabetically",
                UserSearchSort.RecentUpdate => "recentupdate",
                UserSearchSort.LeastUpdate => "leastupdate",
                _ => null,
            },
            paging: paging,
            cancelToken: cancelToken
        );

        var users = result.data?.ToExportInfo(onlyActive) ?? throw new McpException("Failed to retrieve information");
        return users;
    }

    [McpServerTool(UseStructuredContent = ForgejoToolsConfigConstants.ToolStructuredOutput)]
    [Description($"Search repositories in {ForgejoToolsConfigConstants.InstanceNameTag}.")]
    [Authorize(Roles = $"{AppAuthRoles.Level1},{AppAuthRoles.Level2}")]
    public async Task<IEnumerable<RepositoryInfo>> SearchRepositoriesAsync(
        [Description("Search keyword. Matches against repository name or description.")]
        string? query = default,
        [Description("Target user ID to filter repositories by")]
        long? userId = default,
        [Description("Field to sort search results by")]
        RepoSearchSort? sort = default,
        [Description("Sort order for search results")]
        RepoSearchOrder? order = default,
        [Description("Maximum number of items to return per request")]
        int? limit = default,
        [Description("Page number for pagination (1-based index; defaults to 1 if omitted).")]
        int? page = default,
        CancellationToken cancelToken = default)
    {
        var paging = limit.HasValue || page.HasValue ? new PagingOptions(limit: limit, page: page) : default;
        var includePrivate = this.configProvider.McpConfig.Repository.IncludePrivate;
        var result = await this.client.Repository.SearchAsync(
            q: query,
            includeDesc: true,
            uid: userId,
            sort: sort switch
            {
                RepoSearchSort.Name => "alpha",
                RepoSearchSort.Created => "created",
                RepoSearchSort.Updated => "updated",
                RepoSearchSort.ContentSize => "size",
                RepoSearchSort.GitSize => "git_size",
                RepoSearchSort.LfsSize => "lfs_size",
                RepoSearchSort.Id => "id",
                RepoSearchSort.Stars => "stars",
                RepoSearchSort.Forks => "forks",
                _ => null,
            },
            order: order switch
            {
                RepoSearchOrder.Asc => "asc",
                RepoSearchOrder.Desc => "desc",
                _ => null,
            },
            @private: includePrivate,
            paging: paging,
            cancelToken: cancelToken
        );

        var repos = result.data?.ToExportInfo() ?? throw new McpException("Failed to retrieve information");
        return repos;
    }

    [McpServerTool(UseStructuredContent = ForgejoToolsConfigConstants.ToolStructuredOutput)]
    [Description("Get branch information for a repository.")]
    [Authorize(Roles = $"{AppAuthRoles.Level1},{AppAuthRoles.Level2}")]
    public async Task<IEnumerable<BranchInfo>> GetRepositoryBranchesAsync(
        [Description("Repository owner")]
        string repoOwner,
        [Description("Repository name")]
        string repoName,
        [Description("Maximum number of items to return per request (defaults to 10 if omitted).")]
        int? limit = default,
        [Description("Page number for pagination (1-based index; defaults to 1 if omitted).")]
        int? page = default,
        CancellationToken cancelToken = default)
    {
        var paging = new PagingOptions(page: page, limit: limit ?? 10);
        var result = await this.client.Repository.ListBranchesAsync(
            owner: repoOwner,
            repo: repoName,
            paging: paging,
            cancelToken: cancelToken
        );

        var commits = result?.ToExportInfo() ?? throw new McpException("Failed to retrieve information");
        return commits;
    }

    [McpServerTool(UseStructuredContent = ForgejoToolsConfigConstants.ToolStructuredOutput)]
    [Description("Get the file list for a repository.")]
    [Authorize(Roles = AppAuthRoles.Level2)]
    public async Task<RepoFileList> GetRepositoryFileListAsync(
        [Description("Repository owner")]
        string repoOwner,
        [Description("Repository name")]
        string repoName,
        [Description("Target branch or commit hash (defaults to the default branch if omitted).")]
        string? branch = null,
        [Description("Maximum number of items to return per request. The returned entry list may contain fewer items than this because some entries may be excluded.")]
        int? limit = default,
        [Description("Page number for pagination (1-based index; defaults to 1 if omitted).")]
        int? page = default,
        CancellationToken cancelToken = default)
    {
        var target = branch;
        if (target == null)
        {
            var repo = await this.client.Repository.GetAsync(
                owner: repoOwner,
                repo: repoName,
                cancelToken: cancelToken
            );
            target = repo.default_branch ?? "main";
        }

        var result = await this.client.Repository.GetTreeAsync(
            owner: repoOwner,
            repo: repoName,
            sha: target,
            recursive: true,
            page: page,
            per_page: limit,
            cancelToken: cancelToken
        );

        var fileList = result.ToExportInfo() ?? throw new McpException("Failed to retrieve information");
        return fileList;
    }

    [McpServerTool(UseStructuredContent = ForgejoToolsConfigConstants.ToolStructuredOutput)]
    [Description("Get file content from a repository as UTF-8 text.")]
    [Authorize(Roles = AppAuthRoles.Level2)]
    public async Task<RepoFileInfo> GetRepositoryFileContentAsync(
        [Description("Repository owner")]
        string repoOwner,
        [Description("Repository name")]
        string repoName,
        [Description("File path to retrieve")]
        string filePath,
        [Description("Target branch or commit hash")]
        string? branch = null,
        CancellationToken cancelToken = default)
    {
        var result = await this.client.Repository.GetContentAsync(
            owner: repoOwner,
            repo: repoName,
            filepath: filePath,
            @ref: branch,
            cancelToken: cancelToken
        );

        var file = result.ToExportInfo();
        return file;
    }

    [McpServerTool(UseStructuredContent = ForgejoToolsConfigConstants.ToolStructuredOutput)]
    [Description("Get commit information for a repository.")]
    [Authorize(Roles = AppAuthRoles.Level2)]
    public async Task<IEnumerable<CommitInfo>> GetRepositoryCommitsAsync(
        [Description("Repository owner")]
        string repoOwner,
        [Description("Repository name")]
        string repoName,
        [Description("Target branch or commit hash")]
        string? branch = null,
        [Description("Whether to include changed file information")]
        bool with_files = false,
        [Description("Maximum number of items to return per request (defaults to 10 if omitted).")]
        int? limit = default,
        [Description("Page number for pagination (1-based index; defaults to 1 if omitted).")]
        int? page = default,
        CancellationToken cancelToken = default)
    {
        var paging = new PagingOptions(page: page, limit: limit ?? 10);
        var result = await this.client.Repository.ListCommitsAsync(
            owner: repoOwner,
            repo: repoName,
            sha: branch,
            files: with_files,
            paging: paging,
            cancelToken: cancelToken
        );

        var commits = result?.ToExportInfo() ?? throw new McpException("Failed to retrieve information");
        return commits;
    }

    [McpServerTool(UseStructuredContent = ForgejoToolsConfigConstants.ToolStructuredOutput)]
    [Description("Get the diff for a repository commit.")]
    [Authorize(Roles = AppAuthRoles.Level2)]
    public async Task<string?> GetRepositoryCommitDiffAsync(
        [Description("Repository owner")]
        string repoOwner,
        [Description("Repository name")]
        string repoName,
        [Description("Target commit hash")]
        string hash,
        [Description("Diff format")]
        GitDiffType? type = default,
        CancellationToken cancelToken = default)
    {
        var result = await this.client.Repository.GetCommitDiffAsync(
            owner: repoOwner,
            repo: repoName,
            sha: hash,
            diffType: type switch
            {
                GitDiffType.Patch => "patch",
                _ => "diff",
            },
            cancelToken: cancelToken
        );

        return result;
    }

    [McpServerTool(UseStructuredContent = ForgejoToolsConfigConstants.ToolStructuredOutput)]
    [Description("Get issue or pull request information for a repository.")]
    [Authorize(Roles = $"{AppAuthRoles.Level1},{AppAuthRoles.Level2}")]
    public async Task<IEnumerable<IssueInfo>> GetRepositoryIssuesAsync(
        [Description("Repository owner")]
        string repoOwner,
        [Description("Repository name")]
        string repoName,
        [Description("Issue type: issue or pull request")]
        RepoIssueType? type = default,
        [Description("Issue state")]
        RepoIssueState? state = default,
        [Description("Search keyword")]
        string? query = default,
        [Description("Comma-separated list of labels to filter by")]
        string? labels = default,
        [Description("Filter to issues updated at or after the specified timestamp")]
        DateTimeOffset? since= default,
        [Description("Filter to issues updated at or before the specified timestamp")]
        DateTimeOffset? before= default,
        [Description("Issue author")]
        string? author = default,
        [Description("Issue assignee")]
        string? assignee = default,
        [Description("Sort order for search results")]
        RepoIssueSort? sort = default,
        [Description("Whether to omit the issue body from results")]
        bool withoutBody = false,
        [Description("Maximum number of items to return per request (defaults to 10 if omitted).")]
        int? limit = default,
        [Description("Page number for pagination (1-based index; defaults to 1 if omitted).")]
        int? page = default,
        CancellationToken cancelToken = default)
    {
        var paging = new PagingOptions(page: page, limit: limit ?? 10);
        var result = await this.client.Issue.ListAsync(
            owner: repoOwner,
            repo: repoName,
            q: query,
            labels: labels,
            state: state switch
            {
                RepoIssueState.All => "all",
                RepoIssueState.Open => "open",
                RepoIssueState.Closed => "closed",
                _ => null,
            },
            type: type switch
            {
                RepoIssueType.Issue => "issues",
                RepoIssueType.PullRequest => "pulls",
                _ => null,
            },
            sort: sort switch
            {
                RepoIssueSort.Latest => "latest",
                RepoIssueSort.Relevance => "relevance",
                RepoIssueSort.Oldest => "oldest",
                RepoIssueSort.RecentUpdate => "recentupdate",
                RepoIssueSort.LeastUpdate => "leastupdate",
                RepoIssueSort.MostComment => "mostcomment",
                RepoIssueSort.LeastComment => "leastcomment",
                RepoIssueSort.NearDueDate => "nearduedate",
                RepoIssueSort.FarDueDate => "farduedate",
                _ => null,
            },
            since: since,
            before: before,
            created_by: author,
            assigned_by: assignee,
            paging: paging,
            cancelToken: cancelToken
        );

        var commits = result?.ToExportInfo(withoutBody) ?? throw new McpException("Failed to retrieve information");
        return commits;
    }

    [McpServerTool(UseStructuredContent = ForgejoToolsConfigConstants.ToolStructuredOutput)]
    [Description("Get pull request information.")]
    [Authorize(Roles = $"{AppAuthRoles.Level2}")]
    public async Task<PullRequestInfo> GetRepositoryPullRequestAsync(
        [Description("Repository owner")]
        string repoOwner,
        [Description("Repository name")]
        string repoName,
        [Description("Pull request number")]
        long number,
        CancellationToken cancelToken = default)
    {
        var result = await this.client.Repository.GetPullRequestAsync(
            owner: repoOwner,
            repo: repoName,
            index: number,
            cancelToken: cancelToken
        );

        var pr = result.ToExportInfo();
        return pr;
    }

    [McpServerTool(UseStructuredContent = ForgejoToolsConfigConstants.ToolStructuredOutput)]
    [Description("Get the diff for a repository pull request.")]
    [Authorize(Roles = AppAuthRoles.Level2)]
    public async Task<string?> GetRepositoryPullRequestDiffAsync(
        [Description("Repository owner")]
        string repoOwner,
        [Description("Repository name")]
        string repoName,
        [Description("Pull request number")]
        long number,
        [Description("Diff format")]
        GitDiffType? type = default,
        CancellationToken cancelToken = default)
    {
        var result = await this.client.Repository.GetPullRequestDiffAsync(
            owner: repoOwner,
            repo: repoName,
            index: number,
            diffType: type switch
            {
                GitDiffType.Patch => "patch",
                _ => "diff",
            },
            cancelToken: cancelToken
        );

        return result;
    }

    [McpServerTool(UseStructuredContent = ForgejoToolsConfigConstants.ToolStructuredOutput)]
    [Description($"Get a list of organizations in {ForgejoToolsConfigConstants.InstanceNameTag}.")]
    [Authorize(Roles = $"{AppAuthRoles.Level1},{AppAuthRoles.Level2}")]
    public async Task<IEnumerable<OrgInfo>> GetOrganizationsAsync(
        [Description("Maximum number of items to return per request (defaults to 10 if omitted).")]
        int? limit = default,
        [Description("Page number for pagination (1-based index; defaults to 1 if omitted).")]
        int? page = default,
        CancellationToken cancelToken = default)
    {
        var paging = new PagingOptions(page: page, limit: limit ?? 10);
        var result = await this.client.Organization.ListAsync(
            paging: paging,
            cancelToken: cancelToken
        );

        var orgs = result?.ToExportInfo() ?? throw new McpException("情報を取得できなかった");
        return orgs;
    }

    [McpServerTool(UseStructuredContent = ForgejoToolsConfigConstants.ToolStructuredOutput)]
    [Description($"Get a list of repositories in an organization.")]
    [Authorize(Roles = $"{AppAuthRoles.Level1},{AppAuthRoles.Level2}")]
    public async Task<IEnumerable<RepositoryInfo>> GetOrganizationRepositoriesAsync(
        [Description("Organization name")]
        string orgName,
        [Description("Maximum number of items to return per request (defaults to 10 if omitted).")]
        int? limit = default,
        [Description("Page number for pagination (1-based index; defaults to 1 if omitted).")]
        int? page = default,
        CancellationToken cancelToken = default)
    {
        var paging = new PagingOptions(page: page, limit: limit ?? 10);
        var result = await this.client.Organization.ListRepositoriesAsync(
            org: orgName,
            paging: paging,
            cancelToken: cancelToken
        );

        var repos = result.ToExportInfo();
        return repos;
    }

    [McpServerTool(UseStructuredContent = ForgejoToolsConfigConstants.ToolStructuredOutput)]
    [Description($"Get a list of users in an organization.")]
    [Authorize(Roles = $"{AppAuthRoles.Level1},{AppAuthRoles.Level2}")]
    public async Task<IEnumerable<UserInfo>> GetOrganizationMembersAsync(
        [Description("Organization name")]
        string orgName,
        [Description("Maximum number of items to return per request (defaults to 10 if omitted).")]
        int? limit = default,
        [Description("Page number for pagination (1-based index; defaults to 1 if omitted).")]
        int? page = default,
        CancellationToken cancelToken = default)
    {
        var paging = new PagingOptions(page: page, limit: limit ?? 10);
        var result = await this.client.Organization.ListMembersAsync(
            org: orgName,
            paging: paging,
            cancelToken: cancelToken
        );

        var users = result.ToExportInfo(onlyActive: false);
        return users;
    }

    private readonly ForgejoClient client;
    private readonly IAppConfigProvider configProvider;
}
