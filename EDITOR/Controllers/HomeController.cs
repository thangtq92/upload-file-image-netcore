using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using EDITOR.Models;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Net.Http.Headers;

namespace EDITOR.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        /// <summary>
        /// Upload Image to Ckeditors
        /// </summary>
        /// <param name="upload"></param>
        /// <param name="CKEditorFuncNum"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task UploadImage(IList<IFormFile> upload, string CKEditorFuncNum) //, string CKEditor, string langCode
        {
            DateTime now = DateTime.Now;
            if (upload.Count == 0)
            {
                await HttpContext.Response.WriteAsync("Yêu cầu nhập ảnh");
            }
            else
            {
                var file = upload[0];
                // Lấy tên file
                var fileName = ContentDispositionHeaderValue
                                    .Parse(file.ContentDisposition)
                                    .FileName
                                    .Trim('"').Replace(" ", "-").Replace("_", "-").ToLower();

                var imageFolder = $@"/wwwroot/upload/images/{now.ToString("yyyyMMdd")}";

                string folder = Directory.GetCurrentDirectory() + imageFolder;
                // Kiểm tra Folder đã tồn tại chưa? Thêm mới Folder
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                // Tạo đường dẫn tuyệt đối file
                var filePath = Path.Combine(folder, fileName);

                // Thêm mới file vào Folder
                using (FileStream fs = System.IO.File.Create(filePath))
                {
                    file.CopyTo(fs);
                    fs.Flush();
                }
                // Tạo URL trả ra giao diện
                var urlFolder = $@"/upload/images/{now.ToString("yyyyMMdd")}";
                var url = Path.Combine(urlFolder, fileName).Replace(@"\", @"/");

                // Thực hiện CallFunction CKeditor hiển thị dữ liệu ra Form (CKEditorFuncNum +'url')
                var textResult = "<script>window.parent.CKEDITOR.tools.callFunction(" + CKEditorFuncNum + ", '" + url + "', ''" + ");</script>";
                await HttpContext.Response.WriteAsync(textResult);

            }

        }
    }
}
