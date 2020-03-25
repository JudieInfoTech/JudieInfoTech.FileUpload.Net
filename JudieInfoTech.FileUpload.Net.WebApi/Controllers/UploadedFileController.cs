using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using Newtonsoft.Json;
using JudieInfoTech.FileUpload.Net.Resources;
using JudieInfoTech.FileUpload.Net.Resources.Shared;
using System.Net;
using JudieInfoTech.FileUpload.Net.Resources.Service;


namespace JudieInfoTech.FileUpload.Net.WebUI.WebApi.Controllers
{
    [Route("api/[controller]")]
    public class UploadedFileController : Controller
    {
        private readonly IUploadedFileResourceService ResourceService;

        public UploadedFileController(IUploadedFileResourceService resourceService)
        {
            ResourceService = resourceService;
        }


        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(Int32 id)
        {
            var resource = await ResourceService.FindAsync(id);

            return (resource == null) ? NotFound() as IActionResult : Json(resource);
        }


        [HttpGet("Create")]
        public IActionResult Create()
        {
            var resource = ResourceService.Create();

            return Json(resource);
        }


        [HttpGet]
        public IActionResult Get(String sortBy, String sortDirection, Int32 skip, Int32 take, String search, String searchFields)
        {
            var result = ResourceService.Load(sortBy, sortDirection, skip, take, search, searchFields);

            return Json(result);
        }


        [HttpGet("{code}")]
        public IActionResult Get(String code)
        {
            if (code.IsNullOrEmpty())
                return BadRequest();

            code = code.ToUpper();

            UploadedFileResource result = null;

            switch (code.Length)
            {
                case 2:
                    result = ResourceService.Items().Where(c => c.ImageFilename == code).FirstOrDefault();
                    break;

                case 3:
                    result = ResourceService.Items().Where(c => c.ImageFilePath  == code).FirstOrDefault();
                    break;
            }

            return (result == null) ? NotFound() as IActionResult : Json(result);
        }


        [HttpPost]
        public async Task<IActionResult> Post([FromBody]UploadedFileResource resource)
        {
            try
            {
                // Create rescource
                var serviceResult = await ResourceService.InsertAsync(resource);

                // if return error message if needed
                if (serviceResult.Errors.Count > 0)
                    return BadRequest(serviceResult);

                // On succes return url with id and newly created resource  
                return CreatedAtAction(nameof(Get), new { id = serviceResult.Resource.Id }, serviceResult.Resource);
            }
            catch (Exception ex)
            {
                var result = new ResourceResult<UploadedFileResource>(resource);

                while (ex != null)
                    result.Exceptions.Add(ex.Message);

                return BadRequest(result);
            }
        }


        [HttpPut]
        public async Task<IActionResult> Put([FromBody]UploadedFileResource resource)
        {
            try
            {
                var currentResource = await ResourceService.FindAsync(resource.Id);

                if (currentResource == null)
                    return NotFound();

                var serviceResult = await ResourceService.UpdateAsync(resource);

                if (serviceResult.Errors.Count > 0)
                    return BadRequest(serviceResult);

                return Ok(serviceResult.Resource);
            }
            catch (Exception ex)
            {
                var result = new ResourceResult<UploadedFileResource>(resource);

                while (ex != null)
                {
                    result.Exceptions.Add(ex.Message);

                    if (ex is ConcurrencyException)
                        return StatusCode(HttpStatusCode.Conflict.ToInt32(), result);

                    ex = ex.InnerException;
                }

                return BadRequest(result);
            }
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Int32 id)
        {
            try
            {
                var serviceResult = await ResourceService.DeleteAsync(id);

                if (serviceResult.Resource == null)
                    return NoContent();

                if (serviceResult.Errors.Count > 0)
                    return BadRequest(serviceResult);

                return Ok();
            }
            catch (Exception ex)
            {
                var result = new ResourceResult<UploadedFileResource>();

                while (ex != null)
                    result.Exceptions.Add(ex.Message);

                return BadRequest(result);
            }
        }
    }
}