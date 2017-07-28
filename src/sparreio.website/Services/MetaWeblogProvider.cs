using System;
using System.Linq;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using sparreio.website.Models;
using WilderMinds.MetaWeblog;

namespace sparreio.website.Services
{
    public class MetaWeblogProvider : IMetaWeblogProvider
    {
        private readonly IPostService _postService;
        private readonly IHostingEnvironment _environment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;

        public MetaWeblogProvider(IPostService postService, IHostingEnvironment environment, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _postService = postService;
            _environment = environment;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
        }

        private void EnsureValidCredentials(string username, string password)
        {
            if (username != _configuration["MetaWeblog:Username"] || password != _configuration["MetaWeblog:Password"])
            {
                throw new MetaWeblogException("Forbidden: Wrong username or password", 401);
            }
        }

        public UserInfo GetUserInfo(string key, string username, string password)
        {
            EnsureValidCredentials(username, password);

            throw new System.NotImplementedException();
        }

        public BlogInfo[] GetUsersBlogs(string key, string username, string password)
        {
            EnsureValidCredentials(username, password);

            var url = GetBlogRootUrl();

            return new BlogInfo[]
            {
                new BlogInfo
                {
                    blogName = "sparre.io - " + _environment.EnvironmentName,
                    blogid = "1",
                    url = url
                },
            };
        }

        private string GetBlogRootUrl()
        {
            var requestUri = _httpContextAccessor.HttpContext.Request.GetUri();

            var url = $"{requestUri.Scheme}://{requestUri.Host}:{requestUri.Port}";
            return url;
        }

        public Post GetPost(string postid, string username, string password)
        {
            EnsureValidCredentials(username, password);

            var id = Convert.ToInt32(postid);
            var postModel = _postService.GetPost(id).Result;
            var post = BuildPost(postModel);
            return post;
        }

        private Post BuildPost(PostModel postModel)
        {
            var content = _postService.GetContent(postModel.Id).Result;

            return new Post
            {
                userid = "1",
                title = postModel.Title,
                categories = postModel.Tags,
                dateCreated = postModel.CreatedUtc,
                description = content,
                permalink = "/" + postModel.Id,
                postid = postModel.Id
            };
        }

        public Post[] GetRecentPosts(string blogid, string username, string password, int numberOfPosts)
        {
            EnsureValidCredentials(username, password);

            var postModels = _postService.GetAllPosts().Result;
            var posts = postModels.Select(s => BuildPost(s)).ToArray();
            return posts;
        }

        public string AddPost(string blogid, string username, string password, Post post, bool publish)
        {
            EnsureValidCredentials(username, password);

            var postId = _postService.GetNextPostId().Result;

            var postModel = _postService.CreatePost(postId, post.title, post.dateCreated, post.categories).Result;
            _postService.SaveContent(postId, post.description).Wait();

            if (publish)
            {
                _postService.PublishPost(postId).Wait();
            }

            return postId.ToString();
        }

        public bool DeletePost(string key, string postid, string username, string password, bool publish)
        {
            EnsureValidCredentials(username, password);

            _postService.DeletePost(Convert.ToInt32(postid)).Wait();
            return true;
        }

        public bool EditPost(string postid, string username, string password, Post post, bool publish)
        {
            EnsureValidCredentials(username, password);

            var id = Convert.ToInt32(postid);
            var postModel = _postService.UpdatePost(id, post.title, post.categories).Result;
            _postService.SaveContent(id, post.description).Wait();

            if (publish)
            {
                _postService.PublishPost(id).Wait();
            }
            else
            {
                _postService.UnpublishPost(id).Wait();
            }

            return true;
        }

        public CategoryInfo[] GetCategories(string blogid, string username, string password)
        {
            EnsureValidCredentials(username, password);

            var tags = _postService.GetAllPosts().Result.SelectMany(s => s.Tags).Distinct();
            return tags.Select(s => new CategoryInfo { categoryid = s, title = s, htmlUrl = "/tags/" + s }).ToArray();
        }

        public int AddCategory(string key, string username, string password, NewCategory category)
        {
            EnsureValidCredentials(username, password);

            return 0;
        }

        public MediaObjectInfo NewMediaObject(string blogid, string username, string password, MediaObject mediaObject)
        {
            EnsureValidCredentials(username, password);

            _postService.SaveMedia(mediaObject.name, mediaObject.type, Convert.FromBase64String(mediaObject.bits));

            var mediaObjectInfo = new MediaObjectInfo
            {
                url = $"/post-media/{mediaObject.name}"
            };
            return mediaObjectInfo;
        }
    }
}