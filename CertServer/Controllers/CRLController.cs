using Microsoft.AspNetCore.Mvc;
using System;

using CertServer.DataModifiers;
using CertServer.Models;
using CoreCA.DataModel;

namespace CertServer.Controllers
{
    [ApiController, Route("api")]
    public class CRLController : ControllerBase
    {
        private readonly CADBModifier _caDBModifier;

        public CRLController(CADBModifier caDBModifier)
        {
            _caDBModifier = caDBModifier;
        }

        /// <summary>
        /// Get certificate revocation list.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /api/crl
        ///
        /// </remarks>
        /// <returns>Signed CRL in DER-encoded format</returns>

        // XXX: Return type?
        [Produces(Constants.CrlMimeType)]
        [ProducesResponseType(200)]
        [HttpGet("[controller]")]
        public IActionResult GetCRL()
        {
            return Ok(_caDBModifier.GenerateCRL());
        }
    }
}