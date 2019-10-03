using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using WebServer.Models;
using WebServer.Models.Account;

namespace WebServer.Controllers
{
    public class AccountController : Controller
    {
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
                // TODO: validate credentials against DB
                // This should be constructed using info from the DB
                var user = new User
                {
                    Email = loginDetails.Email,
                    FirstName = "Test",
                    LastName = "User",
                    Id = "testuser"
                };

                var authProps = new AuthenticationProperties
                {
                    // TODO: configure
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
        public IActionResult Update(User user)
        {
            return View();
        }

        [HttpPost, Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction(nameof(Login));
        }
    }
}
