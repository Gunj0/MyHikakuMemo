using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MyHikakuMemo.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class MemoController : ControllerBase
{
    public IEnumerable<string> Get()
    {
        return new List<string> { "Memo1", "Memo2", "Memo3" };
    }
}
