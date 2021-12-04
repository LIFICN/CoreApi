using CoreApi.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CoreApi.Controllers;

/// <summary>
/// JWT 示例
/// </summary>
[ApiVersionNeutral]
public class JwtTestController : BaseController
{
    public JwtTestController()
    {
    }

    [HttpGet("getToken")]
    [AllowAnonymous]
    public IActionResult GetToken()
    {
        var data = new Dictionary<string, string>();
        data["user_id"] = "111";
        var token = JwtExtension.CreateToken(data, DateTime.Now.AddSeconds(50));
        return Ok(token);
    }

    [HttpGet("testToken")]
    public IActionResult TestToken()
    {
        return Ok(User.Claims.Select(d => new { type = d.Type, value = d.Value }).ToList());
    }
}
