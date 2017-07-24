// Copyright (c) Christian Sparre. All rights reserved. 
// Licensed under the MIT License, see LICENSE.txt in the repository root for license information.

using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.Storage;
using sparreio.website.Services;

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

            var account = CloudStorageAccount.Parse(Configuration["ConnectionStrings:AzureStorage"]);
            account.CreateCloudBlobClient().GetContainerReference("postcontent").CreateIfNotExistsAsync().Wait();
            account.CreateCloudTableClient().GetTableReference("posts").CreateIfNotExistsAsync().Wait();

            services.AddSingleton(s => account);
            services.AddTransient<IPostService, AzureStoragePostService>();
        }
        
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseMvc();
        }
    }
}
