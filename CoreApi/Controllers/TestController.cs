using CoreApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace CoreApi.Controllers;

/// <summary>
/// 测试
/// </summary>
[AllowAnonymous]
[ApiVersionNeutral]
public class TestController : BaseController
{
    private readonly ITestServicecs testService;
    private readonly HttpClient httpClient;

    public TestController(ITestServicecs _testService, IHttpClientFactory httpClientFactory)
    {
        testService = _testService;
        httpClient = httpClientFactory.CreateClient();
    }

    /// <summary>
    /// EF Core 多表分页查询测试
    /// </summary>
    /// <returns></returns>
    [HttpGet("ef-page")]
    public async Task<IActionResult> EFPage(int pageIndex, int pageSize)
    {
        (dynamic data, int total) = await testService.GetListAsync(pageIndex, pageSize);
        return Ok(new { data, total });
    }

    /// <summary>
    /// 获取当前时间戳(毫秒)
    /// </summary>
    /// <returns></returns>
    [HttpGet("getTimeStampNow")]
    public IActionResult GetTimeStampNow()
    {
        return Ok(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
    }

    /// <summary>
    /// 解析时间戳(毫秒)
    /// </summary>
    /// <param name="timestamp">时间戳</param>
    /// <returns></returns>
    [HttpGet("getTimeBy")]
    public IActionResult GetTimeBy(string timestamp)
    {
        var res = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(timestamp)).LocalDateTime;
        return Ok(res);
    }

    /// <summary>
    /// 依赖注入测试
    /// </summary>
    /// <returns></returns>
    [HttpGet("testDI")]
    public IActionResult TestDI() => Ok(testService.TestDI("test DI success"));
}
