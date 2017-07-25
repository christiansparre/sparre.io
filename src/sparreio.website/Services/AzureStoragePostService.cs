// Copyright (c) Christian Sparre. All rights reserved. 
// Licensed under the MIT License, see LICENSE.txt in the repository root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using sparreio.website.Models;

namespace sparreio.website.Services
{
    public class AzureStoragePostService : IPostService
    {
        private CloudBlobContainer _contentContainer;
        private CloudTable _table;

        public AzureStoragePostService(CloudStorageAccount storageAccount)
        {
            _contentContainer = storageAccount.CreateCloudBlobClient().GetContainerReference("postcontent");
            _table = storageAccount.CreateCloudTableClient().GetTableReference("posts");
        }

        public async Task<PostModel> GetPost(int id)
        {
            var entityResult = await _table.ExecuteAsync(TableOperation.Retrieve<PostEntity>(PostEntity.StaticPartitionKey, PostId.Get(id)));

            if (entityResult.Result == null)
            {
                return null;
            }

            var postEntity = (PostEntity)entityResult.Result;
            return FromEntity(postEntity);
        }

        private PostModel FromEntity(PostEntity entity)
        {
            return new PostModel
            {
                Id = PostId.Get(entity.RowKey),
                Title = entity.Title,
                PublishedUtc = entity.PublishedUtc,
                Tags = entity?.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
            };
        }

        public async Task<PostModel[]> GetAllPostsAsync()
        {
            var query = new TableQuery<PostEntity>();
            var posts = new List<PostEntity>();

            TableContinuationToken continuationToken = null;
            do
            {
                var postsSegment = await _table.ExecuteQuerySegmentedAsync(query, continuationToken);
                posts.AddRange(postsSegment.Results);
                continuationToken = postsSegment.ContinuationToken;
            } while (continuationToken != null);

            return posts.Select(FromEntity).ToArray<PostModel>();
        }

        public async Task<int> GetNextPostId()
        {
            var query = new TableQuery<PostEntity>().Select(new List<string>());
            var posts = new List<PostEntity>();

            TableContinuationToken continuationToken = null;
            do
            {
                var postsSegment = await _table.ExecuteQuerySegmentedAsync(query, continuationToken);
                posts.AddRange(postsSegment.Results);
                continuationToken = postsSegment.ContinuationToken;
            } while (continuationToken != null);

            return posts.Any() ? posts.Max(s => PostId.Get(s.RowKey)) + 1 : 1;
        }

        public async Task<PostModel> SavePost(int id, string title, string[] tags, DateTime publishedUtc)
        {
            var entity = new PostEntity(id)
            {
                Title = title,
                Tags = string.Join(',', tags),
                PublishedUtc = publishedUtc == DateTime.MinValue ? DateTime.UtcNow : publishedUtc
            };

            await _table.ExecuteAsync(TableOperation.Insert(entity));
            return FromEntity(entity);
        }

        public async Task SavePostContent(int id, string content)
        {
            var blobRef = _contentContainer.GetBlockBlobReference($"{PostId.Get(id)}.md");
            blobRef.Properties.ContentType = "text/markdown";
            await blobRef.UploadTextAsync(content, Encoding.UTF8, null, null, null);
            await blobRef.SetPropertiesAsync();
        }

        public async Task<string> GetPostContent(int id)
        {
            var blobRef = _contentContainer.GetBlockBlobReference($"{PostId.Get(id)}.md");

            if (!await blobRef.ExistsAsync())
            {
                return null;
            }

            var postContent = await blobRef.DownloadTextAsync(Encoding.UTF8, null, null, null);
            return postContent;
        }

    }
}