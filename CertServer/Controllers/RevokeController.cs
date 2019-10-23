using System;
using Microsoft.AspNetCore.Mvc;
using CertServer.Authentication;
using CertServer.Models;

namespace CertServer.Controllers
{
	[ApiController, Route("api")]
	public class RevokeController : ControllerBase
	{		
		/// <summary>
		/// Revoke a certificate.
		/// </summary>
		/// <remarks>
		/// Sample request:
		///
		///     POST /api/issue
		///     {
		///        	"uid": "ab",
		///			"password": "plain",
		///			"SerialNumber": 1234
		///     }
		///
		/// </remarks>
		/// <param name="revokeRequest"></param>
		/// <response code="200">Certificate revoked</response>
		/// <response code="401">Unauthorized request</response>
		[Produces("application/json")]
		[ProducesResponseType(200)]
		[ProducesResponseType(401)]
		[HttpPost("[controller]")]
		public IActionResult RevokeCertificate(RevokeRequest revokeRequest)
		{
			User user = UserDBAuthenticator.GetUser(revokeRequest.Uid, revokeRequest.Password);

			if (user != null)
			{
				// XXX: Implement revocation
				return Ok();
			}
			else {
				return Unauthorized();
			}
		}
	}
}
