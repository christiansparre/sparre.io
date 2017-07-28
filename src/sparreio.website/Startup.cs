// Copyright (c) Christian Sparre. All rights reserved. 
// Licensed under the MIT License, see LICENSE.txt in the repository root for license information.

using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.Storage;
using sparreio.website.Services;
using WilderMinds.MetaWeblog;

namespace sparreio.website
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IHostingEnvironment Environment { get; }

        public Startup(IConfiguration configuration, IHostingEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }
        
        public void ConfigureServices(IServiceCollection services)
        {
            if (Environment.IsProduction())
            {
                services.Configure<MvcOptions>(options =>
                {
                    options.Filters.Add(new RequireHttpsAttribute());
                });
            }
            
            services.AddAzureAdAuthentication();

            services.AddAuthorization(options =>
            {
                options.AddPolicy("Admin", builder =>
                {
                    builder.RequireClaim(ClaimTypes.Name, Configuration["Authorization:Admins"].Split(','));
                });
            });


            services.AddMvc();

            ConfigureAzureStorage(services);
            
            services.AddTransient<IPostService, AzureStoragePostService>();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<MetaWeblogService>();
            services.AddSingleton<IMetaWeblogProvider, MetaWeblogProvider>();
        }

        private void ConfigureAzureStorage(IServiceCollection services)
        {
            var account = CloudStorageAccount.Parse(Configuration["ConnectionStrings:AzureStorage"]);
            var cloudBlobClient = account.CreateCloudBlobClient();
            cloudBlobClient.GetContainerReference("postcontent").CreateIfNotExistsAsync().Wait();
            cloudBlobClient.GetContainerReference("postmedia").CreateIfNotExistsAsync().Wait();
            account.CreateCloudTableClient().GetTableReference("posts").CreateIfNotExistsAsync().Wait();

            services.AddSingleton(s => account);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseMetaWeblog("/livewriter/" + Configuration["MetaWeblog:ApiAccessKey"]);
            
            app.UseMvc();
        }
    }
}
