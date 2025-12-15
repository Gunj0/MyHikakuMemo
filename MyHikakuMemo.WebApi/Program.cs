using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MyHikakuMemo.WebApi.Data;
using MyHikakuMemo.WebApi.Data.Entities;

var builder = WebApplication.CreateBuilder(args);

// PostgreSQL設定
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("接続文字列が見つかりません"))
);

// Identity設定
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireDigit = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// JWT認証設定
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException("JWT発行者が見つかりません"),
        ValidAudience = builder.Configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException("JWTオーディエンスが見つかりません"),
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]
                ?? throw new InvalidOperationException("JWTシークレットキーが見つかりません")
            ))
    };
});

// コントローラ
builder.Services.AddControllers();
// OpenAPI自動生成 https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// 開発環境でOpenAPIドキュメントを有効化
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// DBマイグレーション
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.Migrate();
}

// Httpsリダイレクトを有効化
app.UseHttpsRedirection();

// 認証ミドルウェアを必ず認可ミドルウェアの前に配置
app.UseAuthentication();
app.UseAuthorization();

// コントローラをマッピング
app.MapControllers();

app.Run();
