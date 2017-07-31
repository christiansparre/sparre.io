using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Mvc;
using sparreio.website.Services;

namespace sparreio.website.Controllers
{
    public class FeedController : Controller
    {
        private readonly IPostService _postService;

        public FeedController(IPostService postService)
        {
            _postService = postService;
        }

        [HttpGet("feeds/rss")]
        public async Task<IActionResult> Rss()
        {
            var requestUri = HttpContext.Request.GetUri();

            var url = $"{requestUri.Scheme}://{requestUri.Host}:{requestUri.Port}";

            var postModels = await _postService.GetAllPosts();

            var publishedPosts = postModels.Where(p => p.PublishedUtc.HasValue).OrderByDescending(p => p.PublishedUtc);

            XNamespace atomNs = "http://www.w3.org/2005/Atom";

            var feed = new XDocument(new XElement("rss",
                new XAttribute("version", "2.0"),
                new XAttribute(XNamespace.Xmlns + "atom", atomNs)));

            var root = feed.Root ?? throw new Exception("Should not in a million fucking years happen, look above stupid compiler! :)");

            var channel = new XElement("channel");
            channel.Add(new XElement("title", "sparre.io"));
            channel.Add(new XElement("link", url));
            channel.Add(new XElement("description", "About software development, technology and myself"));
            channel.Add(new XElement("copyright", "2017, Christian Sparre"));
            channel.Add(new XElement(atomNs + "link", new XAttribute("href", $"{url}/feeds/rss"), new XAttribute("rel", "self"), new XAttribute("type", "application/rss+xml")));
            root.Add(channel);

            foreach (var post in publishedPosts)
            {
                var content = await _postService.GetContent(post.Id);

                var item = new XElement("item");
                item.Add(new XElement("title", post.Title));
                var permalink = $"{url}/{post.Id}/{post.Slug}";
                item.Add(new XElement("link", permalink));
                item.Add(new XElement("guid", permalink, new XAttribute("isPermaLink", "true")));
                item.Add(new XElement("description", new XCData(content)));
                item.Add(new XElement("pubDate", post.PublishedUtc.Value.ToString("r")));
                channel.Add(item);
            }

            return Content(feed.ToString(), "application/rss+xml", Encoding.UTF8);
        }
    }
}