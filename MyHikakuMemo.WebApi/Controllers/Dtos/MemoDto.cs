using System.ComponentModel.DataAnnotations;

namespace MyHikakuMemo.WebApi.Controllers.Dtos;

public class CreateMemoDto
{
    [Required]
    [MaxLength(255)]
    public required string Title { get; set; }

    [Required]
    [MaxLength(10000)]
    public required string Content { get; set; }
}

public class MemoDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = "";
    public string Content { get; set; } = "";
    public string UserId { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
