using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage;
using System;

using CertServer.DataModifiers;
using CertServer.Models;

namespace CertServer.Controllers
{
	[ApiController, Route("api")]
    public class ChangePasswordController : ControllerBase
    {
        private readonly UserDBAuthenticator _userDBAuthenticator;

		public ChangePasswordController(UserDBAuthenticator userDBAuthenticator)
		{
			_userDBAuthenticator = userDBAuthenticator;
		}

		/// <summary>
		/// Change a user password by providing the old and the new password.
		/// </summary>
		/// <remarks>
		/// Sample request:
		///
		///     POST /api/changepassword
		///     {
		///        	"uid": "ab",
		///			"oldPassword": "OldPassword",
		///			"newPassword": "NewPassword"
		///     }
		///
		/// </remarks>
		/// <param name="passwordChangeRequest"></param>
		/// <response code="200">Password successfully changed</response>
		/// <response code="400">Bad request</response>
		/// <response code="401">Invalid password</response>
		[Produces("application/json")]
		[ProducesResponseType(200)]
		[ProducesResponseType(400)]
		[ProducesResponseType(401)]
		[HttpPost("[controller]")]
		public IActionResult ChangePassword(PasswordChangeRequest passwordChangeRequest)
		{
			IActionResult response;

			using (
				IDbContextTransaction scope = _userDBAuthenticator.GetScope()
			)
			{
				User user = _userDBAuthenticator.AuthenticateAndGetUser(
					passwordChangeRequest.Uid, 
					passwordChangeRequest.OldPassword
				);

				if (user != null)
				{
					if (_userDBAuthenticator.ChangePassword(user, passwordChangeRequest.NewPassword))
					{
						response = Ok();
					}
					else
					{
						response = BadRequest();
					}
				}
				else {
					response = Unauthorized();
				}

				scope.Commit();
			}

			return response;
		}
    }
}