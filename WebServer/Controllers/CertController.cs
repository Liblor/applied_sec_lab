using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebServer.ViewModels;
using WebServer.Models;

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
        	// TODO Query certificates from the database.

			var valid = new HashSet<Certificate>();
			var revoked = new HashSet<Certificate>();
			var expired = new HashSet<Certificate>();

			var c = new Certificate
			{
				Fingerprint = "test",
				CreateDate = DateTime.Now,
				ExpireDate = DateTime.Now.Subtract(new TimeSpan(365, 0, 0, 0, 0)),
			};
			valid.Add(c);

            var viewModel = new CertificatesData()
            {
                Valid = valid,
                Revoked = revoked,
                Expired = expired,
            };

            return View(viewModel);
        }

        // TODO: Enable XSRF protection for HttpPost endpoints if not present by default
        // TODO: Rate limit
        [HttpPost]
        public IActionResult New()
        {
			// TODO Request certificate.

			bool success = false;

			if (success)
			{
        		ViewData["SuccessMessage"] = "Certificate issued successfully.";
			}
			else
			{
        		ViewData["ErrorMessage"] = "Issuing certificate failed.";
			}

            return View("Index");
        }

        // TODO: consider replacing string argument with a model object
        [HttpPost]
        public IActionResult Revoke(string serialNumber)
        {
			// TODO Revoke certificate.

			bool success = false;

			if (success)
			{
        		ViewData["SuccessMessage"] = "Certificate revoked successfully.";
			}
			else
			{
        		ViewData["ErrorMessage"] = "Revoking certificate failed.";
			}

            return View("Index");
        }
    }
}
