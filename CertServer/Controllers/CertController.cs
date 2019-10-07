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

	[ApiController, Route("api")]
	public class CertController : ControllerBase
	{
		[HttpGet]
		public Dictionary<string, string> Get()
		{
			var url = string.Format("{0}://{1}{2}", 
							Request.Scheme, 
							Request.Host, 
							Url.Action()
						);
			// XXX: Hacky (lots of love, C# noobs)
			return new Dictionary<string, string>
				{
					{"issue", url + "/issue"},
					{"revoke", url + "/revoke"},
					{"ciphersuites", url + "/ciphersuites"},
				};
		}
	}
}
