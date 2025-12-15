using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using MyHikakuMemo.WebApi.Data.Entities;

namespace MyHikakuMemo.WebApi.Services;

public class JwtTokenService(IConfiguration configuration) : ITokenService
{
    private readonly IConfiguration _configuration = configuration;

    public (string Token, DateTime Expiration) GenerateToken(ApplicationUser user)
    {
        // 環境設定からJWT設定を取得(Program.csで存在チェック済)
        var secretKey = Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]!);
        var issuer = _configuration["Jwt:Issuer"]!;
        var audience = _configuration["Jwt:Audience"]!;

        // トークンの有効期限を1時間後に設定
        var expiration = DateTime.UtcNow.AddHours(1);

        // クレームの設定
        var claims = new List<Claim>
            {
                // ユーザーIDとEmailをクレームとして含める
                new(JwtRegisteredClaimNames.Sub, user.Id),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(ClaimTypes.Name, user.UserName ?? user.Email ?? ""),
                new(ClaimTypes.Email, user.Email ?? "")
            };

        // トークンの生成
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiration,
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(secretKey), SecurityAlgorithms.HmacSha256Signature)
        };

        // トークンハンドラーを使用してトークンを作成
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return (tokenHandler.WriteToken(token), expiration);
    }
}
