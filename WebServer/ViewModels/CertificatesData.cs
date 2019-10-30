using System.Collections.Generic;
using WebServer.Models;
using WebServer.Models.Cert;

namespace WebServer.ViewModels
{
    public class CertificatesData
    {
        public HashSet<Certificate> Valid { get; set; }
        public HashSet<Certificate> Revoked { get; set; }
        public HashSet<Certificate> Expired { get; set; }

        public DownloadCertDetails DownloadCertDetails { get; set; }
        public RequestNewCertDetails RequestNewCertDetails { get; set; }
    }
}
