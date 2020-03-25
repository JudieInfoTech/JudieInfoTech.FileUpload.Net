using AutoMapper;
using JudieInfoTech.FileUpload.Net.Entities;
using JudieInfoTech.FileUpload.Net.Entities.Mappings;
using JudieInfoTech.FileUpload.Net.Resources;
using JudieInfoTech.FileUpload.Net.Resources.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
namespace JudieInfoTech.FileUpload.Net.Resources.Service
{
    public class UploadedFileResourceService : IUploadedFileResourceService, IDisposable
    {
        private readonly IMapper mapper;
        protected EntityContext EntityContext { get; private set; }

        public UploadedFileResourceService(EntityContext entityContext)
        {
            EntityContext = entityContext;

            // Setup AutoMapper between Resource and Entity
            var config = new AutoMapper.MapperConfiguration(cfg =>
            {
                cfg.AddProfiles(typeof(UploadedFileMapping ).GetTypeInfo().Assembly);
            });

            mapper = config.CreateMapper();
        }


        public UploadedFileResource  Create()
        {
            var result = new UploadedFileResource() { CreatedAt = DateTime.Now };

            return result;
        }

        protected virtual void BeautifyResource(UploadedFileResource resource)
        {
            // Only letter are allowed in codes
            //resource.Code2 = resource.Code2?.ToUpperInvariant()?.ToLetter();
            //resource.Code3 = resource.Code3?.ToUpperInvariant()?.ToLetter();

            //resource.Name = resource.Name?.Trim();
            resource.ImageFilename = resource.ImageFilename?.Trim();
            resource.Comments = resource.Comments?.Trim();
        }


        /// <summary>
        ///  Perform basic validation
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="errors"></param>
        protected void ValidateAttributes(UploadedFileResource resource, IList<ValidationError> errors)
        {
            var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(resource);
            var validationResults = new List<ValidationResult>(); ;

            Validator.TryValidateObject(resource, validationContext, validationResults, true);

            foreach (var item in validationResults)
                errors.Add(new ValidationError(item.ErrorMessage, item.MemberNames?.FirstOrDefault() ?? ""));
        }


        protected virtual void ValidateBusinessRules(UploadedFileResource resource, IList<ValidationError> errors)
        {
            // Check if Code2 and Code3 are unique 
            //var code2Check = Items().Where(r => r.Code2 == resource.Code2);
            //var code3Check = Items().Where(r => r.Code3 == resource.Code3);
            var filename2check = Items().Where(r => r.ImageFilename == resource.ImageFilename);

            // Check if Id is unique for new resource
            if (resource.RowVersion.IsNullOrEmpty())
            {
                if (Items().Where(r => r.Id == resource.Id).Count() > 0)
                    errors.Add(new ValidationError($"{resource.Id} is already taken", nameof(resource.Id)));
            }
            else
            {
                // Existing resource, skip resource itself in unique check
                //code2Check = code2Check.Where(r => r.Code2 == resource.Code2 && r.Id != resource.Id);
                //code3Check = code3Check.Where(r => r.Code3 == resource.Code3 && r.Id != resource.Id);
                filename2check = filename2check.Where(r => r.ImageFilename == resource.ImageFilename && r.Id != resource.Id);
            }

            // set error message
            //if (code2Check.Count() > 0)
            //  errors.Add(new ValidationError($"{resource.Code2} already exist", nameof(resource.Code2)));

            //if (code3Check.Count() > 0)
            //  errors.Add(new ValidationError($"{resource.Code3} already exist", nameof(resource.Code3)));
            if (filename2check.Count() > 0)
                errors.Add(new ValidationError($"{resource.ImageFilename} already exist", nameof(resource.ImageFilename)));
        }


        protected virtual void ValidateDelete(UploadedFileResource resource, IList<ValidationError> errors)
        {
            //if (resource.Code2.EqualsEx("NL"))
            //{
            //    errors.Add(new ValidationError("It's not allowed to delete the Low Lands! ;-)"));
            //}

            //if (resource.Code2.EqualsEx("BE"))
            //{
            //    errors.Add(new ValidationError("Without Belgium no great Beer!"));
            //}
            if(resource.ImageFilename.EqualsEx("TEST"))
            {
                errors.Add(new ValidationError("This is default record"));
            }
        }


        public async Task<UploadedFileResource> FindAsync(Int32 id)
        {
            // Fetch entity from storage
            var entity = await EntityContext.FindAsync<UploadedFile>(id);

            // Convert emtity to resource
            var result = mapper.Map<UploadedFileResource>(entity);

            return result;
        }


        public IQueryable<UploadedFileResource> Items()
        {
            var entities = Enumerable.AsEnumerable(EntityContext.UploadedFiles);
            var result = mapper.Map<IEnumerable<UploadedFile>, IEnumerable<UploadedFileResource>>(entities);

            return result.AsQueryable();
        }


        private IEnumerable<String> CreateFieldNames(IQueryable items, String searchFields = "")
        {
            IEnumerable<String> fieldNames = searchFields.Split(new Char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

            IEnumerable<String> propertyNames = items.ElementType.GetProperties(BindingFlags.Public | BindingFlags.Instance).Select(p => p.Name).ToList();

            // Use only valid field names
            IEnumerable<String> result = fieldNames.Where(n => propertyNames.Contains(n)).ToList();

            return result;
        }


        // needs System.Linq.Dynamic.Core
        private IQueryable SearchItems(IQueryable items, String sortBy, String sortDirection, Int32 skip, Int32 take, String search, String searchFields)
        {
            IEnumerable<String> propertyNames = items.ElementType.GetProperties(BindingFlags.Public | BindingFlags.Instance).Select(p => p.Name).ToList();

            // Apply filtering to all visible column names
            if (!String.IsNullOrEmpty(search))
            {
                // Use only valid fieldnames
                IEnumerable<String> fieldNames = CreateFieldNames(items, searchFields);

                StringBuilder sb = new StringBuilder();

                // create dynamic Linq expression
                foreach (String fieldName in fieldNames)
                    sb.AppendFormat("({0} == null ? false : {0}.ToString().IndexOf(@0, @1) >=0) or {1}", fieldName, Environment.NewLine);

                String searchExpression = sb.ToString();
                // remove last "or" occurrence
                searchExpression = searchExpression.Substring(0, searchExpression.LastIndexOf("or"));

                // Apply filtering
                items = items.Where(searchExpression, search, StringComparison.OrdinalIgnoreCase);
            }

            // Skip requires sorting, so make sure there is always sorting
            String sortExpression = "";

            if (!String.IsNullOrEmpty(sortBy))
            {
                sortExpression += String.Format("{0} {1}", sortBy, sortDirection);
                items = items.OrderBy(sortExpression);
            }

            // show 100 records if limit is not set
            if (take == 0)
                take = 100;

            items = items.Skip(skip).Take(take);

            return items;
        }


        public LoadResult<UploadedFileResource> Load(String sortBy, String sortDirection, Int32 skip, Int32 take, String search, String searchFields)
        {
            IQueryable entities = EntityContext.UploadedFiles.AsQueryable();

            // where clause is set, count all records
            Int32 count = entities.Count();

            // Perform filtering, ordering and paging
            entities = SearchItems(entities, sortBy, sortDirection, skip, take, search, searchFields);

            // Prepare result
            var result = new LoadResult<UploadedFileResource>()
            {
                CountUnfiltered = count,
                Items = mapper.Map<IList<UploadedFile>, IList<UploadedFileResource>>(entities.ToDynamicList<UploadedFile>())
            };

            return result;
        }


        public async Task<ResourceResult<UploadedFileResource>> InsertAsync(UploadedFileResource resource)
        {
            // Fields are set by persistance service 
            resource.CreatedBy = null;

            resource.ModifiedAt = null;
            resource.ModifiedBy = null;

            resource.RowVersion = null;

            return await UpsertAsync(resource);
        }


        public async Task<ResourceResult<UploadedFileResource>> UpdateAsync(UploadedFileResource resource)
        {
            return await UpsertAsync(resource);
        }


        public async Task<ResourceResult<UploadedFileResource>> UpsertAsync(UploadedFileResource resource)
        {
            var result = new ResourceResult<UploadedFileResource >();

            // Beautify before validation and make validation more succesfull
            BeautifyResource(resource);

            // save beautify effect effect 
            result.Resource = resource;

            // Apply simple validation on attribute level
            ValidateAttributes(resource, result.Errors);

            // Apply complex business rules validation
            ValidateBusinessRules(resource, result.Errors);

            // Save is only usefull when error free
            if (result.Errors.Count == 0)
            {
                // Convert resource to entity
                var entity = mapper.Map<UploadedFile>(resource);

                // save entity
                await EntityContext.UpsertAsync(entity);

                // convert save result back to resource and get database created values like auto incremental field and timestamps.
                result.Resource = mapper.Map<UploadedFileResource>(entity);
            }

            return result;
        }


        public async Task<ResourceResult<UploadedFileResource>> DeleteAsync(Int32 id)
        {
            var result = new ResourceResult<UploadedFileResource>();

            // Check if resource still exists
            result.Resource = await FindAsync(id);

            if (result.Resource != null)
            {
                // Check if delete is allowed 
                ValidateDelete(result.Resource, result.Errors);

                // Delete only if allowed
                if (result.Errors.Count == 0)
                {
                    var entity = mapper.Map<UploadedFile>(result.Resource);

                    await EntityContext.DeleteAsync(entity);
                }
            }

            return result;
        }

        #region IDisposable Support
        protected virtual void Dispose(Boolean isDisposing)
        {
            if (isDisposing && EntityContext != null)
            {
                EntityContext.Dispose();
                EntityContext = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
