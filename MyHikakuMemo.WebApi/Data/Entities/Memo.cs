using System.ComponentModel.DataAnnotations;

namespace MyHikakuMemo.WebApi.Data.Entities;

public class Memo
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = "";

    [Required]
    [MaxLength(10000)]
    public string Content { get; set; } = "";

    [Required]
    public string UserId { get; set; } = "";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
