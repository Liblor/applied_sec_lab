using Microsoft.AspNetCore.Mvc;
using System;

using CertServer.DataModifiers;
using CertServer.Models;

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
        /// <returns>Signed CRL in PEM format</returns>

        // XXX: Return type?
        [Produces("text/plain")]
        [ProducesResponseType(200)]
        [HttpGet("[controller]")]
        public IActionResult GetCRL()
        {
            return Ok(_caDBModifier.GenerateCRL());
        }
    }
}