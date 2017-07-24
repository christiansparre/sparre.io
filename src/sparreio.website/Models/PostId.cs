// Copyright (c) Christian Sparre. All rights reserved. 
// Licensed under the MIT License, see LICENSE.txt in the repository root for license information.

using System;

namespace sparreio.website.Models
{
    public static class PostId
    {
        public static string Get(int id) => id.ToString("000000000");
        public static int Get(string id) => Convert.ToInt32(id);
    }
}