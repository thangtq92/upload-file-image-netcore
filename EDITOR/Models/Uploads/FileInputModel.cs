using System;
using Microsoft.AspNetCore.Http;

namespace EDITOR.Models.Uploads
{
    public class FileInputModel
    {
        public IFormFile FileToUpload { get; set; }
    }
}
