using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyHikakuMemo.WebApi.Controllers.Dtos;
using MyHikakuMemo.WebApi.Services;

namespace MyHikakuMemo.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class MemoController(
    IMemoService memoService, IAuthService authService)
    : ControllerBase
{
    private readonly IMemoService _memoService = memoService;
    private readonly IAuthService _authService = authService;

    /// <summary>
    /// Memo一覧を取得
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<MemoDto>>> GetMemos()
    {
        var userId = _authService.GetUserId(User);
        var memos = await _memoService.GetMemosAsync(userId);
        return Ok(memos);
    }

    /// <summary>
    /// 特定のMemoを取得
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<MemoDto>> GetMemo(Guid id)
    {
        var userId = _authService.GetUserId(User);
        var memo = await _memoService.GetMemoAsync(id, userId);

        if (memo == null)
        {
            return NotFound();
        }

        return Ok(memo);
    }

    /// <summary>
    /// 新しいMemoを作成
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<MemoDto>> CreateMemo(
        [FromBody] CreateMemoDto dto)
    {
        var userId = _authService.GetUserId(User);
        var memo = await _memoService.CreateMemoAsync(userId, dto);

        return CreatedAtAction(nameof(GetMemo), new { id = memo.Id }, memo);
    }

    /// <summary>
    /// Memoを更新
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<MemoDto>> UpdateMemo(
        Guid id, [FromBody] CreateMemoDto dto)
    {
        var userId = _authService.GetUserId(User);
        var memo = await _memoService.UpdateMemoAsync(id, userId, dto);

        if (memo == null)
        {
            return NotFound();
        }

        return Ok(memo);
    }

    /// <summary>
    /// Memoを削除
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteMemo(Guid id)
    {
        var userId = _authService.GetUserId(User);
        var success = await _memoService.DeleteMemoAsync(id, userId);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }
}
