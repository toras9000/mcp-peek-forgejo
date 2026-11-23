using System.Buffers;
using System.Buffers.Text;
using System.Text;
using ForgejoApiClient.Api;

namespace mcp_peek_forgejo.Tools;

/// <summary>MCPデータ関連拡張メソッド</summary>
public static class ForgejoToolsDataExtensions
{
    /// <summary>ユーザ情報をMCP公開データに変換する</summary>
    /// <param name="users">APIユーザ情報シーケンス</param>
    /// <param name="onlyActive">アクティブなユーザのみを対象とするか否か</param>
    /// <returns>MCPユーザ情報シーケンス</returns>
    public static IEnumerable<UserInfo> ToExportInfo(this IEnumerable<User> users, bool onlyActive)
    {
        return users
            .Where(u => !onlyActive || u.active == true)
            .Select(u => u.ToExportInfo());
    }

    /// <summary>ユーザ情報をMCP公開データに変換する</summary>
    /// <param name="user">APIユーザ情報</param>
    /// <returns>MCPユーザ情報</returns>
    public static UserInfo ToExportInfo(this User user)
    {
        return new UserInfo
        {
            Id = user.id,
            Name = user.login,
            FullName = user.full_name,
            Description = user.description,
            IsActive = user.active,
            IsAdmin = user.is_admin,
            IsProhibit = user.prohibit_login,
        };
    }

    /// <summary>ユーザ情報をMCP公開データに変換する</summary>
    /// <param name="user">APIユーザ情報</param>
    /// <returns>MCPユーザサマリ</returns>
    public static UserInfo ToExportSummary(this User user)
    {
        return new UserInfo
        {
            Id = user.id,
            Name = user.login,
            FullName = user.full_name,
            Description = user.description,
            IsActive = user.active,
            IsAdmin = user.is_admin,
            IsProhibit = user.prohibit_login,
        };
    }

    /// <summary>リポジトリ情報をMCP公開データに変換する</summary>
    /// <param name="repos">APIリポジトリ情報シーケンス</param>
    /// <returns>MCPリポジトリ情報シーケンス</returns>
    public static IEnumerable<RepositoryInfo> ToExportInfo(this IEnumerable<Repository> repos)
    {
        return repos.Select(r => r.ToExportInfo());
    }

    /// <summary>リポジトリ情報をMCP公開データに変換する</summary>
    /// <param name="repo">APIリポジトリ情報</param>
    /// <returns>MCPリポジトリ情報</returns>
    public static RepositoryInfo ToExportInfo(this Repository repo)
    {
        return new RepositoryInfo
        {
            Id = repo.id,
            OwnerName = repo.owner?.login,
            Name = repo.name,
            FullName = repo.full_name,
            DefaultBranch = repo.default_branch,
            Description = repo.description,
            IsPrivate = repo.@private,
            IsArchived = repo.archived,
            IsForked = repo.fork,
            ForksCount = repo.forks_count,
            StarsCount = repo.stars_count,
            Size = repo.size,
            IssuesCount = repo.open_issues_count,
            PullRequestCount = repo.open_pr_counter,
            Url = repo.html_url,
            Language = repo.language,
            Topics = repo.topics,
            CreatedAt = repo.created_at,
            UpdatedAt = repo.updated_at
        };
    }

    /// <summary>ブランチ情報をMCP公開データに変換する</summary>
    /// <param name="branches">APIブランチ情報シーケンス</param>
    /// <returns>MCPブランチ情報シーケンス</returns>
    public static IEnumerable<BranchInfo> ToExportInfo(this IEnumerable<Branch> branches)
    {
        return branches.Select(b => new BranchInfo
        {
            Name = b.name,
            Updated = b.commit?.timestamp,
        });
    }

    /// <summary>コミット情報をMCP公開データに変換する</summary>
    /// <param name="commits">APIコミット情報シーケンス</param>
    /// <returns>MCPコミット情報シーケンス</returns>
    public static IEnumerable<CommitInfo> ToExportInfo(this IEnumerable<Commit> commits)
    {
        return commits.Select(c => new CommitInfo
        {
            AuthorName = c.commit?.author?.name,
            ComitterName = c.commit?.committer?.name,
            Message = c.commit?.message,
            Hash = c.sha ?? "",
            Files = c.files?.Select(f => new CommitFileInfo() { Path = f.filename, Status = f.status, }).ToArray() ?? [],
            Created = c.created ?? default,
        });
    }

    /// <summary>Gitツリー情報をMCP公開データに変換する</summary>
    /// <param name="tree">APIGitツリー情報</param>
    /// <returns>MCPファイルリスト情報</returns>
    public static RepoFileList ToExportInfo(this GitTreeResponse tree)
    {
        var entries = tree.tree
            ?.Where(f => f.type == "blob")
            .Select(f => new RepoFileEntry
            {
                Path = f.path,
                Sha = f.sha,
                Size = f.size,
            })
            .ToArray();

        return new()
        {
            Entries = entries,
            EntryCount = entries?.Length,
            Hash = tree.sha,
            Page = tree.page,
            Truncated = tree.truncated,
        };
    }

    /// <summary>ファイル内容情報をMCP公開データに変換する</summary>
    /// <param name="file">APIファイル内容情報</param>
    /// <returns>MCPファイル内容情報</returns>
    public static RepoFileInfo ToExportInfo(this ContentsResponse file)
    {
        var contents = file.content;
        if (contents != null)
        {
            var requireLen = Base64.GetMaxDecodedLength(contents.Length);
            var bytesBuffer = default(byte[]);
            var textBuffer = default(char[]);
            try
            {
                bytesBuffer = ArrayPool<byte>.Shared.Rent(requireLen);
                var bytesLen = Base64.DecodeFromChars(contents, bytesBuffer);
                var bytesBin = bytesBuffer.AsSpan(0, bytesLen);
                var textEncoding = Encoding.UTF8;
                var textWriter = textEncoding.GetDecoder();
                var textMaxLen = textEncoding.GetMaxCharCount(bytesBin.Length);
                textBuffer = ArrayPool<char>.Shared.Rent(textMaxLen);
                textWriter.Convert(bytesBin, textBuffer, flush: true, out _, out var written, out _);
                contents = textBuffer.AsSpan(0, written).ToString();
            }
            finally
            {
                if (bytesBuffer != null) ArrayPool<byte>.Shared.Return(bytesBuffer);
                if (textBuffer != null) ArrayPool<char>.Shared.Return(textBuffer);
            }
        }

        return new RepoFileInfo
        {
            Name = file.name,
            Path = file.path,
            Sha = file.sha,
            Size = file.size,
            Contents = contents,
        };
    }

    /// <summary>イシュー情報をMCP公開データに変換する</summary>
    /// <param name="issues">APIイシュー情報シーケンス</param>
    /// <param name="withoutBody">本文を除外するか否か</param>
    /// <returns>MCPイシュー情報シーケンス</returns>
    public static IEnumerable<IssueInfo> ToExportInfo(this IEnumerable<Issue> issues, bool withoutBody)
    {
        return issues.Select(i => i.ToExportInfo(withoutBody));
    }

    /// <summary>イシュー情報をMCP公開データに変換する</summary>
    /// <param name="issue">APIイシュー情報</param>
    /// <param name="withoutBody">本文を除外するか否か</param>
    /// <returns>MCPイシュー情報</returns>
    public static IssueInfo ToExportInfo(this Issue issue, bool withoutBody)
    {
        return new IssueInfo
        {
            Id = issue.id,
            Number = issue.number,
            Type = issue.pull_request == null ? "issue" : "pull_request",
            Title = issue.title,
            Author = issue.user?.ToExportSummary(),
            Assignees = issue.assignees?.Select(a => a.ToExportSummary()).ToArray(),
            State = issue.state,
            Labels = issue.labels
                ?.Select(l => new IssueLabelInfo
                {
                    Name = l.name,
                    Description = l.description,
                })
                .ToArray(),
            Body = withoutBody ? default : issue.body,
            CommentsCount = issue.comments,
            CreatedAt = issue.created_at,
            UpdatedAt = issue.updated_at,
            DueDate = issue.due_date,
            ClosedAt = issue.closed_at,
            IsMerged = issue.pull_request?.merged,
        };
    }

    /// <summary>プルリクエスト情報をMCP公開データに変換する</summary>
    /// <param name="file">APIプルリクエスト情報</param>
    /// <returns>MCPプルリクエスト情報</returns>
    public static PullRequestInfo ToExportInfo(this PullRequest pr)
    {
        return new PullRequestInfo
        {
            Id = pr.id,
            Number = pr.number,
            State = pr.state,
            Title = pr.title,
            Body = pr.body,
            Author = pr.user?.ToExportSummary(),
            Assignees = pr.assignees?.Select(a => a.ToExportSummary()).ToArray(),
            ChangedFileCount = pr.changed_files,
            Additions = pr.additions,
            Deletions = pr.deletions,
            CreatedAt = pr.created_at,
            UpdatedAt = pr.updated_at,
            DueDate = pr.due_date,
            ClosedAt = pr.closed_at,
            IsDraft = pr.draft,
            IsMerged = pr.merged,
            IsMergeable = pr.mergeable,
            MergeBase = pr.merge_base,
            MergeCommitHash = pr.merge_commit_sha,
            BaseRepoId = pr.@base?.repo_id,
            BaseRef = pr.@base?.@ref,
            BaseHash = pr.@base?.sha,
            HeadRepoId = pr.head?.repo_id,
            HeadRef = pr.head?.@ref,
            HeadHash = pr.head?.sha,
        };
    }

    /// <summary>組織情報をMCP公開データに変換する</summary>
    /// <param name="orgs">API組織情報シーケンス</param>
    /// <returns>MCP組織情報シーケンス</returns>
    public static IEnumerable<OrgInfo> ToExportInfo(this IEnumerable<Organization> orgs)
    {
        return orgs.Select(o => new OrgInfo
        {
            Id = o.id,
            Name = o.name,
            FullName = o.full_name,
            Description = o.description,
        });
    }


}
