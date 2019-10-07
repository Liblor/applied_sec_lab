using System;
using System.Net;
using System.Web;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CertServer.Models;

namespace CertServer.Controllers
{
	[ApiController, Route("api")]
    public class IssueController : ControllerBase
    {
		/// <summary>
		/// Request to issue a new certificate.
		/// </summary>
		/// <remarks>
		/// Sample request:
		///
		///     POST /api/issue
		///     {
		///        	"uid": "ab",
		///			"password": "plain",
		///			"cipherSuite": "TODO"
		///     }
		///
		/// </remarks>
		/// <param name="certRequest"></param>
		/// <returns>Private key as well as the certificate for the public key</returns>
		/// <response code="200">Certificate generation was successful</response>
		/// <response code="400">Certificate generation failed</response>
		[Produces("application/json")]
		[ProducesResponseType(200)]
		[ProducesResponseType(400)]
		[HttpPost("[controller]")]
		public CertRequest IssueCertificate(CertRequest certRequest)
		{
            return certRequest;
		}
    }
}