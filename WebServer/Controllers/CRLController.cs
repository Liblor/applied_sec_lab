using CoreCA.Client;
using CoreCA.DataModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace WebServer.Controllers
{
    [Route("crl")]
    public class CRLController : Controller
    {
        private readonly CoreCAClient _coreCAClient;

        public CRLController(CoreCAClient coreCAClient)
        {
            _coreCAClient = coreCAClient;
        }

        [HttpGet, AllowAnonymous]
        [Route("revoked.crl")]
        [Produces(Constants.CrlMimeType)]
        [ResponseCache(Duration = Constants.CRLNextUpdatedIntervalMinutes * 60, Location = ResponseCacheLocation.Any)]
        public async Task<IActionResult> Get()
        {
            return Ok(await _coreCAClient.GetCRL());
        }
    }
}
