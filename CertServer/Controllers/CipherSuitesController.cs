using Microsoft.AspNetCore.Mvc;
using System;

using CertServer.Models;

namespace CertServer.Controllers
{
	[ApiController, Route("api")]

	public class CipherSuitesController : ControllerBase
	{
		/// <summary>
		/// Get supported cipher suites.
		/// </summary>
		/// <remarks>
		/// Sample request:
		///
		///     GET /api/ciphersuites
		///
		/// </remarks>
		/// <returns>Supported cipher suites</returns>
		[Produces("application/json")]
		[ProducesResponseType(200)]
		[HttpGet("[controller]")]
		public IActionResult GetCipherSuites()
		{
			return Ok(CAConfig.CipherSuites);
		}
	}
}
