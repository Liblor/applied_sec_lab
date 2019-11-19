using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using CertServer.DataModifiers;
using CoreCA.DataModel;

namespace CertServer.Controllers
{
    [ApiController, Route("api")]
    public class RevokeController : ControllerBase
    {
        private readonly CADBModifier _caDBModifier;
        private readonly UserDBAuthenticator _userDBAuthenticator;
        private readonly ILogger _logger;

        public RevokeController(
            CADBModifier caDBModifier,
            UserDBAuthenticator userDBAuthenticator,
            ILogger<RevokeController> logger
        )
        {
            _caDBModifier = caDBModifier;
            _userDBAuthenticator = userDBAuthenticator;
            _logger = logger;
        }

        /// <summary>
        /// Revoke a certificate.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/issue
        ///     {
        ///            "uid": "ab",
        ///            "password": "plain",
        ///            "SerialNumber": 1234
        ///     }
        ///
        /// </remarks>
        /// <param name="revokeRequest"></param>
        /// <response code="204">Certificate revoked</response>
        /// <response code="400">Bad request</response>
        [Produces("application/json")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [HttpPost("[controller]")]
        public IActionResult RevokeCertificate(RevokeRequest revokeRequest)
        {
            User user = _userDBAuthenticator.GetUser(revokeRequest.Uid);

            if (user != null)
            {
                _caDBModifier.RevokeAllCertificatesOfUser(user);
                return NoContent();
            }
            else
            {
                _logger.LogWarning(
                    string.Format(
                        "Failed to revoke certificates of user {0}; No such user found.",
                        revokeRequest.Uid
                    )
                );

                return BadRequest();
            }
        }
    }
}
