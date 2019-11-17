using CoreCA.DataModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using WebServer.ViewModels;

namespace WebServer.Controllers
{
    [Authorize(Policy = Constants.AdminPolicy)]
    public class AdminController : Controller
    {
        private readonly IMoviesCertContext _certContext;
        public AdminController(IMoviesCertContext certContext)
        {
            _certContext = certContext;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var stats = new AdminStats();
            stats.NumIssuedCertificates = _certContext.PublicCertificates.Count();
            stats.NumRevokedCertificates = _certContext.PublicCertificates.Count(c => c.IsRevoked);
            stats.CurrentSerialNum = _certContext.PublicCertificates
                .Select(c => c.SerialNr)
                .OrderByDescending(s => s)
                .FirstOrDefault();

            return View(stats);
        }
    }
}
