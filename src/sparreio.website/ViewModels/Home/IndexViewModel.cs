// Copyright (c) Christian Sparre. All rights reserved. 
// Licensed under the MIT License, see LICENSE.txt in the repository root for license information.

using System;
using System.Collections.Generic;

namespace sparreio.website.ViewModels.Home
{
    public class IndexViewModel
    {
        public IEnumerable<Post> Posts { get; set; }

        public class Post
        {
            public string Title { get; set; }
            public DateTime PublishedUtc { get; set; }
            public string Exerpt { get; set; }
            public string[] Categories { get; set; }
            public int Id { get; set; }
        }
    }
}