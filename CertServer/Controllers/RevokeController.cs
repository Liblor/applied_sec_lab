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
    public class RevokeController : ControllerBase
    {
		[HttpGet]
		public string Get()
		{
            return "hi";
		}
    }
}