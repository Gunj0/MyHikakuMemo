using System.ComponentModel.DataAnnotations;

namespace MyHikakuMemo.WebApi.Controllers.Dtos.Auth;

public class ExternalLoginDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    // Googleからの名前（表示名として使用）
    public string? Name { get; set; }

    [Required]
    // 外部プロバイダー名（"Google"）
    public string Provider { get; set; } = string.Empty;

    [Required]
    // 外部プロバイダーでのユーザーID（必須ではないが、連携の確実性を高める）
    public string ProviderKey { get; set; } = string.Empty;
}
