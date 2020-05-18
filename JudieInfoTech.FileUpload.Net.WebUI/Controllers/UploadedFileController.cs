using AutoMapper;
using JudieInfoTech.FileUpload.Net.Resources;
using JudieInfoTech.FileUpload.Net.Resources.Shared;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using JudieInfoTech.FileUpload.Net.WebUI.Controllers.Models;
using JudieInfoTech.FileUpload.Net.WebUI.Controllers.Mappings;
using JudieInfoTech.FileUpload.Net.Azure.Service;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace JudieInfoTech.FileUpload.Net.WebUI.Controllers
{
    public class UploadedFileController : Controller
    {
        // delegate for easy switching between insert or update
        private delegate Task<HttpResponseMessage> Upsert(String requestUri, HttpContent content);

        private readonly IMapper mapper;
        private readonly String apiUrl;
        private HttpClient apiClient;
        IHostingEnvironment env;

        public UploadedFileController(HttpClient client)
        {
            apiClient = client;

            apiUrl = "/api/uploadedfile/";

            // Setup AutoMapper for mapping between Resource and Model
            var config = new AutoMapper.MapperConfiguration(cfg =>
            {
                cfg.AddProfiles(typeof(UploadedFileMapping).GetTypeInfo().Assembly);
            });

            mapper = config.CreateMapper();
        }


        public IActionResult Index()
        {
            return View();
        }


        [HttpGet]
        public async Task<IActionResult> Load(String sort, String order, Int32 offset, Int32 limit, String search, String searchFields)
        {
            //  setup url with query parameters
            var queryString = new Dictionary<String, String>();
            queryString["sortBy"] = sort ?? "";
            queryString["sortDirection"] = order ?? "";
            queryString["skip"] = offset.ToString();
            queryString["take"] = limit.ToString();
            queryString[nameof(search)] = search ?? "";
            queryString[nameof(searchFields)] = searchFields ?? "";

            // convert dictionary to query params
            var uriBuilder = new UriBuilder(apiClient.BaseAddress + apiUrl)
            {
                Query = QueryHelpers.AddQueryString("", queryString)
            };

            using (var response = await apiClient.GetAsync(uriBuilder.Uri))
            {
                var document = await response.Content.ReadAsStringAsync();

                var loadResult = JsonConvert.DeserializeObject<LoadResult<UploadedFileResource>>(document);

                // Convert loadResult into Bootstrap-Table compatible format
                var result = new
                {
                    total = loadResult.CountUnfiltered,
                    rows = loadResult.Items
                };

                return Json(result);
            }
        }


        [HttpGet]
        public async Task<IActionResult> Edit(Int32 id)
        {
            String url = apiUrl + ((id == 0) ? "create" : $"{id}");

            using (var response = await apiClient.GetAsync(url))
            {
                var document = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var resource = JsonConvert.DeserializeObject<UploadedFileResource>(document);

                    var result = mapper.Map<UploadedFileModel>(resource);

                    return PartialView(nameof(Edit), result);
                }

                else
                {
                    var result = new ResourceResult<UploadedFileResource>();

                    if (response.StatusCode == HttpStatusCode.NotFound)
                        result.Errors.Add(new ValidationError($"Record with id {id} is not found"));

                    return StatusCode(response.StatusCode.ToInt32(), result);
                }
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromForm]UploadedFileModel model)
        {
            if (!ModelState.IsValid)
                PartialView();

            if (model.FileToUpload != null)
            {
                //var uploads = Path.Combine(env.WebRootPath, "uploads");
                var uploads = Path.Combine("C:/", "uploads");
                bool exists = Directory.Exists(uploads);
                if (!exists)
                    Directory.CreateDirectory(uploads);

                var fileName = Path.GetFileName(model.FileToUpload.FileName);
                var fileStream = new FileStream(Path.Combine(uploads, model.FileToUpload.FileName), FileMode.Create);
                string mimeType = model.FileToUpload.ContentType;
                byte[] fileData = new byte[model.FileToUpload.Length];


                BlobStorageService objBlobService = new BlobStorageService();

                model.ImageFilePath = objBlobService.UploadFileToBlob(model.ImageFilename,model.FileToUpload.FileName, fileData, mimeType);

            }

             
             
            // Map model to resource
            var resource = mapper.Map<UploadedFileResource>(model);

            // save resource to Json
            var resourceDocument = JsonConvert.SerializeObject(resource);

            using (var content = new StringContent(resourceDocument, Encoding.UTF8, "application/json"))
            {
                // determen call update or insert
                Upsert upsert = apiClient.PutAsync;

                // no RowVersion indicates insert
                if (model.RowVersion.IsNullOrEmpty())
                    upsert = apiClient.PostAsync;

                using (var response = await upsert(apiUrl, content))
                {
                    // init result
                    var result = new ResourceResult<UploadedFileResource>(resource);

                    // read result from RESTful service
                    var responseDocument = await response.Content.ReadAsStringAsync();

                    if (response.StatusCode == HttpStatusCode.OK ||
                        response.StatusCode == HttpStatusCode.Created)
                    {
                        // Fetch created or updated resource from response
                        result.Resource = JsonConvert.DeserializeObject<UploadedFileResource>(responseDocument); ;
                    }
                    else
                    {
                        // fetch errors and or exceptions
                        result = JsonConvert.DeserializeObject<ResourceResult<UploadedFileResource>>(responseDocument);
                    }

                    // Set error message for concurrency error
                    if (response.StatusCode == HttpStatusCode.Conflict)
                    {
                        result.Errors.Clear();
                        result.Errors.Add(new ValidationError("This record is modified by another user"));
                        result.Errors.Add(new ValidationError("Your work is not saved and replaced with new content"));
                        result.Errors.Add(new ValidationError("Please review the new content and if required edit and save again"));
                    }

                    if (response.StatusCode.IsInSet(HttpStatusCode.OK, HttpStatusCode.Created, HttpStatusCode.Conflict))
                        return StatusCode(response.StatusCode.ToInt32(), result);

                    // copy errors so they will be rendered in edit form
                    foreach (var error in result.Errors)
                        ModelState.AddModelError(error.MemberName ?? "", error.Message);

                    // Update model with Beautify effect(s) and make it visible in the partial view
                    IEnumerable<PropertyInfo> properties = model.GetType().GetTypeInfo().GetProperties(BindingFlags.Public | BindingFlags.Instance);

                    foreach (var property in properties)
                    {
                        var rawValue = property.GetValue(model);
                        var attemptedValue = rawValue == null ? "" : Convert.ToString(rawValue, CultureInfo.InvariantCulture);

                        ModelState.SetModelValue(property.Name, rawValue, attemptedValue);
                    }

                    // No need to specify model here, it has no effect on the render process :-(
                    return PartialView();
                   
                }
            }
            
        }


        [HttpPost]
        public async Task<IActionResult> Delete(Int32 id)
        {
            String url = apiUrl + $"{id}";

            using (var response = await apiClient.DeleteAsync(url))
            {
                var responseDocument = await response.Content.ReadAsStringAsync();

                // create only response if somethomg off has happenend
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    var result = JsonConvert.DeserializeObject<ResourceResult<UploadedFileResource>>(responseDocument);

                    return StatusCode(response.StatusCode.ToInt32(), result);
                }

                return Content(null);
            }
        }
    }
}
