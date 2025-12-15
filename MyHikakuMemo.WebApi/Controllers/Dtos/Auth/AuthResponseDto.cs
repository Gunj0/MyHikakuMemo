namespace MyHikakuMemo.WebApi.Controllers.Dtos.Auth;

public class AuthResponseDto
{
    // JWTトークン
    public string Token { get; set; } = string.Empty;
    // トークンの有効期限
    public DateTime Expiration { get; set; }
    // ユーザーのメールアドレス
    public string Email { get; set; } = string.Empty;
}
