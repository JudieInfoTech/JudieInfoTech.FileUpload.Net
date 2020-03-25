using AutoMapper;
using JudieInfoTech.FileUpload.Net.WebUI.Controllers.Models;

namespace JudieInfoTech.FileUpload.Net.WebUI.Controllers.Mappings
{
    public class UploadedFileMapping : Profile
    {
        public UploadedFileMapping()
        {
            // 2 way mapping resource <==> ViewModel
            CreateMap<JudieInfoTech.FileUpload.Net.Resources.UploadedFileResource, UploadedFileModel>();
            CreateMap<UploadedFileModel, JudieInfoTech.FileUpload.Net.Resources.UploadedFileResource>();
        }
    }
}
