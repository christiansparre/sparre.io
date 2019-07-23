using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sparreio.Website.Features.Blog
{
    public class ParsedPostModel
    {
        public string Slug { get; set; }
        public string Id { get; set; }
        public string SlugAndId => $"{Slug}-{Id}";

        public DateTime Published { get; set; }
        public DateTime Modified { get; set; }
        public string[] Tags { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }

    }
}
