using System.Net.Http.Headers;
using ToraNith.WebNook;

namespace mcp_peek_forgejo.Services;

/// <summary>認証ロール定義</summary>
public static class AppAuthRoles
{
    /// <summary>認証済みレベル1</summary>
    public const string Level1 = "Level1";

    /// <summary>認証済みレベル2</summary>
    public const string Level2 = "Level2";
}

/// <summary>認証情報検証サービス</summary>
public interface IAppAuthValidator
{
    /// <summary>認証ヘッダを検証する</summary>
    /// <param name="header">認証ヘッダ</param>
    /// <returns>検証結果。検証対象外の場合は null </returns>
    public SimpleAuthResult? Authenticate(AuthenticationHeaderValue header);
}

/// <summary>認証情報検証サービス実装</summary>
/// <remarks>
/// 固定のトークン値と比較する非常に単純な認証検証を行う。
/// </remarks>
public class AppAuthValidator(IAppConfigProvider config) : IAppAuthValidator
{
    /// <summary>認証ユーザ名(適当)</summary>
    public const string AppAuthUser = "McpUser";

    /// <inheritdoc />
    public SimpleAuthResult? Authenticate(AuthenticationHeaderValue value)
    {
        // 想定する認証スキーム以外は対象外
        if (!value.Scheme.Equals("Bearer", StringComparison.OrdinalIgnoreCase)) return null;

        // アプリ設定の認証トークン設定が無ければ対象外
        var secrets = config.McpConfig.Service.Secrets;
        if (secrets == null || secrets.Count <= 0) return null;

        // 設定されたトークンとの単純比較
        foreach (var secret in secrets)
        {
            var role = secret.Key;
            var token = secret.Value;
            if (token.Equals(value.Parameter, StringComparison.Ordinal))
            {
                // トークンが一致したら認証成功。トークンと対応するロールのユーザとする。
                return new SimpleAuthSuccess(AppAuthUser, role);
            }
        }

        // いずれもマッチしなければ認証失敗
        return new SimpleAuthFailed("Invalid authentication");
    }
}
