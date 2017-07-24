// Copyright (c) Christian Sparre. All rights reserved. 
// Licensed under the MIT License, see LICENSE.txt in the repository root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sparreio.website.Services;
using sparreio.website.ViewModels.Admin.Posts;

namespace sparreio.website.Controllers
{
    [Authorize(Policy = "Admin")]
    public class AdminController : Controller
    {
        private readonly IPostService _postService;

        public AdminController(IPostService postService)
        {
            _postService = postService;
        }

        [HttpGet("admin/posts/create")]
        public IActionResult CreatePost()
        {
            return View("Posts/Create");
        }

        [HttpPost("admin/posts/create")]
        public async Task<IActionResult> CreatePost(CreatePostViewModel model)
        {
            var postId = await _postService.GetNextPostId();
            var postModel = await _postService.SavePost(postId, model.Title, model.Categories.Split(',', StringSplitOptions.RemoveEmptyEntries), model.PublishedUtc);
            await _postService.SavePostContent(postId, model.Content);

            return RedirectToAction("Post", "Home", new { id = postId });
        }
    }
}