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
        private CloudBlobContainer _mediaContainer;

        public AzureStoragePostService(CloudStorageAccount storageAccount)
        {
            var cloudBlobClient = storageAccount.CreateCloudBlobClient();
            _contentContainer = cloudBlobClient.GetContainerReference("postcontent");
            _mediaContainer = cloudBlobClient.GetContainerReference("postmedia");
            _table = storageAccount.CreateCloudTableClient().GetTableReference("posts");
        }

        public async Task<PostModel> GetPost(int id)
        {
            var postEntity = await GetPostEntity(id);
            return postEntity == null ? null : FromEntity(postEntity);
        }

        private async Task<PostEntity> GetPostEntity(int id)
        {
            var entityResult = await _table.ExecuteAsync(TableOperation.Retrieve<PostEntity>(PostEntity.StaticPartitionKey, PostId.Get(id)));

            if (entityResult.Result == null)
            {
                return null;
            }

            var postEntity = (PostEntity)entityResult.Result;
            return postEntity;
        }

        private PostModel FromEntity(PostEntity entity)
        {
            return new PostModel
            {
                Id = PostId.Get(entity.RowKey),
                Title = entity.Title,
                PublishedUtc = entity.PublishedUtc,
                CreatedUtc = entity.CreatedUtc,
                Tags = entity?.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
            };
        }

        public async Task<PostModel[]> GetAllPosts(bool includeDeleted = false)
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

            if (!includeDeleted)
            {
                posts = posts.Where(p => p.Deleted == false).ToList();
            }

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

        public async Task<PostModel> CreatePost(int id, string title, DateTime createdUtc, string[] tags)
        {
            var existingPost = await GetPost(id);

            if (existingPost != null)
            {
                throw new Exception("A post with that id already exists");
            }

            var entity = new PostEntity(id)
            {
                Title = title,
                Tags = string.Join(',', tags),
                CreatedUtc = createdUtc == DateTime.MinValue ? DateTime.UtcNow : createdUtc
            };

            await _table.ExecuteAsync(TableOperation.Insert(entity));
            return FromEntity(entity);
        }

        public async Task<PostModel> UpdatePost(int id, string title, string[] tags)
        {
            var existingPost = await GetPostEntity(id);

            if (existingPost == null)
            {
                throw new Exception("A post with that id does not exist");
            }

            existingPost.Title = title;
            existingPost.Tags = tags == null ? "" : string.Join(',', tags);

            await _table.ExecuteAsync(TableOperation.Replace(existingPost));
            return FromEntity(existingPost);
        }

        public async Task PublishPost(int id)
        {
            var existingPost = await GetPostEntity(id);

            if (existingPost == null)
            {
                throw new Exception("A post with that id does not exist");
            }

            if (existingPost.PublishedUtc.HasValue)
            {
                return;
            }

            existingPost.PublishedUtc = DateTime.UtcNow;

            await _table.ExecuteAsync(TableOperation.Replace(existingPost));
        }

        public async Task DeletePost(int id)
        {
            var existingPost = await GetPostEntity(id);

            if (existingPost == null)
            {
                throw new Exception("A post with that id does not exist");
            }

            existingPost.Deleted = true;

            await _table.ExecuteAsync(TableOperation.Replace(existingPost));
        }

public async Task SaveMedia(string path, string type, byte[] data)
{
    var blobRef = _mediaContainer.GetBlockBlobReference(path);
    blobRef.Properties.ContentType = type;
    await blobRef.UploadFromByteArrayAsync(data, 0, data.Length, null, null, null);
    await blobRef.SetPropertiesAsync();
}

        public async Task<(byte[] data, string type)> GetMedia(string path)
        {
            var blobRef = _mediaContainer.GetBlockBlobReference(path);

            if (!await blobRef.ExistsAsync())
            {
                return (null, null);
            }

            var data = new byte[blobRef.Properties.Length];
            var read = await blobRef.DownloadToByteArrayAsync(data, 0, null, null, null);
            return (data: data, type: blobRef.Properties.ContentType);
        }

        public async Task UnpublishPost(int id)
        {
            var existingPost = await GetPostEntity(id);

            if (existingPost == null)
            {
                throw new Exception("A post with that id does not exist");
            }

            if (!existingPost.PublishedUtc.HasValue)
            {
                return;
            }

            existingPost.PublishedUtc = null;

            await _table.ExecuteAsync(TableOperation.Replace(existingPost));
        }

        public async Task SaveContent(int id, string content)
        {
            var blobRef = _contentContainer.GetBlockBlobReference($"{PostId.Get(id)}.html");
            blobRef.Properties.ContentType = "text/html";
            await blobRef.UploadTextAsync(content, Encoding.UTF8, null, null, null);
            await blobRef.SetPropertiesAsync();
        }

        public async Task<string> GetContent(int id)
        {
            var blobRef = _contentContainer.GetBlockBlobReference($"{PostId.Get(id)}.html");

            if (!await blobRef.ExistsAsync())
            {
                return null;
            }

            var postContent = await blobRef.DownloadTextAsync(Encoding.UTF8, null, null, null);
            return postContent;
        }
    }
}