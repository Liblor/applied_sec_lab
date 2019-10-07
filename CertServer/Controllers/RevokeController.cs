using System;
using Microsoft.AspNetCore.Mvc;
using CertServer.Models;

namespace CertServer.Controllers
{
	[ApiController, Route("api")]
    public class RevokeController : ControllerBase
    {
		[HttpPost("[controller]")]
		public IActionResult RevokeCertificate()
		{
            return Ok("hi");
		}
    }
}