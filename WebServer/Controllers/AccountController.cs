using Microsoft.AspNetCore.Mvc;
using System;
using WebServer.Models;

namespace WebServer.Controllers
{
	public class AccountController : Controller
	{
		[HttpGet]
		public IActionResult Index()
		{
			return View();
		}

		[HttpPost]
		public IActionResult Update(User user)
		{
			throw new NotImplementedException();
		}
	}
}
