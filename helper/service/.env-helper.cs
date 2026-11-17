#!/usr/bin/env -S dotnet run --file
#:package Lestaly.General@0.148.0
using Lestaly;

/// <summary>テスト用 Forgejo設定</summary>
public class ServiceConfig(DirectoryInfo dir)
{
    /// <summary>サービスインスタンス設定</summary>
    public class InstanceConfig
    {
        /// <summary>サービスのURL</summary>
        public Uri Url { get; } = new("http://localhost:3880");
    }

    /// <summary>サービスインスタンス設定</summary>
    public class DockerConfig(DirectoryInfo dir)
    {
        /// <summary>Compose ファイル</summary>
        public FileInfo ComposeFile { get; } = dir.RelativeFile("compose.yml");

        /// <summary>マウントディレクトリ/summary>
        public DirectoryInfo MountDirectory { get; } = dir.RelativeDirectory("work/mount");

        /// <summary>Forgejo サービス/summary>
        public string ForgejoService { get; } = "app";
    }

    /// <summary>初回起動セットアップの設定</summary>
    public class SetupConfig
    {
        /// <summary>セットアップ時の admin ユーザ名</summary>
        public string AdminUser { get; } = "forgejo-admin";

        /// <summary>セットアップ時の admin パスワード</summary>
        public string AdminPass { get; } = "forgejo-admin-pass";

        /// <summary>セットアップ時の admin メールアドレス</summary>
        public string AdminMail { get; } = "forgejo-admin@example.com";

        /// <summary>admin 用の管理トークン名</summary>
        public string AdminTokenName { get; } = "admin-script-token";
    }

    /// <summary>APIキーファイル設定</summary>
    public class ApiKeysConfig(DirectoryInfo dir)
    {
        /// <summary>Adminの管理用APIキーファイル</summary>
        public FileInfo ManageTokenFile { get; } = dir.RelativeFile("work/keys/.admin-token");

        /// <summary>テスターのAPIキーファイル</summary>
        public FileInfo TesterTokenFile { get; } = dir.RelativeFile("work/keys/.tester-token");
    }

    /// <summary>テストユーザの設定</summary>
    public class TesterConfig
    {
        /// <summary>テスタユーザ名</summary>
        public string TesterName { get; } = "テストユーザ";

        /// <summary>テスタユーザID</summary>
        public string TesterUser { get; } = "test-user";

        /// <summary>テスタパスワード</summary>
        public string TesterPass { get; } = "test-pass";

        /// <summary>テスタメールアドレス</summary>
        public string TesterMail { get; } = "test@example.com";

        /// <summary>テスタトークン名</summary>
        public string TesterTokenName { get; } = "test-mcp-token";

        /// <summary>テスタトークンスコープ</summary>
        public string[] TesterTokenScopes { get; } = ["write:issue", "write:organization", "write:package", "write:repository", "write:user"];
    }

    /// <summary>テスト組織の設定</summary>
    public class TestOrgConfig
    {
        /// <summary>テスト組織名</summary>
        public string OrgName { get; } = "test-org";

        /// <summary>テストチーム名</summary>
        public string TeamName { get; } = "test-team";
    }

    /// <summary>開発関連の設定</summary>
    public class DevelopConfig(DirectoryInfo dir)
    {
        /// <summary>開発用設定ファイル</summary>
        public FileInfo AppSettingsFile { get; } = dir.FindFileAncestor("mcp-peek-forgejo.slnx")!.Directory!.RelativeFile("src/appsettings.Development.json");
    }

    /// <summary>サービスインスタンス設定</summary>
    public InstanceConfig Instance { get; } = new();

    /// <summary>Docker関連設定</summary>
    public DockerConfig Docker { get; } = new(dir);

    /// <summary>初回起動セットアップの設定</summary>
    public SetupConfig Setup { get; } = new();

    /// <summary>APIキーファイル設定</summary>
    public ApiKeysConfig ApiKeys { get; } = new(dir);

    /// <summary>テストユーザの設定</summary>
    public TesterConfig Tester { get; } = new();

    /// <summary>テスト組織の設定</summary>
    public TestOrgConfig TestOrg { get; } = new();

    /// <summary>開発関連の設定</summary>
    public DevelopConfig Develop { get; } = new(dir);
}


public record ServiceCredential(Uri Service, string Username, string Password);
public record ServiceToken(Uri Service, string Token);

public static class EnvHelper
{
    extension(FileInfo self)
    {
        /// <summary>ファイルへのスクランブル保存処理を生成</summary>
        /// <returns>ファイルへのスクランブル保存処理</returns>
        public FileRoughScrambler ScriptScrambler()
            => self.CreateScrambler(context: self.FullName);

        /// <summary>ファイルに認証トークン情報を保存する</summary>
        /// <param name="service">アクセス先サービスURI</param>
        /// <param name="token">APIトークン</param>
        /// <param name="cancelToken">キャンセルトークン</param>
        /// <returns>保存した認証トークン情報</returns>
        public ValueTask<ServiceToken> SaveTokenAsync(Uri service, string token, CancellationToken cancelToken = default)
            => self.WithDirectoryCreate().ScriptScrambler().SaveTokenAsync(service, token, cancelToken);

        /// <summary>ファイルから認証トークン情報を読み出す</summary>
        /// <param name="cancelToken">キャンセルトークン</param>
        /// <returns>読み取った認証トークン情報</returns>
        public ValueTask<ServiceToken?> LoadTokenAsync(CancellationToken cancelToken = default)
            => self.ScriptScrambler().LoadTokenAsync(cancelToken);
    }

    extension(FileRoughScrambler self)
    {
        /// <summary>ファイルに認証トークン情報を保存する</summary>
        /// <param name="service">アクセス先サービスURI</param>
        /// <param name="token">APIトークン</param>
        /// <param name="cancelToken">キャンセルトークン</param>
        /// <returns>保存した認証トークン情報</returns>
        public async ValueTask<ServiceToken> SaveTokenAsync(Uri service, string token, CancellationToken cancelToken = default)
        {
            var auth = new ServiceToken(service, token);
            var authText = $"{auth.Service.AbsoluteUri}\t{auth.Token}";
            await self.ScrambleTextAsync(authText, cancelToken: cancelToken);
            return auth;
        }

        /// <summary>ファイルから認証トークン情報を読み出す</summary>
        /// <param name="cancelToken">キャンセルトークン</param>
        /// <returns>読み取った認証トークン情報</returns>
        public async ValueTask<ServiceToken?> LoadTokenAsync(CancellationToken cancelToken = default)
        {
            var authText = await self.DescrambleTextAsync(cancelToken);
            if (authText == null) return null;
            var authParts = authText.Split("\t");
            return new(new(authParts[0]), authParts[1]);
        }
    }

}
