using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;

using CertServer.Models;

namespace CertServer.Controllers
{
	[ApiController, Route("api")]

	public class CipherSuitesController : ControllerBase
	{
		private readonly ILogger _logger;

		public CipherSuitesController(ILogger<CipherSuitesController> logger)
		{
			_logger = logger;
		}

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
			_logger.LogInformation("Cipher suites were requested");
			return Ok(CAConfig.CipherSuites);
		}
	}
}
