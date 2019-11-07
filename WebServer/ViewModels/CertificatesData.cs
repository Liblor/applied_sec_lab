using System.Collections.Generic;
using WebServer.Models;
using WebServer.Models.Cert;

namespace WebServer.ViewModels
{
    public class CertificatesData
    {
        public IEnumerable<Certificate> Valid { get; set; }
        public IEnumerable<Certificate> Revoked { get; set; }
        public IEnumerable<Certificate> Expired { get; set; }

        public DownloadCertDetails DownloadCertDetails { get; set; }
        public RequestNewCertDetails RequestNewCertDetails { get; set; }
    }
}
