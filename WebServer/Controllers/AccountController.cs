﻿using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using WebServer.Models;
using WebServer.Models.Account;

namespace WebServer.Controllers
{
    public class AccountController : Controller
    {
        private readonly IMoviesUserContext _dbContext;

        public AccountController(IMoviesUserContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet, Authorize]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet, AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost, AllowAnonymous]
        public async Task<IActionResult> Login(LoginDetails loginDetails, string returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                string passwordHash;
                using (var sha1 = SHA1.Create())
                {
                    byte[] hashBytes = sha1.ComputeHash(Encoding.UTF8.GetBytes(loginDetails.Password));
                    var sb = new StringBuilder();
                    foreach (byte b in hashBytes)
                        sb.AppendFormat("{0:x2}", b);

                    passwordHash = sb.ToString();
                }

                var user = _dbContext.Users.SingleOrDefault(u =>
                    u.Email.Equals(loginDetails.Email, StringComparison.OrdinalIgnoreCase) &&
                    u.PasswordHash == passwordHash);

                if (user == null)
                {
                    ViewData["ErrorMessage"] = "Email address or password incorrect.";
                    return View(loginDetails);
                }

                var authProps = new AuthenticationProperties
                {
                    AllowRefresh = true,
                    // TODO: reevaluate arbitrary expiration
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(1),
                    IsPersistent = true,
                    IssuedUtc = DateTimeOffset.UtcNow,
                    RedirectUri = returnUrl
                };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(user.ToClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme)),
                    authProps);

                // TODO: assess potential open redirect vulnerability
                if (!string.IsNullOrWhiteSpace(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction(nameof(Index));
            }

            return View(loginDetails);
        }

        [HttpGet, Authorize]
        public IActionResult Update()
        {
            return View();
        }

        [HttpPost, Authorize]
        public IActionResult Update(UserDetails userDetails)
        {
            if (ModelState.IsValid)
            {

                bool success = false;

                var user = _dbContext.Users.Find(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                if (user != null)
                {
                    user.Email = userDetails.Email;
                    user.FirstName = userDetails.FirstName;
                    user.LastName = userDetails.LastName;

                    _dbContext.Users.Update(user);
                    // TODO Revoke all certificates not matching the new information.

                    _dbContext.SaveChanges();
                    success = true;
                }

                if (success)
                {
                    ViewData["SuccessMessage"] = "Account information updated successfully.";
                }
                else
                {
                    ViewData["ErrorMessage"] = "Updating account information failed.";
                }

                return RedirectToAction(nameof(Index));
            }
            else
            {
                return View();
            }
        }

        [HttpPost, Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }
    }
}
