using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using System;

using CertServer.DataModifiers;
using CertServer.Models;
using CoreCA.DataModel;

namespace CertServer.Controllers
{
    [ApiController, Route("api")]
    public class ChangePasswordController : ControllerBase
    {
        private readonly UserDBAuthenticator _userDBAuthenticator;
        private readonly ILogger _logger;

        public ChangePasswordController(
            UserDBAuthenticator userDBAuthenticator,
            ILogger<ChangePasswordController> logger
        )
        {
            _userDBAuthenticator = userDBAuthenticator;
            _logger = logger;
        }

        /// <summary>
        /// Change a user password by providing the old and the new password.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/changepassword
        ///     {
        ///            "uid": "ab",
        ///            "oldPassword": "OldPassword",
        ///            "newPassword": "NewPassword"
        ///     }
        ///
        /// </remarks>
        /// <param name="passwordChangeRequest"></param>
        /// <response code="204">Password successfully changed</response>
        /// <response code="400">Password does not meet password policy</response>
        /// <response code="401">Invalid password</response>
        [Produces("application/json")]
        [ProducesResponseType(204)]
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
                        response = NoContent();
                    }
                    else
                    {
                        response = BadRequest();
                    }
                }
                else {
                    _logger.LogWarning(
                        "Unauthorized attempt to change the password of user "
                        + passwordChangeRequest.Uid
                    );
                    response = Unauthorized();
                }

                scope.Commit();
            }

            return response;
        }
    }
}
