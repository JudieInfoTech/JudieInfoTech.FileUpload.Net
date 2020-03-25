using JudieInfoTech.FileUpload.Net.Entities;
using JudieInfoTech.FileUpload.Net.Resources.Service;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Swashbuckle.AspNetCore.Swagger;
using System;
using System.Data.Common;
using System.Reflection;
using System.Security.Principal;

namespace JudieInfoTech.FileUpload.Net.WebApi
{
  public class Startup
  {
    public IConfigurationRoot Configuration { get; }

    public Startup(IHostingEnvironment env)
    {
      var builder = new ConfigurationBuilder()
          .SetBasePath(env.ContentRootPath)
          .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
          .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
          .AddEnvironmentVariables();
      Configuration = builder.Build();
    }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
      // Get Database connection config
      var connectionString = Configuration.GetConnectionString("DbConnection");

      // Connect by default SqlServer other wise to MySql 
      var databaseDriver = Configuration.GetConnectionString("DatabaseDriver");

      // Setup Database Service layer used in CountryResourceService
      if (databaseDriver.EqualsEx("MySQL"))
        services.AddDbContext<EntityContext>(options => options.UseMySql(connectionString));
      else
        services.AddDbContext<EntityContext>(options => options.UseSqlServer(connectionString));


      // Setup ResourceService
      services.AddTransient<IUploadedFileResourceService, UploadedFileResourceService>();
      services.AddTransient<IIdentity, IdentityResolverService>();

      // Add framework services.
      services.AddMvc()
        .AddJsonOptions(options =>
        {
          // Override default camelCase style (yes its strange the default configuration results in camel case)
          options.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver();
        });

      // Register the Swagger generator, defining one or more Swagger documents
      services.AddSwaggerGen(c =>
      {
        c.SwaggerDoc("v1", new Info { Title = "Demo Api", Version = "v1" });
      });

      var config = new AutoMapper.MapperConfiguration(cfg =>
      {
        cfg.AddProfiles(Assembly.GetEntryAssembly());
      });

      var mapper = config.CreateMapper();
      services.AddSingleton(mapper);
    }


    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
    {
      loggerFactory.AddConsole(Configuration.GetSection("Logging"));
      loggerFactory.AddDebug();

      app.UseMvc();

      // Enable middleware to serve generated Swagger as a JSON endpoint.
      app.UseSwagger();

      // Enable middleware to serve swagger-ui (HTML, JS, CSS etc.), specifying the Swagger JSON endpoint.
      app.UseSwaggerUI(c =>
      {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Demo Api v1");
      });
    }



  }
}
