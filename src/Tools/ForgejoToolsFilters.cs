using ForgejoApiClient;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace mcp_peek_forgejo.Tools;

/// <summary>ForgejoTools用のMCPフィルタ定義</summary>
public static class ForgejoToolsFilters
{
    /// <summary>ForgejoTools ツール呼び出しフィルタ</summary>
    /// <param name="next">リクエストパイプラインの次のハンドラ</param>
    /// <returns>リクエストハンドラ</returns>
    public static McpRequestHandler<CallToolRequestParams, CallToolResult> ToolCallFilter(McpRequestHandler<CallToolRequestParams, CallToolResult> next)
    {
        return async (context, breaker) =>
        {
            try
            {
                // パイプライン呼び出し
                return await next(context, breaker);
            }
            catch (ForgejoClientException ex)
            {
                // Forgejo からのエラーメッセージを伝達
                return new CallToolResult { IsError = true, Content = [new TextContentBlock { Text = $"Forgejo Error: {ex.Message}" }] };
            }
        };
    }
}
