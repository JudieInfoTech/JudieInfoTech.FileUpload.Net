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
        [StringLength(50, MinimumLength = 2, ErrorMessage = "File Name must be 2 to 50 chars long")]
        public String ImageFilename { get; set; }
        [Required ]
        [StringLength(2000, MinimumLength = 2, ErrorMessage = "Comments must be 2 to 2000 chars long")]
        public String Comments { get; set; }
        [Required]
        [StringLength(250, MinimumLength = 2, ErrorMessage = "File path must be 2 to 250 chars long")]
        public String ImageFilePath { get; set; }

        public IFormFile File { get; set; }
    }
}
