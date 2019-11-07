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
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace WebServer.Controllers
{
    [Authorize]
    public class CertController : Controller
    {
        private readonly CoreCAClient _client;
        private readonly ILogger<CertController> _logger;
        private readonly IMoviesCertContext _certContext;

        public CertController(CoreCAClient client, ILogger<CertController> logger, IMoviesCertContext certContext)
        {
            _client = client;
            _logger = logger;
            _certContext = certContext;
        }

        private CertificatesData FetchCertificatesData()
        {
            var valid = _certContext.PublicCertificates.AsEnumerable()
                .Where(c =>
                c.Parse().NotBefore <= DateTime.Now &&
                DateTime.Now <= c.Parse().NotAfter &&
                !c.IsRevoked)
                .Select(c => new Certificate(c));

            var revoked = _certContext.PublicCertificates.AsEnumerable()
                .Where(c => c.IsRevoked)
                .Select(c => new Certificate(c));

            var expired = _certContext.PublicCertificates.AsEnumerable()
                .Where(c =>
                DateTime.Now > c.Parse().NotAfter &&
                !c.IsRevoked)
                .Select(c => new Certificate(c));

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
			var viewModel = FetchCertificatesData();

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
                var viewModel = FetchCertificatesData();
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
                var viewModel = FetchCertificatesData();
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
