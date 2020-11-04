using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using EDITOR.Infrastructure.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EDITOR.Models.Uploads;
using EDITOR.Extensions;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace EDITOR.Controllers
{
    public class UploadController : Controller
    {
        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Upload Image to Ckeditors
        /// </summary>
        /// <param name="upload"></param>
        /// <param name="CKEditorFuncNum"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task UploadImageForCKEditor(IList<IFormFile> upload, string CKEditorFuncNum) //, string CKEditor, string langCode
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

                var urlFolderDefault = $@"/upload/image-form/{now.ToString("yyyyMMdd")}";
                var imageFolder = "/wwwroot" + urlFolderDefault;

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
                var url = Path.Combine(urlFolderDefault, fileName).Replace(@"\", @"/");

                // Thực hiện CallFunction CKeditor hiển thị dữ liệu ra Form (CKEditorFuncNum +'url')
                var textResult = "<script>window.parent.CKEDITOR.tools.callFunction(" + CKEditorFuncNum + ", '" + url + "', ''" + ");</script>";
                await HttpContext.Response.WriteAsync(textResult);

            }

        }


        /// <summary>
        /// Upload image for form
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IActionResult UploadImage()
        {
            DateTime now = DateTime.Now;
            var files = Request.Form.Files;
            if (files.Count == 0)
            {
                return new BadRequestObjectResult(files);
            }
            else
            {
                var file = files[0];
                var extension = "." + file.FileName.Split('.')[file.FileName.Split('.').Length - 1];
                var filename = ContentDispositionHeaderValue
                                    .Parse(file.ContentDisposition)
                                    .FileName
                                    .Trim('"').Replace(extension, "");
                var urlFolderDefault = $@"/upload/image-form/{now.ToString("yyyyMMdd")}";

                var imageFolder = "/wwwroot" + urlFolderDefault;

                string folder = Directory.GetCurrentDirectory() + imageFolder;

                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                var fileNameAlias = TextHelper.ToUnsignString(filename) + extension;

                string filePath = Path.Combine(folder, fileNameAlias);

                using (FileStream fs = System.IO.File.Create(filePath))
                {
                    file.CopyTo(fs);
                    fs.Flush();
                }
                return new OkObjectResult(Path.Combine(urlFolderDefault, fileNameAlias).Replace(@"\", @"/"));
            }
        }

        /// <summary>
        /// Upload Multiple Files
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> UploadFiles(List<IFormFile> files)
        {
            if (files == null || files.Count == 0)
                return Content("files not selected");
            DateTime now = DateTime.Now;

            var urlFolderDefault = $@"/upload/files/{now.ToString("yyyyMMdd")}";
            var uploadFolder = "/wwwroot" + urlFolderDefault;

            string folder = Directory.GetCurrentDirectory() + uploadFolder;

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            var model = new FilesViewModel();

            foreach (var file in files)
            {
                // This is path file of folder root
                var path = Path.Combine(
                       //Directory.GetCurrentDirectory(), "wwwroot",
                       folder, file.GetFilename());

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // This is pathFile on Server

                var pathFile = Path.Combine(urlFolderDefault, file.GetFilename()).Replace(@"\", @"/");

                // Add Name, Path to List to return
                model.Files.Add(
                    new FileDetails { Name = file.GetFilename(), Path = pathFile });
            }
            return new OkObjectResult(model);
            //return RedirectToAction("Files");
            //return new OkObjectResult(Path.Combine(uploadFolder, file.GetFilename()).Replace(@"\", @"/"));
        }

        /// <summary>
        /// Download a file from Server
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public async Task<IActionResult> Download(string fileName)
        {

            DateTime now = DateTime.Now;
            var urlFolderDefault = $@"/upload/files/{now.ToString("yyyyMMdd")}";
            var uploadFolder = "/wwwroot" + urlFolderDefault;

            string folder = Directory.GetCurrentDirectory() + uploadFolder;


            if (fileName == null)
                return Content("filename not present");

            var path = Path.Combine(folder, fileName);

            var memory = new MemoryStream();
            using (var stream = new FileStream(path, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;
            return File(memory, GetContentType(path), Path.GetFileName(path));
        }

        private string GetContentType(string path)
        {
            var types = GetMimeTypes();
            var ext = Path.GetExtension(path).ToLowerInvariant();
            return types[ext];
        }

        private Dictionary<string, string> GetMimeTypes()
        {
            return new Dictionary<string, string>
            {
                {".txt", "text/plain"},
                {".pdf", "application/pdf"},
                {".doc", "application/vnd.ms-word"},
                {".docx", "application/vnd.ms-word"},
                {".xls", "application/vnd.ms-excel"},
                {".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"},
                {".png", "image/png"},
                {".jpg", "image/jpeg"},
                {".jpeg", "image/jpeg"},
                {".gif", "image/gif"},
                {".csv", "text/csv"}
            };
        }


        //public IActionResult Files()
        //{
        //    var model = new FilesViewModel();
        //    foreach (var item in this.fileProvider.GetDirectoryContents(""))
        //    {
        //        model.Files.Add(
        //            new FileDetails { Name = item.Name, Path = item.PhysicalPath });
        //    }
        //    return View(model);
        //}
    }
}
