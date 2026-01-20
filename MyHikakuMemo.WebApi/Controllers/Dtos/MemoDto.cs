namespace MyHikakuMemo.WebApi.Controllers.Dtos;

public class CreateMemoDto
{
    public required string Title { get; set; }
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
