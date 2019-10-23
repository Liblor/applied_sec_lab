using System;
using Microsoft.AspNetCore.Mvc;

using CertServer.Models;
using CertServer.DataModifiers;

namespace CertServer.Controllers
{
	[ApiController, Route("api")]

    public class DownloadPrivateKeyController : ControllerBase
    {
		private readonly PrivateKeysDBModifier _privKeysModifier;
		private readonly UserDBAuthenticator _userDBAuthenticator;

		public DownloadPrivateKeyController(
			PrivateKeysDBModifier privKeysModifier,
			UserDBAuthenticator userDBAuthenticator
		)
		{
			_privKeysModifier = privKeysModifier;
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
		[HttpPost("[controller]")]
		public IActionResult DownloadPrivateKey(DownloadPrivateKeyRequest privKeyRequest)
		{
			User user = _userDBAuthenticator.AuthenticateAndGetUser(privKeyRequest.Uid, privKeyRequest.Password);

			if (user != null)
			{
				PrivateKey privKey = _privKeysModifier.GetPrivateKey(user);

				if (privKey == null)
				{
					return BadRequest();
				}
				else
				{
					return Ok(privKey);
				}
			}
			else {
				return Unauthorized();
			}
		}
    }
}