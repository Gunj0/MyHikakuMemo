using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using MyHikakuMemo.WebApi.Data;
using MyHikakuMemo.WebApi.Services;

var builder = WebApplication.CreateBuilder(args);

// PostgreSQL設定
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("接続文字列が見つかりません"))
);

// Better Auth設定
var betterAuthUrl = builder.Configuration["BetterAuth:Url"] ?? "http://localhost:3000";
var issuer = builder.Configuration["BetterAuth:Issuer"] ?? betterAuthUrl;
var requireHttpsMetadata = !builder.Environment.IsDevelopment();

// MemoryCache追加
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient("Jwks", client =>
{
    client.Timeout = TimeSpan.FromSeconds(5);
});

// JWKSキーリゾルバーをサービスコンテナに登録
var jwksUrl = $"{betterAuthUrl}/api/auth/jwks";
builder.Services.AddSingleton(sp =>
    new JwksKeyResolver(
        sp.GetRequiredService<IHttpClientFactory>().CreateClient("Jwks"),
        jwksUrl,
        sp.GetRequiredService<IMemoryCache>(),
        sp.GetRequiredService<ILogger<JwksKeyResolver>>()));

// JWT認証設定
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<JwksKeyResolver, ILogger<Program>>((options, keyResolver, logger) =>
    {
        options.Authority = issuer;
        options.RequireHttpsMetadata = requireHttpsMetadata;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            // 動的に鍵を取得するリゾルバーを設定（キャッシュ付き）
            IssuerSigningKeyResolver = (token, securityToken, kid, parameters) =>
                keyResolver.ResolveSigningKeysAsync(kid).GetAwaiter().GetResult(),
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = false, // Better Authでは通常設定不要
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                logger.LogWarning(context.Exception, "JWT認証失敗");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                var userId = context.Principal?.FindFirst("sub")?.Value;
                logger.LogInformation("JWT認証成功 - ユーザーID: {UserId}", userId);
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// サービス登録
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IMemoService, MemoService>();

// コントローラ
builder.Services.AddControllers();
builder.Services.AddHealthChecks();

var app = builder.Build();

// DBマイグレーション
// using (var scope = app.Services.CreateScope())
// {
//     var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
//     context.Database.Migrate();
// }

// Httpsリダイレクトを有効化
app.UseHttpsRedirection();

// 認証ミドルウェアを必ず認可ミドルウェアの前に配置
app.UseAuthentication();
app.UseAuthorization();

// コントローラをマッピング
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
