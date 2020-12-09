using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CoreApi.Controllers
{
    /// <summary>
    /// 文件上传下载示例
    /// </summary>
    [AllowAnonymous]
    [ApiVersion("1.0")]
    public class FileController : BaseController
    {
        public string FilePath { get => $"{AppContext.BaseDirectory}Files"; }

        /// <summary>
        /// 多文件上传
        /// </summary>
        [HttpPost("upload")]
        public async ValueTask<IActionResult> FileUpload(List<IFormFile> fileCollection)
        {
            //var fileCollection = HttpContext.Request.Form.Files;
            if (fileCollection == null || fileCollection.Count == 0)
                return BadRequest("上传文件不能为空");

            if (!Directory.Exists(FilePath))
                Directory.CreateDirectory(FilePath);

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
            if (!System.IO.File.Exists(path))
                return BadRequest("文件不存在");

            var fileStream = System.IO.File.OpenRead(path);
            return File(fileStream, "application/octet-stream", fileName);
        }

        /// <summary>
        /// 文件切片合并
        /// </summary>
        /// <param name="mergeFileName">合并后的文件名</param>
        /// <param name="sliceName">单个文件切片的文件名(切片标识符)</param>
        /// <param name="sliceType">单个文件切片的格式(如:.txt,.zip)</param>
        /// <returns></returns>
        [HttpGet("merge")]
        public async ValueTask<IActionResult> MergeFiles(string mergeFileName, string sliceName, string sliceType)
        {
            var filePath = $"{FilePath}/{mergeFileName}";
            if (System.IO.File.Exists(filePath)) return BadRequest("文件已存在,合并失败");

            var sliceNameLength = sliceName.Length;
            var rootLength = FilePath.Length;
            var sliceTypeLength = sliceType.Length;
            var list = Directory.EnumerateFiles(FilePath).Where(d => d.LastIndexOf(sliceName) > -1).OrderBy(d =>
            {
                var span = d.AsSpan(rootLength + 1).Slice(sliceNameLength);
                return int.Parse(span.Slice(0, span.Length - sliceTypeLength));
            }).ToArray();

            if (list == null || !list.Any()) return BadRequest("文件切片不存在,合并失败");

            using FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Write);
            foreach (var path in list)
            {
                using FileStream sliceStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                await sliceStream.CopyToAsync(fileStream).ConfigureAwait(false);
                sliceStream.Flush();
            }

            fileStream.Flush();

            foreach (var path in list)
                System.IO.File.Delete(path);

            return Ok("合并成功");
        }
    }
}