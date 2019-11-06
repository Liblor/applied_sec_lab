using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebServer.ViewModels;
using WebServer.Models;
using WebServer.Models.Cert;
using CoreCA.Client;
using System.Security.Claims;
using System.Net.Mime;

namespace WebServer.Controllers
{
    [Authorize]
    public class CertController : Controller
    {
        private readonly CoreCAClient _client;
        private readonly ILogger<CertController> _logger;

        public CertController(CoreCAClient client, ILogger<CertController> logger)
        {
            _client = client;
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

        [HttpGet]
        public async Task<IActionResult> Download()
        {
            string certb64 = null;
            // Try to see if a prior POST to Download or New cached a B64-encoded cert in TempData
            if (TempData.TryGetValue("CertB64", out object certb64Obj))
                certb64 = certb64Obj as string;

            if (certb64 == null)
            {
                TempData["ErrorMessage"] = "Certificate download failed.";
                return RedirectToAction(nameof(Index));
            }

            byte[] certBytes = Convert.FromBase64String(certb64);
            return File(certBytes, MediaTypeNames.Application.Octet, "certificate.pfx");
        }

        [HttpPost]
        public async Task<IActionResult> Download(DownloadCertDetails details)
        {
            if (!ModelState.IsValid)
            {
                var viewModel = await FetchCertificatesData();
                viewModel.DownloadCertDetails = details;

                TempData["ErrorMessage"] = "Certificate download failed.";

                return View(nameof(Index), viewModel);
            }

            string uid = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            string certb64 = await _client.DownloadPrivateKey(uid, details.Password);

            if (certb64 == null)
            {
                TempData["ErrorMessage"] = "New certificate request failed.";
                return RedirectToAction(nameof(Index));
            }

            TempData["SuccessMessage"] = "Certificate downloaded.";
            TempData["CertB64"] = certb64;

            TempData["AutoDownloadNewCert"] = Url.Action(nameof(Download), null, null, Request.Scheme);

            return RedirectToAction(nameof(Index));
        }

        // TODO: Enable XSRF protection for HttpPost endpoints if not present by default
        // TODO: Rate limit
        [HttpPost]
        public async Task<IActionResult> New(RequestNewCertDetails details)
        {
            // TODO: consider validating credentials here before making the request

            if (!ModelState.IsValid)
            {
                var viewModel = await FetchCertificatesData();
                viewModel.RequestNewCertDetails = details;

                TempData["ErrorMessage"] = "New certificate request failed.";

                return View(nameof(Index), viewModel);
            }

            string uid = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            string certb64 = await _client.RequestNewCertificate(uid, details.Password, details.Passphrase);

            if (certb64 == null)
            {
                TempData["ErrorMessage"] = "New certificate request failed.";
                return RedirectToAction(nameof(Index));
            }

            TempData["SuccessMessage"] = "Certificate issued successfully.";
            TempData["CertB64"] = certb64;
            TempData["AutoDownloadNewCert"] = Url.Action(nameof(Download), null, null, Request.Scheme);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Revoke()
        {
            string uid = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;

            bool success = await _client.RevokeCertificate(uid);

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
