using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace JudieInfoTech.FileUpload.Net.WebUI.Controllers.Models
{
    public class UploadedFileModel
    {
        public Int32 Id { get; set; }

        public String ImageFilename { get; set; }

        public String Comments { get; set; }

        public String ImageFilePath { get; set; }

        public IFormFile FileToUpload { get; set; }

        public String RowVersion { get; set; }
    }
}
