using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
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

RsaSecurityKey? securityKey = null;

try
{
    var httpClient = new HttpClient();
    var jwksUrl = $"{betterAuthUrl}/api/auth/jwks";
    var jwksResponse = await httpClient.GetStringAsync(jwksUrl);
    Console.WriteLine($"JWKS読み込み成功: {jwksUrl}");

    var jwks = JsonSerializer.Deserialize<JsonElement>(jwksResponse);
    var key = jwks.GetProperty("keys")[0];

    // RSA公開鍵のパラメータを取得
    var nValue = key.GetProperty("n").GetString();
    var eValue = key.GetProperty("e").GetString();

    // RSASecurityKeyの作成
    var rsa = RSA.Create();
    rsa.ImportParameters(new RSAParameters
    {
        Modulus = Base64UrlEncoder.DecodeBytes(nValue),
        Exponent = Base64UrlEncoder.DecodeBytes(eValue)
    });

    securityKey = new RsaSecurityKey(rsa);
    Console.WriteLine("RSA公開鍵の作成成功");
}
catch (Exception ex)
{
    Console.WriteLine($"JWKS読み込みエラー: {ex.Message}");
    throw;
}

// JWT認証設定
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = issuer;
        // options.MetadataAddress = $"{betterAuthUrl}/.well-known/openid-configuration";
        options.MetadataAddress = $"{betterAuthUrl}/api/auth/jwks";
        options.RequireHttpsMetadata = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = securityKey,
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
                Console.WriteLine($"JWT認証失敗: {context.Exception.Message}");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                var userId = context.Principal?.FindFirst("sub")?.Value;
                Console.WriteLine($"JWT認証成功 - ユーザーID: {userId}");
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
