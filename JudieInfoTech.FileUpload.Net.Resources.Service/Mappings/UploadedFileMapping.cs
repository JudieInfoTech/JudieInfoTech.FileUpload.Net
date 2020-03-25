using AutoMapper;
using JudieInfoTech.FileUpload.Net.Entities;
using System;

namespace JudieInfoTech.FileUpload.Net.Entities.Mappings
{
    public class UploadedFileMapping : Profile
    {
        public UploadedFileMapping()
        {
            // 2 way mapping resource <==> entity model
            CreateMap<JudieInfoTech.FileUpload.Net.Resources.UploadedFileResource, UploadedFile >();
            CreateMap<UploadedFile, JudieInfoTech.FileUpload.Net.Resources.UploadedFileResource>();
        }
    }
}
