using MyHikakuMemo.WebApi.Controllers.Dtos;
using MyHikakuMemo.WebApi.Data;
using MyHikakuMemo.WebApi.Data.Entities;

namespace MyHikakuMemo.WebApi.Services;

public interface IMemoService
{
    Task<MemoDto> CreateMemoAsync(string userId, CreateMemoDto dto);
    Task<MemoDto?> GetMemoAsync(Guid id, string userId);
    Task<List<MemoDto>> GetMemosAsync(string userId);
    Task<MemoDto?> UpdateMemoAsync(Guid id, string userId, CreateMemoDto dto);
    Task<bool> DeleteMemoAsync(Guid id, string userId);
}

public class MemoService(ApplicationDbContext context) : IMemoService
{
    private readonly ApplicationDbContext _context = context;

    public async Task<MemoDto> CreateMemoAsync(string userId, CreateMemoDto dto)
    {
        var memo = new Memo
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            Content = dto.Content,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Memos.Add(memo);
        await _context.SaveChangesAsync();

        return MapToDto(memo);
    }

    public async Task<MemoDto?> GetMemoAsync(Guid id, string userId)
    {
        var memo = await _context.Memos.FindAsync(id);

        if (memo == null || memo.UserId != userId)
        {
            return null;
        }

        return MapToDto(memo);
    }

    public async Task<List<MemoDto>> GetMemosAsync(string userId)
    {
        var memos = _context.Memos
            .Where(m => m.UserId == userId)
            .OrderByDescending(m => m.CreatedAt)
            .ToList();

        return await Task.FromResult(memos.Select(MapToDto).ToList());
    }

    public async Task<MemoDto?> UpdateMemoAsync(Guid id, string userId, CreateMemoDto dto)
    {
        var memo = await _context.Memos.FindAsync(id);

        if (memo == null || memo.UserId != userId)
        {
            return null;
        }

        memo.Title = dto.Title;
        memo.Content = dto.Content;
        memo.UpdatedAt = DateTime.UtcNow;

        _context.Memos.Update(memo);
        await _context.SaveChangesAsync();

        return MapToDto(memo);
    }

    public async Task<bool> DeleteMemoAsync(Guid id, string userId)
    {
        var memo = await _context.Memos.FindAsync(id);

        if (memo == null || memo.UserId != userId)
        {
            return false;
        }

        _context.Memos.Remove(memo);
        await _context.SaveChangesAsync();

        return true;
    }

    private static MemoDto MapToDto(Memo memo)
    {
        return new MemoDto
        {
            Id = memo.Id,
            Title = memo.Title,
            Content = memo.Content,
            UserId = memo.UserId,
            CreatedAt = memo.CreatedAt,
            UpdatedAt = memo.UpdatedAt
        };
    }
}
