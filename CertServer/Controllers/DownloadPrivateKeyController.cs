using System;
using Microsoft.AspNetCore.Mvc;

using CertServer.Models;
using CertServer.DataModifiers;
using CoreCA.DataModel;

namespace CertServer.Controllers
{
	[ApiController, Route("api")]

    public class DownloadPrivateKeyController : ControllerBase
    {
		private readonly CADBModifier _caDBModifier;
		private readonly UserDBAuthenticator _userDBAuthenticator;

		public DownloadPrivateKeyController(
			CADBModifier caDBModifier,
			UserDBAuthenticator userDBAuthenticator
		)
		{
			_caDBModifier = caDBModifier;
			_userDBAuthenticator = userDBAuthenticator;
		}

		/// <summary>
		/// Download certificate with encrypted private key in pkcs12 mode
		/// </summary>
		/// <remarks>
		/// Sample request:
		///
		///     POST /api/downloadprivatekey
		///		{
		///			"uid": "ab",
		///			"password": "plain"
		///		}
		///
		/// </remarks>
		/// <returns>Certificate with encrypted private key in pkcs12</returns>
		[Produces("application/json")]
		[ProducesResponseType(200)]
		[ProducesResponseType(401)]
        [HttpPost("[controller]")]
		public IActionResult DownloadPrivateKey(DownloadPrivateKeyRequest privKeyRequest)
		{
			User user = _userDBAuthenticator.AuthenticateAndGetUser(privKeyRequest.Uid, privKeyRequest.Password);

			if (user != null)
			{
				PrivateKey privKey = _caDBModifier.GetPrivateKey(user);

				if (privKey == null)
				{
					return BadRequest();
				}
				else
				{
					return Ok(
						new UserCertificate {
							Pkcs12Archive = privKey.KeyPkcs12
						}
					);
				}
			}
			else {
				return Unauthorized();
			}
		}
    }
}