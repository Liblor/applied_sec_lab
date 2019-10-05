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
    public class RevokeController : Controller
    {
		[HttpGet]
		public string Get()
		{
            return "hi";
		}
    }
}