using CoreApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
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
    private readonly ITestService testService;
    private readonly HttpClient httpClient;

    public TestController(ITestService _testService, IHttpClientFactory httpClientFactory)
    {
        testService = _testService;
        httpClient = httpClientFactory.CreateClient();
    }

    /// <summary>
    /// 依赖注入测试
    /// </summary>
    /// <returns></returns>
    [HttpGet("di")]
    public string DI()
    {
        return testService.SayService("DI,Success!");
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
    /// 获取客户端IP
    /// </summary>
    /// <returns></returns>
    [HttpGet("getIP")]
    public IActionResult GetIP()
    {
        var ip = Request.Headers["X-Forwarded-For"].FirstOrDefault() ?? HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString();
        return Ok(ip);
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
    /// 根据经纬度查询具体位置信息
    /// </summary>
    /// <param name="lng">经度</param>
    /// <param name="lat">纬度</param>
    /// <returns></returns>
    [HttpGet("reverseGeocoding")]
    public async Task<IActionResult> ReverseGeocoding(string lng, string lat)
    {
        if (string.IsNullOrWhiteSpace(lat) || string.IsNullOrWhiteSpace(lng))
            return BadRequest("经纬度不能为空");

        string api = $"https://api.map.baidu.com/reverse_geocoding/v3/?ak=HiQwVOsgoVBbq0DNw6vT6WtfrPCGEc6R&output=json&coordtype=wgs84ll&location={lat},{lng}";
        var res = await httpClient.GetStringAsync(api).ConfigureAwait(false);
        return Ok(res);
    }
}
