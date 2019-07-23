using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Sparreio.Website.Features.Blog
{
    public static class BlogPostParser
    {
        public static ParsedPostModel Parse(string path, string id)
        {
            var post = new ParsedPostModel();
            var doc = XDocument.Load(path);

            if (TryGetMetaPropertyValue(doc, "article:published_time", out var pubStr))
            {
                post.Published = DateTime.Parse(pubStr, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
            }
            if (TryGetMetaPropertyValue(doc, "article:modified_time", out var modStr))
            {
                post.Modified = DateTime.Parse(pubStr, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
            }

            post.Title = doc.XPathSelectElement("html/head/title").Value;
            post.Tags = doc.XPathSelectElements("html/head/meta").Where(e => e.Attribute("property").Value == "article:tag").Select(s => s.Attribute("content").Value).ToArray();
            post.Body = doc.XPathSelectElement("html/body").ToString();

            post.Id = id;

            post.Slug = GenerateSlug(post.Title);

            return post;
        }

        private static bool TryGetMetaPropertyValue(XDocument doc, string key, out string value)
        {
            var element = doc.XPathSelectElements("html/head/meta").Where(e => e.Attribute("property").Value == key).FirstOrDefault();

            value = element?.Attribute("content")?.Value;

            return value != null;
        }


        public static string GenerateSlug(string str)
        {
            str = str.Normalize(NormalizationForm.FormD);

            var stringBuilder = new StringBuilder();

            foreach (var c in str)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            str = stringBuilder.ToString().Normalize(NormalizationForm.FormC);

            str = Regex.Replace(str, @"[^a-z0-9\s-]", "");

            str = Regex.Replace(str, @"\s+", " ").Trim();

            str = str.Substring(0, str.Length <= 45 ? str.Length : 45).Trim();
            str = Regex.Replace(str, @"\s", "-");
            return str;
        }
    }
}
