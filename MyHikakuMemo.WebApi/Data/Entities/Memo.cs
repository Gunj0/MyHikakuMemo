using System.ComponentModel.DataAnnotations;

namespace MyHikakuMemo.WebApi.Data.Entities;

public class Memo
{
    [Key]
    public string Title { get; set; } = "";

    [MaxLength(10000)]
    public string Content { get; set; } = "";
}
