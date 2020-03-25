using JudieInfoTech.FileUpload.Net.Resources;
using JudieInfoTech.FileUpload.Net.Resources.Shared;
using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
namespace JudieInfoTech.FileUpload.Net.Resources
{

    public class UploadedFileResource : Resource<Int32>
    {
        //[Required]
        //[Range(1, 999)]
        public override Int32 Id { get; set; }

        [Required]
        [StringLength(50)]
        public String ImageFilename { get; set; }
        [Required ]
        [StringLength(2000)]
        public String Comments { get; set; }

        [StringLength(250)]
        public String ImageFilePath { get; set; }

        public IFormFile File { get; set; }
    }
}
