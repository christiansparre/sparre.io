// Copyright (c) Christian Sparre. All rights reserved. 
// Licensed under the MIT License, see LICENSE.txt in the repository root for license information.

using System;
using System.Threading.Tasks;
using sparreio.website.Models;

namespace sparreio.website.Services
{
    public interface IPostService
    {
        Task<PostModel> GetPost(int id);
        Task<PostModel[]> GetAllPostsAsync();
        Task<int> GetNextPostId();
        Task<PostModel> SavePost(int id, string title, string[] tags, DateTime publishedFrom);
        Task SavePostContent(int id, string content);
        Task<string> GetPostContent(int id);
    }
}