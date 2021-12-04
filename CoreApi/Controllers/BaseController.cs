using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreApi.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class BaseController : ControllerBase
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public StatusResult<T> Success<T>(T data, string msg = "")
    {
        HttpContext.Response.StatusCode = 200;
        return new StatusResult<T>() { Code = 200, Data = data, Msg = msg };
    }

    [ApiExplorerSettings(IgnoreApi = true)]
    public StatusResult<T> Error<T>(string msg, int code = 400)
    {
        HttpContext.Response.StatusCode = code;
        return new StatusResult<T>() { Code = code, Data = default, Msg = msg };
    }
}

public class StatusResult<T>
{
    public int Code { get; set; }
    public T Data { get; set; }
    public string Msg { get; set; }
}
