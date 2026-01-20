using System.Security.Claims;

namespace MyHikakuMemo.WebApi.Services;

public interface IAuthService
{
    string GetUserId(ClaimsPrincipal user);
}

public class AuthService : IAuthService
{
    public string GetUserId(ClaimsPrincipal user)
    {
        var userId = user.FindFirst("sub")?.Value
            ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("ユーザーIDが見つかりません");
        return userId;
    }
}
