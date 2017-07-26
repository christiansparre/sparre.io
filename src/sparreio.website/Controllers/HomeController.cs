// Copyright (c) Christian Sparre. All rights reserved. 
// Licensed under the MIT License, see LICENSE.txt in the repository root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using sparreio.website.Services;
using sparreio.website.ViewModels.Home;

namespace sparreio.website.Controllers
{
    public class HomeController : Controller
    {
        private readonly IPostService _service;

        public HomeController(IPostService postService)
        {
            _service = postService;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var posts = await _service.GetAllPostsAsync();

            var postViewModels = new List<IndexViewModel.Post>();

            foreach (var p in posts)
            {
                var c = await _service.GetPostContent(p.Id);

                if (c != null)
                {
                    var excerpt = c.Substring(0, c.Length > 400 ? 400 : c.Length);

                    postViewModels.Add(new IndexViewModel.Post
                    {
                        Id = p.Id,
                        Title = p.Title,
                        PublishedUtc = p.PublishedUtc,
                        Tags = p.Tags,
                        Exerpt = excerpt + "...",
                    });
                }
            }

            var indexViewModel = new IndexViewModel { Posts = postViewModels.OrderByDescending(o=>o.PublishedUtc) };
            return View(indexViewModel);
        }

        [HttpGet("{id:int}/{*slug}")]
        public async Task<IActionResult> Post(int id, string slug = null)
        {
            var post = await _service.GetPost(id);

            var postContent = await _service.GetPostContent(id);

            if (post == null || postContent == null)
            {
                return NotFound();
            }

            return View(new PostViewModel { Id = id, Title = post.Title, Content = postContent, PublishedUtc = post.PublishedUtc, Tags = post.Tags });
        }

        [HttpGet("tags/{tag}")]
        public async Task<IActionResult> PostsByTag(string tag)
        {
            var posts = await _service.GetAllPostsAsync();

            var postModels = posts.Where(t => t.Tags.Contains(tag, StringComparer.InvariantCultureIgnoreCase));

            var postViewModels = new List<IndexViewModel.Post>();

            foreach (var p in postModels)
            {
                var c = await _service.GetPostContent(p.Id);

                if (c != null)
                {
                    var excerpt = c.Substring(0, c.Length > 400 ? 400 : c.Length);

                    postViewModels.Add(new IndexViewModel.Post
                    {
                        Id = p.Id,
                        Title = p.Title,
                        PublishedUtc = p.PublishedUtc,
                        Tags = p.Tags,
                        Exerpt = excerpt + "...",
                    });
                }
            }

            ViewBag.Tag = tag;
            
            var indexViewModel = new IndexViewModel { Posts = postViewModels.OrderByDescending(o => o.PublishedUtc) };
            return View(indexViewModel);
        }

        [HttpGet("about")]
        public IActionResult About()
        {
            return View();
        }

        [HttpGet("contact")]
        public IActionResult Contact()
        {
            return View();
        }
    }
}