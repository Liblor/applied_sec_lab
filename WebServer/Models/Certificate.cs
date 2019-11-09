using System;
using System.Security.Cryptography.X509Certificates;

namespace WebServer.Models
{
    public class Certificate
    {
        public string Fingerprint { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ExpireDate { get; set; }

        public Certificate()
        {
        }

        public Certificate(CoreCA.DataModel.PublicCertificate cert)
        {
            var x509cert = cert.Parse();
            Fingerprint = x509cert.Thumbprint;
            ValidFrom = x509cert.NotBefore;
            ExpireDate = x509cert.NotAfter;
        }
    }
}
