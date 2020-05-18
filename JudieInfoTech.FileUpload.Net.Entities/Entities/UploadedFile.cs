using JudieInfoTech.FileUpload.Net.Entities.Shared;
using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JudieInfoTech.FileUpload.Net.Entities
{

    [Table("UploadedFiles")]
    public class UploadedFile : Entity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Int32 Id { get; set; }


        [Required]
        [StringLength(50)]
        public String ImageFilename { get; set; }

        [Required]
        [StringLength(2000)]
        public String Comments { get; set; }
        [Required]
        [StringLength(250)]
        public String ImageFilePath { get; set; }

        [NotMapped]
        public IFormFile File { get; set; }

    }
}
