using HashidsNet;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders.Physical;
using Microsoft.Extensions.Hosting;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Sparreio.Website.Features.Blog.Controllers
{
    public class BlogController : Controller
    {
        private readonly IHostEnvironment _environment;
        private readonly BlogPosts _blogPosts;
        private Hashids _hashids = new Hashids(minHashLength: 12, alphabet: "abcdefghijklmnopqrstuvwxyz1234567890");

        public BlogController(IHostEnvironment environment, BlogPosts blogPosts)
        {
            _environment = environment;
            _blogPosts = blogPosts;
        }

        [HttpGet("blog/posts")]
        public IActionResult Posts()
        {
            return View("Views/Blog/Posts/Posts.cshtml", _blogPosts.Posts.Values.OrderByDescending(o => o.Published).Take(10).ToList());
        }

        [HttpGet("blog/tag/{tag}")]
        public IActionResult PostsByTag(string tag)
        {
            ViewBag.Tag = tag;

            return View("Views/Blog/Posts/PostsByTag.cshtml", _blogPosts.Posts.Values.Where(s => s.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase)).OrderByDescending(o => o.Published).Take(10).ToList());
        }

        [HttpGet("blog/post/{*path}")]
        public IActionResult Post(string path)
        {
            var parts = path.Split('-', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 0)
            {
                return NotFound();
            }
            var id = parts.Last();

            if (_blogPosts.Posts.TryGetValue(id, out var post))
            {
                return View("Views/Blog/Posts/Post.cshtml", post);
            }
            else
            {
                return NotFound();
            }
        }
    }
}
