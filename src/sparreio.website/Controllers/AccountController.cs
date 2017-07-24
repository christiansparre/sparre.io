// Copyright (c) Christian Sparre. All rights reserved. 
// Licensed under the MIT License, see LICENSE.txt in the repository root for license information.

using System.Linq;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace sparreio.website.Controllers
{
    public class AccountController : Controller
    {
        //
        // GET: /Account/SignIn
        [HttpGet("auth/signin")]
        public IActionResult SignIn()
        {
            return Challenge(
                new AuthenticationProperties { RedirectUri = "/" }, OpenIdConnectDefaults.AuthenticationScheme);
        }

        //
        // GET: /Account/SignOut
        [HttpGet("auth/signout")]
        public IActionResult SignOut()
        {
            var callbackUrl = Url.Action(nameof(SignedOut), "Account", values: null, protocol: Request.Scheme);
            return SignOut(new AuthenticationProperties { RedirectUri = callbackUrl },
                CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme);
        }

        [Authorize]
        [HttpGet("auth/claims")]
        public IActionResult Claims()
        {
            var claims = User.Claims.Select(s => new
            {
                s.Type,
                s.Subject.Name,
                s.Value
            });

            return Json(claims);
        }
        
        //
        // GET: /Account/SignedOut
        [HttpGet("auth/signedout")]
        public IActionResult SignedOut()
        {
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                // Redirect to home page if the user is authenticated.
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }

            return View();
        }
    }
}