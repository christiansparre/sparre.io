using HashidsNet;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders.Physical;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Sparreio.Website.Features.Blog
{
    public static class BlogPostConfigurationExtensions
    {
        private static Hashids _hashids = new Hashids(minHashLength: 12, alphabet: "abcdefghijklmnopqrstuvwxyz1234567890");

        public static IServiceCollection AddBlogPosts(this IServiceCollection services, IHostEnvironment hostEnvironment)
        {
            try
            {
                var postDirectories = hostEnvironment.ContentRootFileProvider.GetDirectoryContents("Content/Blog/Posts").OfType<PhysicalDirectoryInfo>();

                var posts = postDirectories
                     .Where(a => int.TryParse(a.Name, out var _) && File.Exists(Path.Combine(a.PhysicalPath, "post.html")))
                     .Select(a => new { key = _hashids.Encode(int.Parse(a.Name)), path = Path.Combine(a.PhysicalPath, "post.html") })
                     .ToList();

                services.AddSingleton(s => new BlogPosts(posts.ToDictionary(d => d.key, d => BlogPostParser.Parse(d.path, d.key))));
            }
            catch (Exception ex)
            {
                services.AddSingleton(s => new BlogPosts(new Dictionary<string, ParsedPostModel>
                {
                    ["abc123"] = new ParsedPostModel
                    {
                        Title = "Something went wrong fetching blog posts..."
                    }
                }));
            }

            return services;
        }


    }
}
