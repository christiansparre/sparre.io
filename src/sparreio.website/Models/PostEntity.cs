// Copyright (c) Christian Sparre. All rights reserved. 
// Licensed under the MIT License, see LICENSE.txt in the repository root for license information.

using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace sparreio.website.Models
{
    public class PostEntity : TableEntity
    {
        public PostEntity()
        {

        }

        public PostEntity(int id) : base("FutileBlog", PostId.Get(id))
        {

        }

        public string Title { get; set; }
        public string Tags { get; set; }
        public string Slug { get; set; }
        public DateTime CreatedUtc { get; set; }
        public DateTime? PublishedUtc { get; set; }
        public bool Deleted { get; set; }


        public const string StaticPartitionKey = "FutileBlog";
    }
}