using mcp_peek_forgejo.Services;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Server;

namespace mcp_peek_forgejo.Tools;

/// <summary>ツールの構成に関する定数</summary>
public static class ForgejoToolsConfigConstants
{
    /// <summary>ツールが構造化出力を利用するか否か</summary>
    public const bool ToolStructuredOutput = true;

    /// <summary>インスタンス名の埋め込みマーカー</summary>
    public const string InstanceNameTag = "{{INSTANCE-NAME}}";
}

/// <summary>ツール構成カスタマイズクラス</summary>
/// <param name="configProvider">アプリケーション構成プロバイダ</param>
public class ForgejoToolsConfigurator(IAppConfigProvider configProvider) : IPostConfigureOptions<McpServerOptions>
{
    /// <inheritdoc />
    public void PostConfigure(string? name, McpServerOptions options)
    {
        // ツールコレクションが構成されていなければ何もせず
        if (options.ToolCollection == null) return;

        // インスタンス名の埋め込み設定がない場合は何もせず
        var instanceName = configProvider.McpConfig.Forgejo.InstanceName;
        if (string.IsNullOrWhiteSpace(instanceName)) return;

        // ツールの Description　の埋め込み文字列置き換え
        foreach (var tool in options.ToolCollection)
        {
            tool.ProtocolTool.Description = tool.ProtocolTool.Description?.Replace(ForgejoToolsConfigConstants.InstanceNameTag, instanceName);
        }
    }
}
