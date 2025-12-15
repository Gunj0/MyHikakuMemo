using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyHikakuMemo.WebApi.Controllers.Dtos.Auth;
using MyHikakuMemo.WebApi.Data.Entities;
using MyHikakuMemo.WebApi.Services;

namespace MyHikakuMemo.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    ITokenService tokenService
) : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
    private readonly ITokenService _tokenService = tokenService;

    /// <summary>
    /// POST: /auth/register (ユーザー登録)
    /// </summary>
    /// <param name="dto">ユーザー登録リクエスト</param>
    /// <returns>認証レスポンス</returns>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = new ApplicationUser { UserName = dto.Email, Email = dto.Email };
        var result = await _userManager.CreateAsync(user, dto.Password);

        if (result.Succeeded)
        {
            // 登録成功と同時にJWTを発行しログイン状態にする
            var (token, expiration) = _tokenService.GenerateToken(user);

            return Ok(new AuthResponseDto
            {
                Token = token,
                Expiration = expiration,
                Email = user.Email
            });
        }

        // 登録失敗（例: メールアドレス重複）
        return BadRequest(result.Errors);
    }

    /// <summary>
    /// POST: /auth/login (ユーザーログイン)
    /// </summary>
    /// <param name="dto">ログインリクエスト</param>
    /// <returns>認証レスポンス</returns>
    [HttpPost("Login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // メールアドレスからユーザーを取得
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
        {
            return Unauthorized(new
            {
                Message = "メールアドレスまたはパスワードが正しくありません"
            });
        }

        // パスワードを検証
        var result = await _signInManager.CheckPasswordSignInAsync(
            user, dto.Password, lockoutOnFailure: true);

        // 認証成功: JWTを発行
        if (result.Succeeded)
        {
            var (token, expiration) = _tokenService.GenerateToken(user);

            return Ok(new AuthResponseDto
            {
                Token = token,
                Expiration = expiration,
                Email = user.Email!
            });
        }

        // 認証失敗
        return Unauthorized(new
        {
            Message = "メールアドレスまたはパスワードが正しくありません"
        });
    }
}
