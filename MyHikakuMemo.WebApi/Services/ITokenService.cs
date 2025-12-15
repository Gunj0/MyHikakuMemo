using MyHikakuMemo.WebApi.Data.Entities;

namespace MyHikakuMemo.WebApi.Services;

public interface ITokenService
{
    (string Token, DateTime Expiration) GenerateToken(ApplicationUser user);
}
