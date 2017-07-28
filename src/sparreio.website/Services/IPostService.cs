// Copyright (c) Christian Sparre. All rights reserved. 
// Licensed under the MIT License, see LICENSE.txt in the repository root for license information.

using System;
using System.Threading.Tasks;
using Markdig.Extensions.TaskLists;
using sparreio.website.Models;

namespace sparreio.website.Services
{
    public interface IPostService
    {
        Task<PostModel> GetPost(int id);
        Task<PostModel[]> GetAllPosts(bool includeDelete = false);
        Task<int> GetNextPostId();

        Task<PostModel> CreatePost(int id, string title, DateTime createdUtc, string[] tags);
        Task<PostModel> UpdatePost(int id, string title, string[] tags);

        Task PublishPost(int id);
        Task UnpublishPost(int id);

        Task SaveContent(int id, string content);
        Task<string> GetContent(int id);
        //Task SaveMediaObject(string path, )
        Task DeletePost(int id);

        Task SaveMedia(string path, string type, byte[] data);
        Task<(byte[] data, string type)> GetMedia(string path);
    }
}