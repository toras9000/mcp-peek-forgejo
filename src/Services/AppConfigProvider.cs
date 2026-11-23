namespace mcp_peek_forgejo.Services;

/// <summary>MCPサービス設定</summary>
/// <param name="Secrets">MCPサービスアクセストークン</param>
public record ServiceConfig(IReadOnlyDictionary<string, string>? Secrets = null);

/// <summary>Forgejo 設定</summary>
/// <param name="Url">Forgejo サービスのURL</param>
/// <param name="ApiToken">APIトークン</param>
/// <param name="InstanceName">インスタンスの任意名称。Tools 説明に埋め込まれる。</param>
public record ForgejoConfig(string Url, string ApiToken, string? InstanceName = null);

/// <summary>リポジトリ設定</summary>
/// <param name="IncludePrivate">リポジトリ一覧にプライベートリポジトリを含めるかどうか</param>
public record RepositoryConfig(bool IncludePrivate);

/// <summary>MCP動作設定</summary>
/// <param name="Service">MCPサービス設定</param>
/// <param name="Forgejo">Forgejo 設定</param>
/// <param name="Repository">リポジトリ設定</param>
public record McpConfig(ServiceConfig Service, ForgejoConfig Forgejo, RepositoryConfig Repository);

/// <summary>アプリケーション構成プロバイダ</summary>
public interface IAppConfigProvider
{
    /// <summary>MCP動作設定</summary>
    public McpConfig McpConfig { get; }
}

/// <summary>アプリケーション構成プロバイダ 実装</summary>
public class AppConfigProvider : IAppConfigProvider
{
    // 構築
    #region コンストラクタ
    /// <summary>依存サービスを注入するコンストラクタ</summary>
    /// <param name="config">構成インタフェース</param>
    public AppConfigProvider(IConfiguration config)
    {
        this.McpConfig = config.GetSection("Mcp").Get<McpConfig>() ?? throw new InvalidDataException($"Illegal configuration");
    }
    #endregion

    // 公開プロパティ
    #region IAppConfigProvider I/Fs
    /// <inheritdoc />
    public McpConfig McpConfig { get; }
    #endregion
}
