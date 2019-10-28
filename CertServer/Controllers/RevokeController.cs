using System;
using Microsoft.AspNetCore.Mvc;
using CertServer.DataModifiers;
using CertServer.Models;

namespace CertServer.Controllers
{
	[ApiController, Route("api")]
	public class RevokeController : ControllerBase
	{
		private readonly CADBModifier _caDBModifier;
		private readonly UserDBAuthenticator _userDBAuthenticator;

		public RevokeController(
			CADBModifier caDBModifier,
			UserDBAuthenticator userDBAuthenticator
		)
		{
			_caDBModifier = caDBModifier;
			_userDBAuthenticator = userDBAuthenticator;
		}

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
		/// <response code="400">Bad request</response>
		[Produces("application/json")]
		[ProducesResponseType(200)]
		[ProducesResponseType(400)]
		[HttpPost("[controller]")]
		public IActionResult RevokeCertificate(RevokeRequest revokeRequest)
		{
			User user = _userDBAuthenticator.GetUser(revokeRequest.Uid);

			if (user != null)
			{
				_caDBModifier.RevokeAllCertificatesOfUser(user);
				return Ok();
			}
			else
			{
				return BadRequest();
			}
		}
	}
}
