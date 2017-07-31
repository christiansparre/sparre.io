// Copyright (c) Christian Sparre. All rights reserved. 
// Licensed under the MIT License, see LICENSE.txt in the repository root for license information.

using System;

namespace sparreio.website.Models
{
    public class PostModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string[] Tags { get; set; }
        public string Slug { get; set; }
        public DateTime CreatedUtc { get; set; }
        public DateTime? PublishedUtc { get; set; }
    }
}