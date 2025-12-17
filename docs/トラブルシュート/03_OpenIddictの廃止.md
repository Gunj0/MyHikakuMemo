# OpenIddict の廃止

## OpenIddict とは

- JWT 認証トークン発行と検証を管理
- Authorization Code Flow + PKCE の OAuth/OpenID Connect が実装できる
  - Authorization Code:
    - ログインにより認可コードを取得し、それを元にトークンを受け取る
  - PKCE(Proof Key for Code Exchange):
    - 認可コードリクエスト時に code_verifier を付与してトークン発行に使用する
    - 認可コード横取り攻撃を避けることができる
  - [参考](https://dev.classmethod.jp/articles/oauth-2-0-pkce-by-auth0/)

## 廃止理由

- 最小構成でも実装が重く複雑になる
- Auth.js で代用できそう

## 構築したときのメモ

### NuGet

```zsh
### OpenIddict
dotnet add package OpenIddict.AspNetCore
dotnet add package OpenIddict.EntityFrameworkCore
### メール送信
dotnet add package MailKit
```

### ApplicationUser エンティティ追加

```cs
public class ApplicationUser : IdentityUser
{
  // IdentityUserで基本スキーマは設定される
  // 追加プロパティはここに書く
}
```

### DbContext 編集

```cs
public class AppDbContext(
    DbContextOptions<AppDbContext> options
    // IdentityDbContextを継承してApplicationUserを追加
    ) : IdentityDbContext<ApplicationUser>(options)
{
  // ここには追記不要
}
```

### Program.cs 修正

```cs
// ASP.NET Core Identity (User 作成)
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequiredLength = 8; // 最小パスワード長
        options.Password.RequireLowercase = true; // 小文字必須
        options.Password.RequireUppercase = true; // 大文字必須
        options.Password.RequireDigit = true; // 数字必須
        options.Password.RequireNonAlphanumeric = false; // 記号必須
        options.User.RequireUniqueEmail = true; // メールアドレスの一意性
        options.SignIn.RequireConfirmedEmail = true; // メール確認
    })
    .AddEntityFrameworkStores<AppDbContext>() // EF Core で管理
    .AddDefaultTokenProviders(); // トークンプロバイダの追加
```

### マイグレーション

```zsh
dotnet ef migrations add Initial
dotnet ef database update
```

- 以下テーブルが作成される
  - AspNetUsers: ユーザー管理
  - AspNetRoles: ユーザーに付与する役割（権限）管理
  - AspNetUserRoles: 上記 2 つの N 対 N 中間テーブル
  - AspNetUserClaims: ユーザー単位での Claim（属性）管理
  - AspNetRoleClaims: 役割が持つ Claim 管理
  - AspNetUserLogins: サードパーティ経由でログインするアカウント情報管理
  - AspNetUserTokens: ユーザー別のアクセストークン管理

### EmailService 実装

- Program.cs で DI する

```cs
using var client = new SmtpClient(smtpHost, smtpPort)
{
    Credentials = new NetworkCredential(smtpUser, smtpPassword),
    EnableSsl = true
};

var mailMessage = new MailMessage
{
    From = new MailAddress(fromEmail ?? "", fromName ?? ""),
    Subject = subject,
    Body = body,
    IsBodyHtml = true
};

mailMessage.To.Add(toEmail);

await client.SendMailAsync(mailMessage);
```

### AuthController 実装

```cs
// ユーザー登録
[HttpPost("register")]
async Task<IActionResult<UserInfoDto>> Register([FromBody] RegisterDto dto);

// メールアドレス確認
[HttpPost("confirm-email")]
async Task<ActionResult> ConfirmEmail([FromBody] ConfirmEmailDto dto);

// ログイン
[HttpPost("login")]
async Task<ActionResult<UserInfoDto>> Login([FromBody] LoginDto dto);

// ログアウト
[HttpPost("logout")]
[Authorize]
async Task<ActionResult> Logout();

// パスワードリセットリクエスト
[HttpPost("forgot-password")]
async Task<ActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto);

// パスワードリセット実行
[HttpPost("reset-password")]
async Task<ActionResult> ResetPassword([FromBody] ResetPasswordDto dto);

// 現在のユーザー情報取得
[HttpGet("me")]
[Authorize]
async Task<ActionResult<UserInfoDto>> GetCurrentUser();
```

### Gmail 設定

- [Gmail の SMTP 設定でメール送信を行う手順](https://zenn.dev/milky/articles/gmail-mail-server)

### Postman から確認

```text
POST https://localhost:7115/api/auth/register
```

```json
{
  "email": "example@example.com",
  "password": "Password123!",
  "displayName": "テストユーザー"
}
```

- 届いたメールの [localhost](https://localhost:7115/api/auth/confirm-email) を開く
- → AspNetUsers の EmailConfirmed が true になる

```text
POST https://localhost:7115/api/auth/login
```

```json
{
  "email": "example@example.com",
  "password": "Password123!"
}
```

- ログイン成功してレスポンスを返す

### Program.cs に OpenIddict 実装

```cs
builder.Services.AddDbContext<AppDbContext>(options =>
    {
        options.UseNpgsql(
          // 省略
        );
        // OpenIddict のテーブルを統合する
        options.UseOpenIddict();
    });

builder.Services.AddOpenIddict()
    .AddCore(options =>
    {
        // OpenIddict を EF Core と統合
        options.UseEntityFrameworkCore()
            .UseDbContext<AppDbContext>();
    })
    .AddServer(options =>
    {
        // エンドポイント設定
        options
            // 認可コード発行、ログイン画面
            .SetAuthorizationEndpointUris("/connect/authorize")
            // 認可コード + PKCE → アクセストークン/IDトークン/リフレッシュトークン発行
            .SetTokenEndpointUris("/connect/token")
            // アクセストークンからユーザー情報(メール・名前)取得
            .SetUserInfoEndpointUris("/connect/userinfo")
            // ログアウト後のリダイレクト
            .SetEndSessionEndpointUris("/connect/logout");
        // フロー設定
        options
            .AllowAuthorizationCodeFlow() // 認可コードフロー
            .RequireProofKeyForCodeExchange(); // PKCE必須(セキュリティ/モバイル対応)
        // Refresh Tokenを有効化
        options.AllowRefreshTokenFlow();
        // スコープの登録
        options.RegisterScopes(
            Scopes.OpenId,
            Scopes.Profile,
            Scopes.Email, // EmailがIDトークン/userinfoで取得可能になる
            "api");
        // 開発用の暗号化・署名キー（本番環境では証明書を使用）
        options
            .AddDevelopmentEncryptionCertificate()
            .AddDevelopmentSigningCertificate();
        // ASP.NET Core統合
        options
            // Controllerに処理を渡せるようにする
            .UseAspNetCore()
                .EnableAuthorizationEndpointPassthrough()
                .EnableTokenEndpointPassthrough()
                .EnableUserInfoEndpointPassthrough()
                .EnableEndSessionEndpointPassthrough()
                // ステータスコードでレスポンスを自動生成する
                .EnableStatusCodePagesIntegration();
    });

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    // DB初期化
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await context.Database.MigrateAsync();

    // データシード
    var seeder = new DbSeeder(scope.ServiceProvider, app.Configuration);
    await seeder.SeedAsync();
}
```

### DbSeeder 実装

```cs
// クライアントの登録
async Task SeedClientsAsync(IServiceProvider provider);
// スコープの登録
async Task SeedScopesAsync(IServiceProvider provider);
```

### AuthorizationController 実装

```cs

```

### AccountController 実装

```cs
[Route("account")]
public class AccountController(
    SignInManager<ApplicationUser> signInManager,
    UserManager<ApplicationUser> userManager) : Controller
{

```

### ダミーログインページ実装

```cs
<form method="post" action="/Account/Login">
  @Html.AntiForgeryToken()
  <input type="hidden" name="returnUrl" value="@ViewData["ReturnUrl"]">

  <div class="form-group">
    <label for="email">メールアドレス</label>
    <input type="email" id="email" name="email" required autocomplete="email" />
  </div>

  <div class="form-group">
    <label for="password">パスワード:</label>
    <input
      type="password"
      id="password"
      name="password"
      required
      autocomplete="current-password"
    />
  </div>

  <div>
    <button type="submit">ログイン</button>
  </div>
</form>
```

### 動作確認

- 認可コード取得
  - code_challenge_method=S256: code_challenge をハッシュ化して送る
  - code_challenge: 43 ～ 128 の文字列を SHA-256 によりハッシュ化したもの

```text
https://localhost:7115/connect/authorize?client_id=nextjs-client&redirect_uri=https://localhost:7115/callback&response_type=code&scope=openid profile email api&code_challenge=E9Melhoa2OwvFrEMTJguCHaoeK1t8URWbuGJSstw-cM&code_challenge_method=S256
```

↓ リダイレクトを確認

```text
https://localhost:7115/Account/Login?ReturnUrl=/connect/authorize?client_id=nextjs-client&redirect_uri=https%3A%2F%2Flocalhost%3A7115%2Fcallback&response_type=code&scope=openid%20profile%20email%20api&code_challenge=E9Melhoa2OwvFrEMTJguCHaoeK1t8URWbuGJSstw-cM&code_challenge_method=S256&prompt=
```

↓ ログイン成功してリダイレクト

```text
https://localhost:7115/callback?code={省略}&iss=https://localhost:7115/
```

↓ code をセットしてアクセス

```text
POST https://localhost:7115/connect/token \
     -H "Content-Type: application/x-www-form-urlencoded" \
     -d "grant_type=authorization_code" \
     -d "client_id=nextjs-client" \
     -d "code=取得したコード" \
     -d "redirect_uri=http://localhost:3000/callback" \
     -d "code_verifier=dBjftJeZ4CVP-mB92K27uhbUJU1p1r_wW1gFWFOEjXk"
```

※body は x-www-form-urlencoded にセットする
※認可コードは 1 度失敗する・期限が過ぎると使えなくなるので再度取得する

↓ トークン取得成功

```json
{
  "access_token": "〜",
  "token_type": "Bearer",
  "expires_in": 3599,
  "scope": "openid profile email api",
  "id_token": "〜"
}
```

↓

- アクセストークンから Userinfo を取得成功

```text
curl https://localhost:7115/connect/userinfo \
     -H "Authorization: Bearer アクセストークン"
```
