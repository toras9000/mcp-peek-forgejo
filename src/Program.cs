using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using mcp_peek_forgejo.Services;
using mcp_peek_forgejo.Tools;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Server;
using ToraNith.WebNook;

// JSONシリアライズオプション
var mpcJson = new JsonSerializerOptions();
mpcJson.AllowTrailingCommas = true;
mpcJson.ReadCommentHandling = JsonCommentHandling.Skip;
mpcJson.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
mpcJson.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
mpcJson.NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.AllowNamedFloatingPointLiterals;
mpcJson.PropertyNameCaseInsensitive = true;
mpcJson.TypeInfoResolver = new DefaultJsonTypeInfoResolver();
mpcJson.MakeReadOnly();

// アプリサービス構築
var builder = WebApplication.CreateBuilder(args);

// サービス登録
builder.Services.AddSingleton<IAppConfigProvider, AppConfigProvider>();
builder.Services.AddSingleton<IAppAuthValidator, AppAuthValidator>();
builder.Services.AddSingleton<IPostConfigureOptions<McpServerOptions>, ForgejoToolsConfigurator>();

// MCPサービス追加
builder.Services
    .AddMcpServer()
    .WithHttpTransport(options =>
    {
        options.Stateless = true;
    })
    .AddAuthorizationFilters()
    .WithRequestFilters(filters =>
    {
        filters.AddCallToolFilter(ForgejoToolsFilters.ToolCallFilter);
    })

    .WithTools<ForgejoTools>(mpcJson)
    ;

// 認証サービス
builder.Services.AddAuthentication(nameof(SimpleAuthHandler))
    .AddScheme<SimpleAuthSchemeOptions, SimpleAuthHandler>(nameof(SimpleAuthHandler), default);
builder.Services.AddOptions<SimpleAuthSchemeOptions>(nameof(SimpleAuthHandler))
        .Configure<IAppAuthValidator>((options, validator) => options.UseValidator(validator.Authenticate));
builder.Services.AddAuthorization();

// Forwardedヘッダ構成
builder.Services.ConfigureForwardedSettings(builder.Configuration.GetSection("ForwardedSettings"));


// アプリインスタンス構築
var app = builder.Build();

// 認証
app.UseAuthentication();
app.UseAuthorization();

// MCPエンドポイント
app.MapMcp();

// 実行
await app.RunAsync();
