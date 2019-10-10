using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

        private static async Task<CertificatesData> FetchCertificatesData()
        {
            // TODO Query certificates from the database.

            var valid = new HashSet<Certificate>();
            var revoked = new HashSet<Certificate>();
            var expired = new HashSet<Certificate>();

            var c = new Certificate
            {
            	Id = 0,
                Fingerprint = "test",
                CreateDate = DateTime.Now,
                ExpireDate = DateTime.Now.Subtract(new TimeSpan(365, 0, 0, 0, 0)),
            };
            valid.Add(c);

            var data = new CertificatesData()
            {
                Valid = valid,
                Revoked = revoked,
                Expired = expired,
            };

			return data;
		}

        [HttpGet]
        public async Task<IActionResult> Index()
        {
			var viewModel = await FetchCertificatesData();

            return View(viewModel);
        }

        // TODO: Enable XSRF protection for HttpPost endpoints if not present by default
        // TODO: Rate limit
        [HttpPost]
        public async Task<IActionResult> New()
        {
            // TODO Request certificate.

            bool success = false;

            if (success)
            {
                TempData["SuccessMessage"] = "Certificate issued successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Issuing certificate failed.";
            }

            return RedirectToAction(nameof(Index));
        }

        // TODO: consider replacing string argument with a model object
        [HttpPost]
        public async Task<IActionResult> Revoke(int id)
        {
            // TODO Revoke certificate.

            bool success = false;

            if (success)
            {
                TempData["SuccessMessage"] = "Certificate revoked successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Revoking certificate failed.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
