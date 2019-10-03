using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace WebServer.Controllers
{
    [Authorize]
    public class CertController : Controller
    {
        private readonly ILogger<CertController> _logger;

        public CertController(ILogger<CertController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // TODO: Enable XSRF protection for HttpPost endpoints if not present by default
        // TODO: Rate limit
        [HttpPost]
        public IActionResult New()
        {
            throw new NotImplementedException();
        }

        // TODO: consider replacing string argument with a model object
        [HttpPost]
        public IActionResult Revoke(string serialNumber)
        {
            throw new NotImplementedException();
        }
    }
}
