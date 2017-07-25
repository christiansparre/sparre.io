// Copyright (c) Christian Sparre. All rights reserved. 
// Licensed under the MIT License, see LICENSE.txt in the repository root for license information.

using System;

namespace sparreio.website.ViewModels.Admin.Posts
{
    public class CreatePostViewModel
    {
        public string Title { get; set; }
        public DateTime PublishedUtc { get; set; }
        public string Tags { get; set; }
        public string Content { get; set; }
    }
}