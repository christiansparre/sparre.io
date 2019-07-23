using System.Collections.Generic;

namespace Sparreio.Website.Features.Blog
{
    public class BlogPosts
    {
        public BlogPosts(Dictionary<string, ParsedPostModel> posts)
        {
            Posts = posts;
        }

        public IReadOnlyDictionary<string, ParsedPostModel> Posts { get; }
    }
}
