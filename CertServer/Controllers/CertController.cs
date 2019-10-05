using System;
using System.Net;
using System.Web;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CertServer.Controllers
{

	[ApiController, Route("api/[controller]")]
	public class CertController : ControllerBase
	{
		[HttpGet]
		public Dictionary<string, string> Get()
		{
			// XXX: Hacky (lots of love, C# noobs)
			return new Dictionary<string, string>
				{
					{"issue", "/api/issue"},
					{"revoke", "/api/revoke"},
					{"ciphersuites", "/api/ciphersuites"},
				};
		}
	}
}
