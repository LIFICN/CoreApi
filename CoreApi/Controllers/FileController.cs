using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace CoreApi.Controllers;

/// <summary>
/// 文件上传下载示例
/// </summary>
[AllowAnonymous]
[ApiVersion("1.0")]
public class FileController : BaseController
{
    public static string FilePath => $"{AppContext.BaseDirectory}files";

    /// <summary>
    /// 多文件上传
    /// </summary>
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> FileUpload(List<IFormFile> fileCollection)
    {
        //var fileCollection = HttpContext.Request.Form.Files;
        if (fileCollection == null || fileCollection.Count == 0)
            return BadRequest("上传文件不能为空");

        if (!Directory.Exists(FilePath)) Directory.CreateDirectory(FilePath);

        foreach (IFormFile item in fileCollection)
        {
            using FileStream fileStream = new FileStream($@"{FilePath}\{item.FileName}", FileMode.Create);
            await item.CopyToAsync(fileStream).ConfigureAwait(false);
            await fileStream.FlushAsync().ConfigureAwait(false);
        }

        return Ok("文件上传成功");
    }

    /// <summary>
    /// 文件下载
    /// </summary>
    [HttpPost("download/{fileName}")]
    public IActionResult FileDownload(string fileName)
    {
        var path = $@"{FilePath}\{fileName}";
        if (!System.IO.File.Exists(path)) return BadRequest("文件不存在");

        var fileStream = System.IO.File.OpenRead(path);
        return File(fileStream, "application/octet-stream", fileName);
    }
}
